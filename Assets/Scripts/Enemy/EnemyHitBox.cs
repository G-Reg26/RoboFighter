/***
 * Author: Gregorio Lozada
 * Date Created: 10/21/2018
 * 
 * This is the class that controls an enemy's hit box. Handles knock back and
 * collision events.
 **/

using UnityEngine;
using System.Collections;

public class EnemyHitBox : MonoBehaviour {

    private Grunt grunt;
    private Animator anim;

    public Vector3[] knockBack;
    public Vector3 currentKnockBack;

    // Use this for initialization
    void Start() {
        // GET componenets from parents
        grunt = transform.parent.GetComponentInParent<Grunt>();
        anim = GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // When enemy is attacking
        if (anim.GetBool("Attack"))
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch3"))
            {
                currentKnockBack = knockBack[1];
            }
            else
            {
                currentKnockBack = knockBack[0];
            }
        }
    }

    // When trigger box collides with another collider
    void OnTriggerEnter2D(Collider2D other)
    {
        // IF the collider belongs to a game object with the tag "Player"
        if (other.CompareTag("Player"))
        {
            // IF the player is not already hit
            if (!other.GetComponentInParent<PlayerController>().IsHit())
            {
                // IF the enemy's to the right of the enemy
                if (grunt.transform.position.x - other.transform.position.x > 0.0f)
                {
                    // Hit the player and knock it back in the negative x direction with the current move's knock back speed
                    other.GetComponentInParent<PlayerController>().Hit(new Vector2(-currentKnockBack.x, currentKnockBack.y));
                    // The player is now facing right
                    other.GetComponentInParent<PlayerController>().SetFacingRight(true);
                }
                else
                {
                    // Hit the player and knock it back in the positive x direction with the current move's knock back speed
                    other.GetComponentInParent<PlayerController>().Hit(currentKnockBack);
                    // The player is now facing left
                    other.GetComponentInParent<PlayerController>().SetFacingRight(false);
                }
            }
        }
    }
}
