/***
 * Author: Gregorio Lozada
 * Created: 11/7/2018
 * 
 * At the moment all this does is spawn an enemy grunt when it's killed
 */

using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

    public Grunt grunt;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Spawn grunt
    public void RespawnNewGrunt(Vector3 position)
    {
        Instantiate(grunt, position, Quaternion.identity, null);
    }
}
