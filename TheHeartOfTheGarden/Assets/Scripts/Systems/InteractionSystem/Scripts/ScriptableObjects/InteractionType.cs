using UnityEditor;
using UnityEngine;

namespace Snog.InteractionSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Interaction System/Interaction Type")]
    public class InteractionType : ScriptableObject
    {
        public string typeName;
        public MonoScript behaviorScript;
        public InteractionPrompt prompt;
    }
}