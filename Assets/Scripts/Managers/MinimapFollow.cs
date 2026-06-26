using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    private Transform player;

    void LateUpdate()
    {
        // Lazily find the player if we don't have them yet
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                // Return early so we don't throw errors while waiting for the player to exist
                return;
            }
        }

        // Target movement code (Runs perfectly after finding the player)
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;

        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}