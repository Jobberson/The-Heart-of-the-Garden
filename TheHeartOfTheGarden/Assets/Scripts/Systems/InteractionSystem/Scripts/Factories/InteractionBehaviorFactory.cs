using UnityEngine;
using System.Collections.Generic;
using Snog.InteractionSystem.Core.Interfaces;
using Snog.InteractionSystem.ScriptableObjects;

namespace Snog.InteractionSystem.Factories
{
    public static class InteractionBehaviorFactory
    {
        private static Dictionary<string, IInteractionBehavior> behaviorMap;
        public static bool IsInitialized { get; private set; }

        public static IInteractionBehavior GetBehavior(string typeName)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            return behaviorMap.TryGetValue(typeName, out var behavior) ? behavior : null;
        }

        private static void Initialize()
        {
            var registries = Resources.FindObjectsOfTypeAll<InteractionRegistry>();

            if (registries.Length == 0)
            {
                Debug.LogError("Interaction System Error: No InteractionRegistry asset found in the project. Please create one.");
                IsInitialized = false;
                return;
            }

            if (registries.Length > 1)
            {
                Debug.LogError("Interaction System Error: Multiple InteractionRegistry assets found. Please ensure there is only one to avoid ambiguity.");
                IsInitialized = false;
                return;
            }

            var registry = registries[0];
            behaviorMap = new Dictionary<string, IInteractionBehavior>();

            foreach (var type in registry.interactionTypes)
            {
                if (type.behaviorScript != null)
                {
                    var behaviorType = type.behaviorScript.GetClass();
                    if (behaviorType != null && typeof(IInteractionBehavior).IsAssignableFrom(behaviorType))
                    {
                        var container = new GameObject($"__{type.typeName}_Behavior") { hideFlags = HideFlags.HideAndDontSave };
                        var component = container.AddComponent(behaviorType) as MonoBehaviour;
                        if (component is IInteractionBehavior behavior)
                        {
                            behaviorMap[type.typeName] = behavior;
                        }
                    }
                }
            }

            IsInitialized = true;
        }
    }
}
