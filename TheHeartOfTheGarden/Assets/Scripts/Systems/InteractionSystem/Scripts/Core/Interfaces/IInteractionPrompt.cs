using UnityEngine;

namespace Snog.InteractionSystem.Core.Interfaces
{
    public interface IInteractionPrompt
    {
        string GetPrompt(KeyCode key);
    }
}