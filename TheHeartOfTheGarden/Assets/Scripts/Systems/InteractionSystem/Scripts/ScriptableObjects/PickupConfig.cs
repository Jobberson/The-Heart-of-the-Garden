using UnityEngine;

namespace Snog.InteractionSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Interaction System/Pickup/Pickup Config")]
    public class PickupConfig : ScriptableObject
    {
        [Header("Hold point (local offset from Camera)")]
        public Vector3 holdLocalPosition = new(0f, -0.2f, 1.2f);
        public Vector3 holdLocalEuler = Vector3.zero;

        [Header("Physics")]
        public float throwForce = 6f;
        public float jointBreakForce = 1000f;
        public float jointBreakTorque = 1000f;
        public bool ignorePlayerCollision = true;

        // Optional defaults (can be edited by designers)
        public RigidbodyInterpolation defaultInterpolation = RigidbodyInterpolation.Interpolate;
    }
}
