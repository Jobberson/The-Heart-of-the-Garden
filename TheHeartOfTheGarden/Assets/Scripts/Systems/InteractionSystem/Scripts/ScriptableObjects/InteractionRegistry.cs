using UnityEngine;
using System.Collections.Generic;

namespace Snog.InteractionSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Interaction System/Registry")]
    public class InteractionRegistry : ScriptableObject
    {
        [Tooltip("List of all available interaction types")]
        public List<InteractionType> interactionTypes = new();
    }
}