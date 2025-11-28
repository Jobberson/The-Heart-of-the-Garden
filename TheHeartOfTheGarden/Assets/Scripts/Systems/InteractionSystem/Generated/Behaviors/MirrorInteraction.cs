using UnityEngine;
using Snog.InteractionSystem.Core.Interfaces;
using SpaceFusion.SF_Portals.Scripts;

namespace Snog.InteractionSystem.Behaviors
{
    public class MirrorInteraction : MonoBehaviour, IInteractionBehavior
    {
        public void Execute(GameObject target)
        {
            if(target.TryGetComponent(out MirrorRotator mirror))
            {
                mirror.interacting = !mirror.interacting;
                var player = FindAnyObjectByType<PlayerController>();
                player._controlsEnabled = !player._controlsEnabled;
            }
        }
    }
}