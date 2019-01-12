/***
 * Author: Gregorio Lozada
 * Date Created: 10/12/2018
 * 
 * This is the class that controls the player's hit box. Handles knock back and
 * collision events.
 **/

using UnityEngine;
using System.Collections;

public class PlayerHitBox : MonoBehaviour {

    private PlayerController pc;
    private PlayerAnimationManager pAnim;

	// Use this for initialization
	void Start () {
        // GET componenets from parents
        pc = transform.parent.GetComponentInParent<PlayerController>();
        pAnim = transform.GetComponentInParent<PlayerAnimationManager>();
	}

    // When trigger box collides with another collider
    void OnTriggerEnter2D(Collider2D other)
    {
        // IF the collider belongs to a game object with the tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            // IF the enemy is not already hit
            if (pc.IsAttacking() && other.GetComponentInParent<Grunt>().CanBeHit())
            {
                // IF the player's to the right of the enemy
                if (pc.transform.position.x - other.transform.position.x > 0.0f)
                {
                    // Hit the enemy and knock it back in the negative x direction with the current move's knock back speed
                    other.GetComponentInParent<Grunt>().Hit(new Vector2(-pc.GetCurrentMove().knockBack.x, pc.GetCurrentMove().knockBack.y));
                }
                else
                {
                    // Hit the enemy and knock it back in the positive x direction with the current move's knock back speed
                    other.GetComponentInParent<Grunt>().Hit(pc.GetCurrentMove().knockBack);
                }

                //The player has hit something
                pc.SetHitObject(true);
            }
            // IF the player is grabbing AND currently has not grabbed any objects AND the enemy is not currently grabbed
            else if (pc.IsGrabbing() && pc.GetObjectsGrabbed().Count == 0 && !other.GetComponentInParent<Grunt>().IsGrabbed())
            {
                // Grab the enemy
                other.GetComponent<Grunt>().Grabbed();

                // SET enemy's parent to the player's hit box and relocate it to the hit box's position
                other.transform.parent = gameObject.transform;
                other.transform.localPosition = new Vector2(0.0f, 0.0f);

                // Add enemy to list of objects grabbed by the player
                pc.AddToGrabbedObjectsList(other.gameObject);
                
                // The player is no longer reaching
                pAnim.EndReachEvent();
            }
        }
    }
}
