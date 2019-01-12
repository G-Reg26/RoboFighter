/***
 * Author: Gregorio Lozada
 * Date Created: Oct 21, 2018
 * 
 * This is the class that controls the Grunt's behavior a basic enemy type.
 * Functions such as movement and combat are handled in this class.
 * 
 * Note: Eventually all enemies will inherit from a base Enemy class that will
 * hold attributes and behaviors common to all types of enemies.
 **/
using UnityEngine;
using System.Collections;

public class Grunt : MonoBehaviour {

    public enum State
    {
        THINKING,
        APPROACH,
        ATTACK,
        BACKAWAY,
        HIT,
        GRABBED,
        RELEASED,
        RECOVER

    }

    private Rigidbody2D rb;
    private Coroutine currentCoroutine;

    private Grunt[] enemyList;
    private GameObject[] playerOnlyWalls;
    private PlayerController player;
    private LevelManager levelManager;

    private Vector2 respawnPoint;

    private State currentState;

    // Movement booleans
    private bool grounded;
    private bool inFrontOfPlayer;
    private bool facingRight;

    // Combat booleans
    private bool attack;
    private bool hit;
    private bool recover;
    private bool canTakeHit;
    private bool grabbed;

    // Physics box parameters
    public Transform feet;
    public Transform front;

    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    public Vector2 groundCheckSize;
    public Vector2 playerCheckSize;

    // Attack clips
    public AnimationClip jab;
    public AnimationClip uppercut;

    // Movement speeds
    public float runSpeed;
    public float backAwaySpeed;

    // Combat durations
    public float recoverDur;
    public float invincibilityDur;

    // State durations
    public float thinkDur;
    public float approachDur;

    public float minDistanceFromPlayer;

    public int health;

    // Use this for initialization
    void Start () {
        // GET components
        rb = GetComponent<Rigidbody2D>();

        // Find objects
        player = FindObjectOfType<PlayerController>();
        levelManager = FindObjectOfType<LevelManager>();
        enemyList = FindObjectsOfType<Grunt>();
        playerOnlyWalls = GameObject.FindGameObjectsWithTag("PlayerOnlyWalls");

        // SET respawn point
        respawnPoint = transform.position;

        // SET movement booleans
        grounded = true;
        facingRight = true;
        inFrontOfPlayer = false;

        // SET combat booleans
        attack = false;
        hit = false;
        recover = false;
        canTakeHit = true;
        grabbed = false;

        // Ignore collision with the player and other enemies
        Physics2D.IgnoreCollision(player.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());

        foreach (Grunt grunt in enemyList)
        {
            Physics2D.IgnoreCollision(grunt.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());
        }

        // Ignore collision with player only walls
        foreach (GameObject wall in playerOnlyWalls)
        {
            Physics2D.IgnoreCollision(wall.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());
        }

        // SET current state
        currentState = State.THINKING;
    }
	
	// Update is called once per frame
	void Update () {
        // IF the player is not grabbed
        if (!grabbed)
        {
            CheckPhysBoxes();

            // IF the enemy is to the player's right
            if (player.transform.position.x + 0.5f < transform.position.x)
            {
                // Face the left
                facingRight = false;
            }
            // ELSE IF the enemy is to the player's left
            else if (player.transform.position.x - 0.5f > transform.position.x)
            {
                // Face the right
                facingRight = true;
            }

            // IF the enemy is grounded, has not been hit
            if (grounded && !hit)
            {
                // IF enemy has no more health
                if (health <= 0)
                {
                    // Destroy enemy
                    Destroy(gameObject);
                    // Create a new grunt (This is temporary)
                    levelManager.RespawnNewGrunt(respawnPoint);
                }

                // Depending on it's current state...
                switch (currentState)
                {
                    case State.THINKING:
                        // IF the enemy has not started thinking
                        if (currentCoroutine == null)
                        {
                            // Start thinking (Just stay idle for a while)
                            currentCoroutine = StartCoroutine(ThinkingState());
                        }
                        break;
                    case State.APPROACH:
                        // IF enemy has not started approaching
                        if (currentCoroutine == null)
                        {
                            // Start approaching (Approach player, but keep a distance for a set amount of time)
                            currentCoroutine = StartCoroutine(ApproachState());
                        }
                        // Move
                        Movement();
                        break;
                    case State.ATTACK:
                        // Attack player
                        Attack();
                        break;
                    case State.BACKAWAY:
                        // Back away from player
                        attack = false;
                        Movement();
                        break;
                }
            }
        }
	}

