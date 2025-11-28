using UnityEngine;

namespace Snog.InteractionSystem.Core.Interfaces
{
    public interface IInteractionBehavior
    {
        void Execute(GameObject interactable);
    }
}