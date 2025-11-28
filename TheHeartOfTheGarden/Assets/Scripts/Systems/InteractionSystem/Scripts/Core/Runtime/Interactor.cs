using UnityEngine;
using Snog.InteractionSystem.Core.Interfaces;

/// <summary>
/// Handles detection of interactable objects and manages interaction lifecycle.
/// </summary>

namespace Snog.InteractionSystem.Core.Runtime
{
    public class Interactor : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private Transform interactorSource;
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private LayerMask interactableLayer;

        private IInteractable currentInteractable;

        private void Update()
        {
            DetectInteractable();
            HandleInteractionInput();
        }

        private void DetectInteractable()
        {
            Ray ray = new(interactorSource.position, interactorSource.forward);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, interactRange, interactableLayer))
            {
                GameObject hitObject = hitInfo.collider.gameObject;

                if (hitObject.TryGetComponent(out IInteractable interactable))
                {
                    if (currentInteractable != interactable)
                    {
                        currentInteractable?.OnInteractExit();
                        currentInteractable = interactable;
                        currentInteractable.OnInteractEnter();
                        InteractionText.instance.textAppear.gameObject.SetActive(true);
                    }
                    return;
                }
            }

            ClearInteractable();
        }

        private void HandleInteractionInput()
        {
            if (currentInteractable == null) return;
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                Debug.Log("Interaction input detected.");
                currentInteractable.Interact();
            }
        }

        private void ClearInteractable()
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnInteractExit();
                currentInteractable = null;
                InteractionText.instance.textAppear.gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (interactorSource)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(interactorSource.position, interactorSource.forward * interactRange);
            }
        }
    }
}