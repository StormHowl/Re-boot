using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;


public class PlayerController : NetworkBehaviour, IRewindable
{
    public GameObject BulletPrefab;
    public Transform BulletSpawn;
    public Camera viewCamera;

    private RewindableGameObject _rewindableGameObject;

    // Use this for initialization
    void Start()
    {
        gameObject.tag = "Player";
        viewCamera = GetComponentInChildren<Camera>();

        if (!isLocalPlayer)
        {
            viewCamera.enabled = false;
        }

        if (isServer)
        {
            global::Rewind.AddRewindable(this);
            _rewindableGameObject = new RewindableGameObject();
        }
    }

    // Update is called once per frame

    void Update()
    {
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

        if (isServer)
        {
            _rewindableGameObject.SaveTemporalFlash(transform.position, Time.deltaTime);
        }
    }

    // This [Command] code is called on the Client …
    // … but it is run on the Server!
    [Command]
    void CmdFire()
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = Instantiate(
            BulletPrefab,
            BulletSpawn.position,
            BulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }

    [Command]
    void CmdRewind()
    {
        global::Rewind.RewindEveryone();
    }

    [Server]
    public void Rewind()
    {
        RpcRewind(_rewindableGameObject.FindObjectFlashPosition());
    }

    [ClientRpc]
    public void RpcRewind(Vector3 position)
    {
        transform.position = position;
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}