using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceFusion.SF_Portals.Scripts {
    public class MainCamera : MonoBehaviour {

        private Portal[] _portals;
        private Camera _playerCamera;

        
        /// <summary>
        /// this actually fixes some weird flickering of the portal when the player goes through the portal sideways
        /// So instead of rendering the RenderTextures directly in the Update function of the Portal, we control it from the main camera for all the listed portals 
        /// </summary>
        private void Awake() {
            _playerCamera = GetComponent<Camera>();
            _portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
             foreach (var portal in _portals) {
                 portal.playerCamera = _playerCamera;
             }
            RenderPipelineManager.beginCameraRendering += RenderPortal;
        }

        private void OnDestroy() {
            RenderPipelineManager.beginCameraRendering -= RenderPortal;
        }

        private void RenderPortal(ScriptableRenderContext context, Camera cam) {
            foreach (var portal in _portals) {
                if (portal == null) continue;
                portal.Render(context, cam);
            }
        }

    }
}