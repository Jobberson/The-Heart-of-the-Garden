using Snog.InteractionSystem.Core.Interfaces;
using UnityEngine;

namespace Snog.InteractionSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Interaction System/Prompt")]
    public class InteractionPrompt : ScriptableObject, IInteractionPrompt
    {
        [Tooltip("Prompt text shown to the player")]
        public string promptText;

        [Tooltip("Optional key to show in the prompt")]
        public KeyCode interactionKey = KeyCode.E;

        public string GetFormattedPrompt()
        {
            return promptText.Replace("{key}", interactionKey.ToString());
        }

        public string GetPrompt(KeyCode key)
        {
            return GetFormattedPrompt();
        }
    }
}