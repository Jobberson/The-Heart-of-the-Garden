using UnityEngine;
using Snog.InteractionSystem.Core.Interfaces;
using Snog.InteractionSystem.Factories;

namespace Snog.InteractionSystem.Core.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class InteractibleObj : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [Tooltip("The name of the interaction type (must match the name in the registry).")]
        [SerializeField] private string interactionTypeName = "Unknown";

        [Header("Outline Settings")]
        [Tooltip("Enable outline handling for this interactible.")]
        public bool useOutline = true;
        
        [Tooltip("If true and no Outline component is found on the object or children, add one to this GameObject.")]
        [SerializeField] private bool addOutlineIfMissing = false;

        private IInteractionBehavior interactionBehavior;
        private string promptText;

        // Typed handle to Quick Outline component
        private Outline outline;

        private void Start()
        {
            interactionBehavior = InteractionBehaviorFactory.GetBehavior(interactionTypeName);

            var promptAsset = InteractionPromptFactory.GetPromptAsset(interactionTypeName);
            promptText = promptAsset != null ? promptAsset.GetFormattedPrompt() : "";

            if(useOutline)
            {
                if (outline == null && addOutlineIfMissing)
                {
                    outline = gameObject.AddComponent<Outline>();
                }

                outline = GetComponentInChildren<Outline>();
                outline.enabled = false;
            }
        }

        public void Interact()
        {
            interactionBehavior?.Execute(gameObject);
        }

        public void OnInteractEnter()
        {
            InteractionText.instance.SetText(promptText);
            if (useOutline && outline != null)
                outline.enabled = true;
        }

        public void OnInteractExit()
        {
            InteractionText.instance.SetText("");
            if (useOutline && outline != null)
                outline.enabled = false;
        }
    }
}
