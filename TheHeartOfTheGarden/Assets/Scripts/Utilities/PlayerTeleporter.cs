using UnityEngine;

public class PlayerTeleporter : MonoBehaviour
{
    public Transform teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        var playerDetector = other.GetComponentInParent<PlayerTriggerDetector>();
        if (playerDetector != null && teleportTarget != null)
        {
            // Get CharacterController from root player
            var controller = other.GetComponentInParent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;

                // Move the player root to the teleport target
                var playerRoot = controller.transform;
                playerRoot.SetPositionAndRotation(teleportTarget.position, teleportTarget.rotation);

                controller.enabled = true;
                Physics.SyncTransforms();
            }
        }
    }
}
