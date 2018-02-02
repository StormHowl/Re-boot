using System.Collections;
using System.Collections.Generic;
using Rewind;
using UnityEngine;

public class DestructibleWall : RewindableSceneryElement {

	public GameObject brick;
    public ParticleSystem ps;

    int health = 100;
    public bool isBuilt;

    int nb_brick_per_wall = 5;

    bool isQuitting = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void TakeDamages(int damages)
    {
        ps.Play();
        health -= damages;
        if (health <=0)
        {
            Destroy(gameObject);
        }
    }

    public override void OnStateChanged(bool state)
    {

    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        if (!isQuitting && !isBuilt)
        {
            System.Random r = new System.Random();
            float x, y, z;
            y = transform.position.y - transform.localScale.y/2 + 0.1f;// - transform.localScale.y / 2;
            float shiftX, shiftY;
            for (int i = 0; i < nb_brick_per_wall; i++)
            {
                shiftX = r.Next(-20, 20) / 10.0f;
                shiftY = r.Next(-20, 20) / 10.0f;
                x = transform.position.x + shiftX;
                z = transform.position.z + shiftY;
                int rotate = r.Next(1, 90);
                Instantiate(brick, new Vector3(x, y, z), Quaternion.Euler(0, rotate, 0));
            }
        }
	}
}
