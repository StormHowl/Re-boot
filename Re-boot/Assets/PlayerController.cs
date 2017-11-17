﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct TimePosition
{
    public float deltaTime;
    public Vector3 position;

    public TimePosition(float deltaTime, Vector3 position)
    {
        this.deltaTime = deltaTime;
        this.position = position;
    }
}

public class PlayerController : NetworkBehaviour
{
    public const float RewindTime = 2.0f;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    public List<TimePosition> positions;
    public float totalRegisteredMovements = 0.0f;

    public Camera camera;

    // Use this for initialization
    void Start()
    {
        positions = new List<TimePosition>();
        camera = GetComponentInChildren<Camera>();
		
		this.transform.position = new Vector3(0.0f,1.0f,0.0f);

        if (!isLocalPlayer)
        {
            camera.enabled = false;
        }
    }

    // Update is called once per frame

    void Update()
    {
        if (!isLocalPlayer)
        {
            RegisterPosition();
            return;
        }

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdFire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            CmdRewind();
        }
    }

    [Server]
    void RegisterPosition()
    {
        positions.Add(new TimePosition(Time.deltaTime, transform.position));
        if (totalRegisteredMovements + Time.deltaTime > 20.0f)
        {
            totalRegisteredMovements -= positions[0].deltaTime;
            positions.RemoveAt(0);

            totalRegisteredMovements += Time.deltaTime;
        }
    }

    // This [Command] code is called on the Client …
    // … but it is run on the Server!
    [Command]
    void CmdFire()
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject) Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletPrefab.GetComponent<Bullet>().speed;

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }

    [Command]
    void CmdRewind()
    {
        float delta = 0.0f;
        int i = positions.Count - 1;
        for (; i > 0 && delta <= RewindTime; --i)
        {
            delta += positions[i].deltaTime;
        }

        transform.position = positions[i].position;
        RpcRewind(transform.position);
    }

    [ClientRpc]
    void RpcRewind(Vector3 position)
    {
        if (isLocalPlayer)
        {
            transform.position = position;
        }
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}