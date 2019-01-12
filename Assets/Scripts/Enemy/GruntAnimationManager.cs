/***
 * Author: Gregorio Lozada
 * Date Created: 11/11/2018
 * 
 * This class handles all animator parameters as well as anything that
 * involves the sprite renderer for the enemy
 */

using UnityEngine;
using System.Collections;

public class GruntAnimationManager : MonoBehaviour {

    private Grunt grunt;
    private Animator anim;

    private int flickerCounter;

    public int flickerDuration;

    // Use this for initialization
    void Start () {
        grunt = GetComponentInParent<Grunt>();
        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        if (grunt.FacingRight())
        {
            grunt.transform.localScale = new Vector2(-1.0f, 1.0f);
        }
        else
        {
            grunt.transform.localScale = new Vector2(1.0f, 1.0f);
        }

        anim.SetInteger("CurrentState", (int)grunt.GetCurrentState());
        anim.SetFloat("VelX", Mathf.Abs(grunt.GetRigidbody2D().velocity.x));
        anim.SetBool("Grounded", grunt.IsGrounded());
        anim.SetBool("Attack", grunt.IsAttacking());
        anim.SetBool("Hit", grunt.IsHit());

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("GruntHit"))
        {
            Flicker();
        }
        else
        {
            flickerCounter = 0;
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void Flicker()
    {
        flickerCounter++;

        if (flickerCounter == flickerDuration)
        {
            flickerCounter = 0;

            if (GetComponent<SpriteRenderer>().enabled)
            {
                GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }
}