    // This checks if the enemy is grounded and if the enemy is in front of a player
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

        // IF physics box overlaps with an object in a layer specified in the whatIsPlayer layermask
        if (Physics2D.OverlapBox(front.position, playerCheckSize, 0.0f, whatIsPlayer))
        {
            // The enemy is in front of the player
            inFrontOfPlayer = true;
        }
        else
        {
            // The enemy is not in front of the player
            inFrontOfPlayer = false;
        }
    }

    // When the enemy is approaching or backing away...
    private void Movement()
    {
        Vector2 targetDir = new Vector2 (1.0f, 0.0f);

        float targetXPosition = 0.0f;

        // IF the enemy is facing right (The player is to the right of the enemy)
        if (facingRight)
        {
            // SET target x position to the difference of the player's position and the minimum distance
            targetXPosition = player.transform.position.x - minDistanceFromPlayer;
        }
        else
        {
            // SET target x position to the sum of the player's position and the minimum distance
            targetXPosition = player.transform.position.x + minDistanceFromPlayer;
        }

        // IF the target x position is to the enemy's left
        if ((targetXPosition - transform.position.x) < 0.0f)
        {
            // SET target direction to the enemy's left
            targetDir = new Vector2(-1.0f, 0.0f);
        }
        // IF the target x position is to the enemy's right
        else if ((targetXPosition - transform.position.x) > 0.0f)
        {
            // SET target direction to the enemy's right
            targetDir = new Vector2(1.0f, 0.0f);
        }

        // IF the enemy is facing right, but target dir is in the negative x direction OR
        // IF the enemy is facing left, but target dir is the positive x direction
        if (facingRight && targetDir.x < 0.0f || !facingRight && targetDir.x > 0.0f)
        {
            // SET the x velocity to the back away speed
            rb.velocity = targetDir * backAwaySpeed;
        }
        else
        {
            // SET the x velocity to the default run speed;
            rb.velocity = targetDir * runSpeed;
        }

        // The distance between it's position and the target position is very small
        if (Mathf.Abs(transform.position.x - targetXPosition) < 0.02f)
        {
            // IF enemy is currently in the state of backing away from the player AND 
            if (currentState == State.BACKAWAY)
            {
                // Stop backing away and start thinking on what to do next
                currentState = State.THINKING;
            }
            // Stop moving
            rb.velocity = Vector3.zero;

            // SET x position to the target x position
            transform.position = new Vector3(targetXPosition, transform.position.y);
        }
    }

    // When the enemy attacks...
    private void Attack()
    {
        // IF the enemy is not attacking
        if (!attack)
        {
            // IF the enemy is grounded
            if (grounded)
            {
                // IF the enemy is not in front of the player
                if (!inFrontOfPlayer)
                {
                    // IF the enemy is facing right
                    if (facingRight)
                    {
                        // SET x velocity to the run speed in the positive x direction
                        rb.velocity = new Vector2(runSpeed, rb.velocity.y);
                    }
                    else
                    {
                        // SET x velocity to the run speed in the negative x direction
                        rb.velocity = new Vector2(-runSpeed, rb.velocity.y);
                    }
                }
                else
                {
                    // SET x velocity to zero
                    rb.velocity = new Vector2(0.0f, rb.velocity.y);

                    // Start attacking
                    attack = true;

                    // Stop current coroutine
                    if (currentCoroutine == null)
                        currentCoroutine = StartCoroutine(AttackingState());
                }
            }
        }
    }

    // When the enemy is hit...
    public void Hit(Vector2 knockBack)
    {
        // SET all bools other than hit to false
        ResetCombatBooleans();
        hit = true;

        // Decrement health
        health--;

        // SET current state to HIT
        currentState = State.HIT;
        
        // Stop current coroutine
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        // Start the HitState coroutine
        currentCoroutine = StartCoroutine(HitState(knockBack));
    }

    // When the enemy is grabbed...
    public void Grabbed()
    {
        // SET all bools other than grabbed to false
        ResetCombatBooleans();
        grabbed = true;

        // SET rigidbody to kinematic and velocity to zero
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        // SET current state to GRABBED
        currentState = State.GRABBED;

        // Stop current coroutine
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        // Disable box collider while grabbed
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }

    // When the enemy is released...
    public void Released(Vector2 releaseSpeed)
    {
        // The enemy is no longer grabbed and can take hits
        grabbed = false;
        canTakeHit = true;

        // The enemy is no longer parented to the player hit box
        transform.parent = null;
        // SET rigidbody to not kinematic and velocity to release speed
        rb.isKinematic = false;
        rb.velocity = releaseSpeed;

        // SET current state to RELEASED
        currentState = State.RELEASED;

        // Start the ReleasedState coroutine
        currentCoroutine = StartCoroutine(ReleasedState());
    }

    // Sets all combat booleans to false
    private void ResetCombatBooleans()
    {
        grabbed = false;
        hit = false;
        attack = false;
        recover = false;
        canTakeHit = false;
    }

    // Coroutines
    private IEnumerator ThinkingState()
    {
        // Stop moving
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(thinkDur);

        // Depending on the random integer generated
        switch (Random.Range(0, 2))
        {
            case 0:
                // SET current state to APPROACH
                currentState = State.APPROACH;
                break;
            case 1:
                // SET current state to ATTACK
                currentState = State.ATTACK;
                break;
        }

        // SET current coroutine to nothing
        currentCoroutine = null;
    }

    private IEnumerator ApproachState()
    {
        yield return new WaitForSeconds(approachDur);

        // SET the current state to ATTACK
        currentState = State.ATTACK;

        // SET current coroutine to nothing
        currentCoroutine = null;
    }

    private IEnumerator AttackingState()
    {
        // Generate a random integer between 1 and 3
        int amountOfAttacks = Random.Range(1, 4);
        float timer;

        // IF the amount of attacks to do is greater than 2
        if (amountOfAttacks > 2)
        {
            // SET timer to the length of 2 jabs and 1 uppercut
            timer = (jab.length * 2) + uppercut.length;
        }
        else
        {
            // SET timer to the amount of attacks multiplied by the length of a jab   
            timer = amountOfAttacks* jab.length;
        }

        yield return new WaitForSeconds(timer);
           
        // SET current state to BACKAWAY
        currentState = State.BACKAWAY;

        // SET current coroutine to nothing
        currentCoroutine = null;
    }

    private IEnumerator HitState(Vector2 knockBack)
    {
        // SET velocity to knockback
        rb.velocity = knockBack;

        yield return new WaitForSeconds(invincibilityDur);

        // The enemy can get hit again
        canTakeHit = true;

        yield return new WaitUntil(() => grounded);

        // The enemy is no longer hit
        hit = false;

        // Stop movement
        rb.velocity = Vector2.zero;

        // Start recovering
        currentCoroutine = StartCoroutine(Recover());
    }

    private IEnumerator Recover()
    {
        // The enemy is recovering
        recover = true;

        // SET current state to RECOVER
        currentState = State.RECOVER;

        yield return new WaitForSeconds(recoverDur);

        // The enemy is no longer recovering
        recover = false;

        // SET current coroutine to nothing
        currentCoroutine = null;

        // SET current state to THINKING
        currentState = State.THINKING;
    }

    private IEnumerator ReleasedState()
    {
        yield return new WaitForSeconds(0.2f);

        // SET box collider to enabled
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
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

    public bool FacingRight()
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

    public bool CanBeHit()
    {
        return canTakeHit;
    }

    public bool IsGrabbed()
    {
        return grabbed;
    }

    public State GetCurrentState()
    {
        return currentState;
    }

    // Collision
    void OnCollisionEnter2D(Collision2D other)
    {
        // IF the enemy collides with the ground AND has been released, or thrown
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && (currentState == State.RELEASED))
        {
            // SET x velocity to zero
            rb.velocity = new Vector2(0.0f, rb.velocity.y);

            // 
            currentCoroutine = null;

            currentState = State.THINKING;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(feet.position, groundCheckSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(front.position, playerCheckSize);
    }
}