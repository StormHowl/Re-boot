using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



public class PlayerController : NetworkBehaviour
{
    public GameObject bulletPrefab;

    public Transform bulletSpawn;

    public float totalRegisteredMovements = 0.0f;

    public Camera camera;

    // Use this for initialization
    void Start()
    {
        gameObject.tag = "Player";
        camera = GetComponentInChildren<Camera>();

        if (!isLocalPlayer)
        {
            camera.enabled = false;
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
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
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