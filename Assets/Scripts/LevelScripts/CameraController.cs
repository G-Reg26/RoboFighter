/***
 * Author: Gregorio Lozada
 * Created: 10/11/2018
 * 
 * This class handles the main camera
 */ 

using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    private PlayerController player;
    private Vector3 targetPosition;

    private float playerYDistance;
    private float yPos;

    public Vector2 xBounds;

    private float speed;

    public float maxYDistance;

    public float cameraSpeed;

	// Use this for initialization
	void Start () {
        player = FindObjectOfType<PlayerController>();
        yPos = transform.position.y - player.transform.position.y;
	}
	
	// Update is called once per frame
    // LateUpdate is called after all regular updates have been implemented
	void LateUpdate () {
        // IF the player is between the x bounds
        if (player.transform.position.x > xBounds.x && player.transform.position.x < xBounds.y)
        {
            // SET x position of camera to player's x position
            transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }
        else
        {
            // Depending on which bound is closest SET the camera's x position to that bound
            if (player.transform.position.x >= xBounds.y)
            {
                transform.position = new Vector3(xBounds.y, transform.position.y, transform.position.z);
            }
            else if (player.transform.position.x <= xBounds.x)
            {
                transform.position = new Vector3(xBounds.x, transform.position.y, transform.position.z);
            }
        }
                
        // GET distance between player and camera's y position
        playerYDistance = player.transform.position.y - transform.position.y;

        // SET camera speed
        speed = cameraSpeed;

        // IF the player is in the air
        if (!player.IsGrounded())
        {
            // IF the player's going up AND exceeds the max y distance from camera
            if (player.GetRigidbody2D().velocity.y > 0.0f && playerYDistance > maxYDistance)
            {
                // SET target position to player's position
                targetPosition = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
            }
            // IF the player's going down
            else if (player.GetRigidbody2D().velocity.y < 0.0f)
            {
                // IF the player has not been hit
                if (!player.IsHit())
                {
                    // SET the target position to below the player's position
                    targetPosition = new Vector3(transform.position.x, player.transform.position.y - yPos, transform.position.z);

                    // Boost the camera speed
                    speed *= 1.5f;
                }
                else
                {
                    // SET the target position to above the player's position
                    targetPosition = new Vector3(transform.position.x, player.transform.position.y + yPos, transform.position.z);
                }
            }
        }
        else
        {
            // SET the target position to above the player's position
            targetPosition = new Vector3(transform.position.x, player.transform.position.y + yPos, transform.position.z);

            // Boost the camera speed
            speed *= 1.5f;
        }

        // Move towards target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
	}
}
