using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Snog.InteractionSystem.ScriptableObjects;

namespace Snog.InteractionSystem.Factories
{
    public static class InteractionPromptFactory
    {
        private static Dictionary<string, InteractionPrompt> promptAssetMap;
        public static bool IsInitialized { get; private set; }

        public static InteractionPrompt GetPromptAsset(string typeName)
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            return promptAssetMap.TryGetValue(typeName, out var promptAsset) ? promptAsset : null;
        }

        private static void Initialize()
        {
            var registries = Resources.FindObjectsOfTypeAll<InteractionRegistry>();

            if (registries.Length == 0)
            {
                Debug.LogError("Interaction System Error: No InteractionRegistry asset found.");
                IsInitialized = false;
                return;
            }
            if (registries.Length > 1)
            {
                Debug.LogError("Interaction System Error: Multiple InteractionRegistry assets found.");
                IsInitialized = false;
                return;
            }

            var registry = registries[0];
            promptAssetMap = new Dictionary<string, InteractionPrompt>();
            foreach (var type in registry.interactionTypes)
            {
                if (type.prompt != null)
                {
                    promptAssetMap[type.typeName] = type.prompt;
                }
            }

            IsInitialized = true;
        }
    }
}
