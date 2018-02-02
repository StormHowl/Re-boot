using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpBrick : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    void OnCollisionEnter(Collision other)
    {
        var hit = other.gameObject;
        if (hit.CompareTag("Player"))
        {
           if (hit.transform.GetComponent<PlayerBuild>().PickUpBrick())
            {
                Destroy(gameObject);
            }
            
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
