/***
 * Author: Gregorio Lozada
 * Created: 10/13/2018
 * 
 * This class handles all animator parameters as well as anything that
 * involves the sprite renderer for the player
 */ 

using UnityEngine;
using System.Collections;

public class PlayerAnimationManager : MonoBehaviour {

    private PlayerController pc;
    private Animator anim;

    private int flickerCounter;

    private string[] punchClipNames = new string[4];

    public int flickerDuration;

    public float normalizedTimeGrab;

	// Use this for initialization
	void Start () {
        pc = GetComponentInParent<PlayerController>();
        anim = GetComponent<Animator>();

        punchClipNames[0] = "RoboFighterJab";
        punchClipNames[1] = "RoboFighterStraight";
        punchClipNames[2] = "RoboFighterUppercut";
        punchClipNames[3] = "RoboFighterDropKick";

        flickerCounter = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Flip transform scale x depending on what direction the player is facing
        if (pc.IsFacingRight())
        {
            pc.transform.localScale = new Vector2(1.0f, 1.0f);
        }
        else
        {
            pc.transform.localScale = new Vector2(-1.0f, 1.0f);
        }
        
        // SET animator parameters
        anim.SetInteger("PunchIndex", pc.GetPunchIndex());
        anim.SetInteger("GrabbedObjects", pc.GetObjectsGrabbed().Count);
        anim.SetFloat("VelX", Mathf.Abs(pc.GetRigidbody2D().velocity.x));
        anim.SetFloat("VelY", pc.GetRigidbody2D().velocity.y);
        anim.SetBool("Grounded", pc.IsGrounded());
        anim.SetBool("Attack", pc.IsAttacking());
        anim.SetBool("Hit", pc.IsHit());
        anim.SetBool("Grab", pc.IsGrabbing());
        anim.SetBool("Throw", pc.IsThrowing());

        // IF the player is attacking
        if (anim.GetBool("Attack"))
        {
            // Play the player's current move's clip
            anim.Play(pc.GetCurrentMove().clip.name);

            // IF clip is a punch clip and clip has ended
            if (CurrentAnimationClipIsAPunchClip() && 
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                // The player is no longer punching
                pc.SetPunchClipEnd(true);
            }
        }

        // IF the  player is grabbing
        if (anim.GetBool("Grab"))
        {
            // IF the player is reaching
            if (pc.IsReachingGrab())
            {
                // Play grab reaching clip
                anim.Play("RoboFighterReach");
            }
            
            // IF the player is recovering from their grab
            if (pc.IsRecoveringGrab())
            {
                // IF the clip has ended
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    // The player is no longer recovering from their grab
                    pc.SetGrabRecover(false);
                }
            }
        }

        // IF player has been hit
        if (anim.GetBool("Hit"))
        {
            // Start flickering
            Flicker();
        }
        else
        {
            // SET flciker counter to zero
            flickerCounter = 0;
            // SET sprite renderer to enabled
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    // Thsi handle the flicker effect when the player is hit by switching the sprite renderer on and off
    private void Flicker()
    {
        // Increment flicker counter
        flickerCounter++;

        // IF counter is equal to the duration of the flicker
        if (flickerCounter == flickerDuration)
        {
            // SET flicker coutner to zero
            flickerCounter = 0;

            // IF sprite renderer is enabled
            if (GetComponent<SpriteRenderer>().enabled)
            {
                // Disable the sprite renderer
                GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                // Enable the sprite renderer
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }

    // Returns true if the current animation playing is a punch clip
    private bool CurrentAnimationClipIsAPunchClip()
    {
        foreach (string punchClip in punchClipNames)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(punchClip))
            {
                return true;
            }
        }

        return false;
    }
    
    // Changes a move's knockback depending on the index passed
    public void ChangeKnockBackEvent(int index)
    {
        pc.GetCurrentMove().ChangeKnockBack(index);
    }

    // When the player is throwing an object
    public void ThrowObjectEvent()
    {
        pc.RemoveObjectsFromGrabbedObjectsList();
    }

    // When the player is no longer reaching to grab an object
    public void EndReachEvent()
    {
        pc.SetGrabReach(false);

        anim.Play("RoboFighterGrab", 0, normalizedTimeGrab);
    }

    // When the player is no longer recovering from their grab
    public void EndRecoveryEvent()
    {
        if (pc.GetObjectsGrabbed().Count == 0)
        {
            pc.SetGrabRecover(false);
        }
    }

    // When the throwing animation clip has ended
    public void EndThrowEvent()
    {
        pc.SetThrowing(false);
    }
}
