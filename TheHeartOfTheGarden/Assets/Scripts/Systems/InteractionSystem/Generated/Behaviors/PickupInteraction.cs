using System.Collections.Generic;
using UnityEngine;
using Snog.InteractionSystem.Core.Interfaces;
using Snog.InteractionSystem.Core.Runtime;
using SpaceFusion.SF_Portals.Scripts; // for Portable

namespace Snog.InteractionSystem.Behaviors
{
    public class PickupInteraction : MonoBehaviour, IInteractionBehavior
    {
        [SerializeField] private float holdDistance = 2.0f;
        [SerializeField] private float maxPickupDistance = 3.5f;
        [SerializeField] private float throwForce = 8f;
        [SerializeField] private bool centerOnHoldPoint = true;

        private Transform holdPoint;
        private GameObject heldObject;
        private Rigidbody heldRb;
        private Portable heldPortable; // optional, the Portable component of the held object

        private readonly Dictionary<Rigidbody, (bool isKinematic, bool useGravity, float drag, float angularDrag, RigidbodyInterpolation interpolation, CollisionDetectionMode ccd, RigidbodyConstraints constraints)> rbState
            = new();

        private void Awake() => EnsureHoldPoint();

        private void EnsureHoldPoint()
        {
            var existing = GameObject.Find("HoldPoint");
            if (existing != null) { holdPoint = existing.transform; return; }

            var cam = Camera.main;
            var parent = cam != null ? cam.transform : transform;
            var go = new GameObject("HoldPoint");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.forward * holdDistance;
            go.transform.localRotation = Quaternion.identity;
            holdPoint = go.transform;
        }

        public void Execute(GameObject target)
        {
            if (target == null) return;

            if (heldObject != null) { Drop(); return; }

            TryPickup(target);
        }

        private void TryPickup(GameObject target)
        {
            if (target == null) return;

            if (!target.TryGetComponent<Rigidbody>(out var rb))
            {
                Debug.Log("[PickupInteraction] Can't pick up — target has no Rigidbody: " + target.name);
                return;
            }

            if (rb.gameObject.isStatic)
            {
                Debug.Log("[PickupInteraction] Can't pick up static object: " + target.name);
                return;
            }

            if (holdPoint != null && Vector3.Distance(holdPoint.position, target.transform.position) > maxPickupDistance)
            {
                Debug.Log("[PickupInteraction] Too far to pick up: " + target.name);
                return;
            }

            if (!rbState.ContainsKey(rb))
            {
                rbState[rb] = (rb.isKinematic, rb.useGravity, rb.linearDamping, rb.angularDamping, rb.interpolation, rb.collisionDetectionMode, rb.constraints);
            }

            heldObject = target;
            heldRb = rb;

            // detect Portable if present and inform it
            if (heldObject.TryGetComponent<Portable>(out var portable))
            {
                heldPortable = portable;
                heldPortable.OnPickedUp(this);
            }
            else heldPortable = null;

            // prepare for hold: make kinematic so transform parenting is stable
            heldRb.isKinematic = true;
            heldRb.useGravity = false;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.interpolation = RigidbodyInterpolation.None;
            heldRb.constraints = RigidbodyConstraints.None;

            // parent to holdPoint
            heldObject.transform.SetParent(holdPoint, worldPositionStays: true);

            if (centerOnHoldPoint)
            {
                heldObject.transform.position = holdPoint.position;
                heldObject.transform.rotation = holdPoint.rotation;
            }

            if (heldObject.TryGetComponent(out Outline ol)) ol.enabled = false;
            if (heldObject.TryGetComponent(out InteractibleObj io)) io.useOutline = false;

            Debug.Log("[PickupInteraction] Picked up: " + heldObject.name);
        }

        public void Drop()
        {
            if (heldObject == null || heldRb == null) return;

            // unparent
            heldObject.transform.SetParent(null, worldPositionStays: true);

            if (rbState.TryGetValue(heldRb, out var saved))
            {
                heldRb.isKinematic = saved.isKinematic;
                heldRb.useGravity = saved.useGravity;
                heldRb.linearDamping = saved.drag;
                heldRb.angularDamping = saved.angularDrag;
                heldRb.interpolation = saved.interpolation;
                heldRb.collisionDetectionMode = saved.ccd;
                heldRb.constraints = saved.constraints;
                rbState.Remove(heldRb);
            }
            else
            {
                heldRb.isKinematic = false;
                heldRb.useGravity = true;
                heldRb.linearDamping = 0f;
                heldRb.angularDamping = 0.05f;
            }

            if (heldPortable != null) { heldPortable.OnDropped(); heldPortable = null; }

            if (heldObject.TryGetComponent(out Outline ol)) ol.enabled = true;
            if (heldObject.TryGetComponent(out InteractibleObj io)) io.useOutline = true;

            Debug.Log("[PickupInteraction] Dropped: " + heldObject.name);
            heldObject = null;
            heldRb = null;
        }

