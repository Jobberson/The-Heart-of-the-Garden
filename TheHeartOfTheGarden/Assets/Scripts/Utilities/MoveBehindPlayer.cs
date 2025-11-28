using UnityEngine;

/// <summary>
/// Moves this GameObject behind the player and rotates it to face them horizontally (no vertical tilt).
/// </summary>
public class MoveBehindPlayer : MonoBehaviour
{
    [Tooltip("The player to position behind.")]
    public Transform player;

    [Tooltip("How far behind the player to place this object.")]
    public float distanceBehind = 2f;

    [Tooltip("Vertical offset relative to the player's position.")]
    public float heightOffset = 0f;
    private bool isMoved = false;

    /// <summary>
    /// Moves this object behind the player and makes it face them (flat rotation).
    /// </summary>
    public void MoveBehind()
    {
        if(isMoved)
            return;
            
        if(!player)
        {
            Debug.LogWarning($"{nameof(MoveBehindPlayer)}: No player assigned!");
            return;
        }

        // Move behind player
        Vector3 targetPosition = player.position - player.forward * distanceBehind + Vector3.up * heightOffset;
        transform.position = targetPosition;

        // Rotate to face the player horizontally only
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f; // ignore vertical difference
        if (lookDirection.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(lookDirection);
        
        isMoved = true;
    }
}
