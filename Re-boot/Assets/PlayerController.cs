using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : RewindableEntity, IRewindEntity
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;


    private Camera _camera;

    // Use this for initialization
    void Start()
    {
        _camera = GetComponentInChildren<Camera>();

        this.transform.position = new Vector3(0.0f, 1.0f, 0.0f);

        if (!isLocalPlayer)
            _camera.enabled = false;
        if (isServer)
        {
            Init();
            GameManager.Instance.AddEntity(this);
        }
    }

    // Update is called once per frame

    void Update()
    {
        if(isServer)
            SaveTemporalFlash(transform.position, Time.deltaTime);

        if (!isLocalPlayer)
            return;


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