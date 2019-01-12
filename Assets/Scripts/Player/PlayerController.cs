/***
 * Author: Gregorio Lozada
 * Created: 10/8/2018
 * 
 * This is the class that controls the Player's behavior.
 * Functions such as movement and combat are handled in this class.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    public enum PunchTier
    {
        LIGHT1,
        LIGHT2,
        MED,
        HEAVY
    }

    private Transform spriteRenderer;
    private Transform hitbox;

    public List<GameObject> objectsGrabbed;

    private Rigidbody2D rb;
    private Coroutine currentCoroutine;
    private Move currentMove;
    private Vector2 negativeXDir;

    private int punchIndex;

    // Movement booleans
    private bool grounded;
    private bool facingRight;

    // Combat booleans
    private bool attack;
    private bool hit;
    private bool hitObject;
    private bool punchClipEnd;

    private bool grabbing;
    private bool grabReach;
    private bool grabRecover;

    private bool throwing;
    
    public Move[] moves;
    public Move airMove;

    // Physics box parameters
    public Transform feet;
    public LayerMask whatIsGround;

    public Vector2 groundCheckSize;

    public float minJumpHeightOffset;

    // Movement speeds
    public float runSpeed;
    public float jumpSpeed;

    public Vector2 throwSpeed;
    public Vector2 releaseSpeed;

    // Combat durations
    public float punchBuffer;
    public float hitDur;

    public float grabTimer;
    public float grabDuration;

    // Use this for initialization
    void Start () {
        // Get components
        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = transform.GetChild(0);
        hitbox = spriteRenderer.GetChild(0);

        // Set negative x direction vector
        negativeXDir = new Vector2(-1.0f, 1.0f);

        punchIndex = 0;

        // Set movement booleans
        facingRight = true;
        grounded = true;

        // Set combat booleans
        attack = false;
        hit = false;
        hitObject = false;
        punchClipEnd = false;

        grabTimer = 0.0f;
        
        // Set moveset
        moves[(int)PunchTier.LIGHT1].Start();
        moves[(int)PunchTier.LIGHT2].Start();
        moves[(int)PunchTier.MED].Start();
        moves[(int)PunchTier.HEAVY].Start();

        airMove.Start();
    }

    // Update is called once per frame
    void Update () {
        CheckPhysBoxes();

        // IF the player is attacking in the air
        if (attack && currentMove.IsAirMove())
        {
            // IF the player is grounded
            if (grounded)
            {
                // Stop current couroutine
                StopCoroutine(currentCoroutine);

                // The player is no longer attacking
                attack = false;
            }
            else
            {
                // IF y velocity is between 0 and the difference of the jump speed and minimum jump height offset
                if (rb.velocity.y > 0.0f && rb.velocity.y < jumpSpeed - minJumpHeightOffset)
                    // SET y velocity to 0
                    rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            }
        }

        // IF the player has not been hit
        if (!hit)
        {
            // IF the player is not attacking AND is not throwing AND
            // is not grabbing OR 
            // is not attacking AND has grabbed an object OR 
            // is not attacking AND is grabbing in the air
            if (!attack && !throwing && (!grabbing || objectsGrabbed.Count > 0 || (grabbing && !grounded)))
            {
                Movement();
            }

            // IF the players is not grabbing AND throwing
            if (!grabbing && !throwing)
            {
                Attack();
            }

            // IF the player is not attacking
            if (!attack)
            {
                Grab();
            }
        }
	}

    // This checks if the player is grounded
    private void CheckPhysBoxes()
    {
        // IF physics box overlaps with an object in a layer specified in the whatIsGround layermask
        if (Physics2D.OverlapBox(feet.position, groundCheckSize, 0.0f, whatIsGround))
        {
            // The enemy is grounded
            grounded = true;
        }
        else
        {
            // The enemy is not grounded (In the air)
            grounded = false;
        }
    }

    private void Movement()
    {
        // IF the player is not grabbing in the air
        if (!(grabbing && !grounded))
        {
            // IF the player is running in the positive x direction
            if (Input.GetAxisRaw("Horizontal") > 0.0f)
            {
                // Player is facing right
                facingRight = true;

                // IF the player is not grabbing
                if (!grabbing)
                {
                    // SET x velocity to positive runspeed
                    rb.velocity = new Vector2(runSpeed, rb.velocity.y);
                }
            }
            // IF the player is running in the negative x direction
            else if (Input.GetAxisRaw("Horizontal") < 0.0f)
            {
                // The player is not facing right
                facingRight = false;

                // IF the player is not grabbing
                if (!grabbing)
                {
                    // SET x velocity to negative runspeed
                    rb.velocity = new Vector2(-runSpeed, rb.velocity.y);
                }
            }
            else
            {
                // SET velocity to 0
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
            }
        }

        // IF the player is grounded AND the jump button has been pressed
        if (Input.GetButtonDown("Jump") && grounded)
        {
            // SET the y velocity to jumpspeed
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
        else
        {
            // SET the y velocity to itself
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
    }

    private void Attack()
    {
        // IF the attack button is pressed AND attack is false
        if (Input.GetButtonDown("Attack") && !attack)
        {
            // The player is attacking, but has not hit anything yet
            attack = true;
            hitObject = false;

            // SET current coroutine to null
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            // IF the player is grounded
            if (grounded)
            {
                // SET the x velocity to 0
                rb.velocity = new Vector2(0.0f, rb.velocity.y);

                // SET the current move depending on the current punch index
                switch (punchIndex)
                {
                    case (int)PunchTier.LIGHT1:
                        currentMove = moves[(int)PunchTier.LIGHT1];
                        break;
                    case (int)PunchTier.LIGHT2:
                        currentMove = moves[(int)PunchTier.LIGHT2];
                        break;
                    case (int)PunchTier.MED:
                        currentMove = moves[(int)PunchTier.MED];
                        break;
                    case (int)PunchTier.HEAVY:
                        currentMove = moves[(int)PunchTier.HEAVY];
                        break;
                    default:
                        currentMove = moves[(int)PunchTier.LIGHT1];
                        break;
                }
            }
            else
            {
                // SET the current move to air move
                currentMove = airMove;
            }

            // Start attack routine
            currentCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    private void Grab()
    {
        // IF the grab button is pressed
        if (Input.GetButtonDown("Grab"))
        {
            // IF the player is not grabbing nor do have they have any objects grabbed currently
            if (!grabbing && objectsGrabbed.Count == 0)
            {
                // SET current coroutine to null
                if (currentCoroutine != null)
                    StopCoroutine(currentCoroutine);

                // The player has started reaching to grab an object
                grabbing = true;
                grabReach = true;
                grabRecover = false;

                // Start grab routine
                currentCoroutine = StartCoroutine(GrabRoutine());
            }

            // IF player has finished recovering their grab AND
            // They have grabbed at least one object AND
            // They have not started throwing
            if (!throwing && objectsGrabbed.Count > 0 && !grabRecover)
            {
                // SET current coroutine to null
                if (currentCoroutine != null)
                    StopCoroutine(currentCoroutine);

                // The player is no longer grabbing, but is now throwing
                grabbing = false;
                throwing = true;
            }
        }

        // IF the player is grabbing
        if (grabbing)
        {
            // IF grounded
            if (grounded)
            {
                // SET x velocity to 0
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
            }
        }
    }

    public void Hit(Vector2 knockBack)
    {
        // IF the player is not hit
        if (!hit)
        {
            // SET booleans to proper values
            hit = true;
            attack = false;
            grabbing = false;
            grabReach = false;
            grabRecover = false;

            // SET current coroutine to null
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            // SET velocity to knockback vector
            rb.velocity = knockBack;            

            // Start hit routine
            currentCoroutine = StartCoroutine(HitRoutine());
        }
    }

    public void RemoveObjectsFromGrabbedObjectsList()
    {
        // For all objects currently grabbed by player
        foreach (GameObject gameObj in objectsGrabbed)
        {
            // Reset hitbox rotation
            hitbox.rotation = Quaternion.identity;

            // IF the player is throwing
            if (throwing)
            {
                // IF the player is facing right
                if (facingRight)
                {
                    // Throw object in the positive x direction
                    gameObj.GetComponent<Grunt>().Released(throwSpeed);
                }
                else
                {
                    // Throw object in the negative x direction
                    gameObj.GetComponent<Grunt>().Released(throwSpeed * negativeXDir);
                }
            }
            else
            {
                // IF the player is facing right
                if (facingRight)
                {
                    // Release object in the positive x direction
                    gameObj.GetComponent<Grunt>().Released(releaseSpeed);
                }
                else
                {
                    // Release object in the negative x direction
                    gameObj.GetComponent<Grunt>().Released(releaseSpeed * negativeXDir);
                }
            }
        }

        // Clear list of object grabbed
        objectsGrabbed.Clear();
    }

    public void AddToGrabbedObjectsList(GameObject gameObj)
    {
        objectsGrabbed.Add(gameObj);
    }

    // Sets all combat booleans to false
    private void ResetCombatBooleans()
    {
        attack = false;
        hit = false;
        hitObject = false;
        punchClipEnd = false;

        grabbing = false;
        grabReach = false;
        grabRecover = false;

        throwing = false;
    }

    // Coroutines
    private IEnumerator AttackRoutine()
    {
        yield return new WaitUntil(()=> punchClipEnd);

        punchClipEnd = false;

        attack = false;

        currentMove.ChangeKnockBack(0);

        if (grounded && hitObject)
        {
            punchIndex++;

            if (punchIndex >= 4)
            {
                punchIndex = 0;
            }

            yield return new WaitForSeconds(punchBuffer);
        }

        punchIndex = 0;
    }

    private IEnumerator GrabRoutine()
    {
        yield return new WaitUntil(() => !grabReach);
        
        grabRecover = true;

        yield return new WaitUntil(() => !grabRecover);

        if (objectsGrabbed.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);

            RemoveObjectsFromGrabbedObjectsList();
        }

        grabbing = false;
    }

    private IEnumerator HitRoutine()
    {
        yield return new WaitForSeconds(hitDur);
        hit = false;
    }

    // Setters
    public void SetFacingRight(bool facingRight)
    {
        this.facingRight = facingRight;
    }

    public void SetPunchClipEnd(bool punchClipEnd)
    {
        this.punchClipEnd = punchClipEnd;
    }

    public void SetHitObject(bool hitObject)
    {
        this.hitObject = hitObject;
    }

    public void SetGrabbing(bool grabbing)
    {
        this.grabbing = grabbing;
    }

    public void SetGrabReach(bool grabReach)
    {
        this.grabReach = grabReach;
    }

    public void SetGrabRecover(bool grabRecover)
    {
        this.grabRecover = grabRecover;
    }

    public void SetThrowing(bool throwing)
    {
        this.throwing = throwing;
    }

    // Getters
    public Rigidbody2D GetRigidbody2D()
    {
        return rb;
    }

    public bool IsGrounded()
    {
        return grounded;
    }

    public bool IsFacingRight()
    {
        return facingRight;
    }

    public bool IsAttacking()
    {
        return attack;
    }

    public bool IsHit()
    {
        return hit;
    }

    public Move GetCurrentMove()
    {
        return currentMove;
    }

    public int GetPunchIndex()
    {
        return punchIndex;
    }

    public bool HasHitObject()
    {
        return hitObject;
    }

    public bool IsGrabbing()
    {
        return grabbing;
    }

    public bool IsReachingGrab()
    {
        return grabReach;
    }

    public bool IsRecoveringGrab()
    {
        return grabRecover;
    }

    public bool IsThrowing()
    {
        return throwing;
    }

    public List<GameObject> GetObjectsGrabbed()
    {
        return objectsGrabbed;
    }

    // Collsion
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && hit)
        {
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(feet.position, groundCheckSize);
    }
}