        public void Throw(float multiplier = 1f)
        {
            if (heldObject == null || heldRb == null) return;

            // unparent and re-enable physics
            heldObject.transform.SetParent(null, worldPositionStays: true);

            if (rbState.TryGetValue(heldRb, out var saved))
            {
                heldRb.isKinematic = false;
                heldRb.useGravity = true;
                heldRb.linearDamping = Mathf.Max(0.01f, saved.drag);
                heldRb.angularDamping = Mathf.Max(0.01f, saved.angularDrag);
                heldRb.interpolation = saved.interpolation;
                heldRb.collisionDetectionMode = saved.ccd;
                heldRb.constraints = saved.constraints;
                rbState.Remove(heldRb);
            }
            else
            {
                heldRb.isKinematic = false;
                heldRb.useGravity = true;
            }

            // apply throw force using camera forward (or transform.forward fallback)
            var cam = Camera.main;
            var dir = cam != null ? cam.transform.forward : transform.forward;
            heldRb.AddForce(dir * throwForce * multiplier, ForceMode.VelocityChange);

            if (heldPortable != null) { heldPortable.OnDropped(); heldPortable = null; }

            if (heldObject.TryGetComponent(out Outline ol)) ol.enabled = true;
            if (heldObject.TryGetComponent(out InteractibleObj io)) io.useOutline = true;

            Debug.Log("[PickupInteraction] Threw: " + heldObject.name + " force: " + (throwForce * multiplier));
            heldObject = null;
            heldRb = null;
        }

        private void FixedUpdate()
        {
            if (holdPoint != null)
            {
                var cam = Camera.main;
                var parent = cam != null ? cam.transform : transform;
                Vector3 targetPos = parent.TransformPoint(Vector3.forward * holdDistance);
                holdPoint.SetPositionAndRotation(Vector3.Lerp(holdPoint.position, targetPos, 25f * Time.fixedDeltaTime), Quaternion.Slerp(holdPoint.rotation, parent.rotation, 25f * Time.fixedDeltaTime));
            }
        }

        // Called via reflection by Portable.NotifyTeleported( ) when the portal teleports the object.
        // Portable passes itself, so we re-connect to the teleported instance (which may be the same GameObject).
        // Signature intentionally matches Portable.NotifyTeleported reflection attempt.
        public void OnPortableTeleported(Portable p)
        {
            if (p == null) return;

            // If the held object got teleported, update our references to the new instance
            if (heldObject == null) return;

            // If the portable's GameObject isn't the one we hold, it might be a clone/other instance; handle by trying to find matching name or reference.
            if (p.gameObject != heldObject && p.portableObject != heldObject && p.portableObject != null)
            {
                // The portal system might have swapped instances; we take the portable's main GameObject as the new held object.
                Debug.Log("[PickupInteraction] Held object was replaced via portal. Rebinding to new instance: " + p.gameObject.name);
            }

            // Rebind to the teleported object (p.gameObject) and make sure it's parented to holdPoint and kept kinematic.
            heldObject = p.gameObject;
            heldRb = heldObject.GetComponent<Rigidbody>();

            if (heldRb != null)
            {
                // store original state if not stored
                if (!rbState.ContainsKey(heldRb))
                    rbState[heldRb] = (heldRb.isKinematic, heldRb.useGravity, heldRb.linearDamping, heldRb.angularDamping, heldRb.interpolation, heldRb.collisionDetectionMode, heldRb.constraints);

                heldRb.isKinematic = true;
                heldRb.useGravity = false;
                heldRb.linearVelocity = Vector3.zero;
                heldRb.angularVelocity = Vector3.zero;
                heldRb.interpolation = RigidbodyInterpolation.None;
                heldRb.constraints = RigidbodyConstraints.None;
            }

            // Reparent and snap transform so the player continues holding the object seamlessly.
            heldObject.transform.SetParent(holdPoint, worldPositionStays: true);
            heldObject.transform.position = holdPoint.position;
            heldObject.transform.rotation = holdPoint.rotation;
        }
    }
}
