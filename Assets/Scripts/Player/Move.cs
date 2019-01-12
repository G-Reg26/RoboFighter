/***
 * Author: Gregorio Lozada
 * Created: 10/15/2018
 * 
 * This class holds the information for certain moves such as:
 * Knocbacks: All the knock back this move does, this is for when
 * moves do different knock back at different frames
 * Knockback: The knock back the move currently has enabled
 * Clip: The move animation clip to play
 */

using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour {

    public Vector2[] knockBacks;

    public Vector2 knockBack;
    public AnimationClip clip;

    public bool airMove;

    public void Start()
    {
        knockBack = knockBacks[0];
    }

    public bool IsAirMove()
    {
        return airMove;
    }

    //Change current move knockback to knockback at specified index
    public void ChangeKnockBack(int index)
    {
        knockBack = knockBacks[index];
    }
}
