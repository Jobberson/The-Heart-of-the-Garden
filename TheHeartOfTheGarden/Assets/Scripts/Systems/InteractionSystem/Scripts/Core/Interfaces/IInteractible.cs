namespace Snog.InteractionSystem.Core.Interfaces
{
    public interface IInteractable
    {
        void Interact(); // Called when we want to interact with the gameobject
        void OnInteractEnter(); // Called when detection with the object starts;
        void OnInteractExit();  // Called when detection with the object ends;
    }
}



