﻿using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : RewindableEntity, IRewindEntity
{
    private static float _sensitivity = 2.0f;
    private static float _speed = 5.0f;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;


    private CharacterController player;
    public Camera Camera;
    public GameObject gun;

    private float _moveLr;
    private float _moveFb;
    private float _rotX;
    private float _rotY;

    private MouseHandler _mouseHandler;

    // Use this for initialization
    void Start()
    {
        transform.position = new Vector3(0.0f, 1.0f, 0.0f);
        player = GetComponent<CharacterController>();

        if (!isLocalPlayer)
            Camera.enabled = false;
        if (isServer)
        {
            Init();
            GameManager.Instance.AddEntity(this);
        }

        if (isLocalPlayer)
        {
            _mouseHandler = new MouseHandler();
            _mouseHandler.Init(transform, Camera.transform, gun.transform);
        }
    }

    // Update is called once per frame

    void Update()
    {
        if(isServer)
            SaveTemporalFlash(transform.position, Time.deltaTime);

        if (!isLocalPlayer)
            return;


        _moveLr = Input.GetAxis("Horizontal") * _speed;
        _moveFb = Input.GetAxis("Vertical") * _speed;
        _rotX = Input.GetAxis("Mouse X") * _sensitivity;
        _rotY = Input.GetAxis("Mouse Y") * _sensitivity;

        Vector3 movement = new Vector3(_moveLr, 0, _moveFb);
        _mouseHandler.LookRotation(transform, Camera.transform, gun.transform);

        movement = transform.rotation * movement;
        player.Move(movement * Time.deltaTime);


        if ((int) Input.GetAxis("Fire1") == 1 || Input.GetKeyDown(KeyCode.Space))
        {
            CmdFire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            CmdRewind();
        }
    }


    // This [Command] code is called on the Client …
    // … but it is run on the Server!
    [Command]
    void CmdFire()
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity =
            bullet.transform.forward * bulletPrefab.GetComponent<Bullet>().speed;

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }

    [Command]
    void CmdRewind()
    {
        GameManager.Instance.Rewind();
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    [Server]
    public void Rewind()
    {
        RpcRewind(FindObjectFlashPosition());
    }

    [ClientRpc]
    public void RpcRewind(Vector3 position)
    {
        transform.position = position;
    }
}