using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : RewindableEntity, IRewindEntity
{
    public static float Speed = 5.0f;
    public static float ShootingCadency = 0.5f;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    private CharacterController player;
    public Camera Camera;
    public GameObject Gun;

    private float _moveLr;
    private float _moveFb;

    private bool _shooting;
    private float _shootingCounter;

    private MouseHandler _mouseHandler;

    public int teamNumber;
    public Vector3 spawnHuman = new Vector3(0.0f, 1.0f, 0.0f);
    public Vector3 spawnRobot = new Vector3(10.0f, 1.0f, 0.0f);

    // Use this for initialization
    void Start()
    {
        transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        player = GetComponent<CharacterController>();

        if (!isLocalPlayer)
        {
            Camera.enabled = false;
            Camera.GetComponent<AudioListener>().enabled = false;
        }
        else if (isLocalPlayer)
        {
            _mouseHandler = new MouseHandler();
            _mouseHandler.Init(transform, Camera.transform, Gun.transform);
        }

        if (!isServer) return;

        Init();
        GameManager.Instance.AddEntity(this);
        teamNumber = TeamManager.Instance.AddPlayer(this);
        RpcInitializeTeam(teamNumber);
    }

    // Update is called once per frame

    void Update()
    {
        if (isServer)
            SaveTemporalFlash(transform.position, Time.deltaTime);

        if (!isLocalPlayer)
            return;

        _moveLr = Input.GetAxis("Horizontal") * Speed;
        _moveFb = Input.GetAxis("Vertical") * Speed;

        Vector3 movement = new Vector3(_moveLr, 0, _moveFb);
        _mouseHandler.LookRotation(transform, Camera.transform, Gun.transform);

        movement = transform.rotation * movement;
        player.Move(movement * Time.deltaTime);

        HandleFire();

        if (Input.GetKeyDown(KeyCode.R))
        {
            CmdRewind();
        }
    }

    [ClientRpc]
    void RpcInitializeTeam(int team)
    {
        teamNumber = team;
        if (teamNumber == 0)
        {
            GetComponent<MeshRenderer>().material.color = Color.green;
            transform.position = spawnHuman;
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.red;
            transform.position = spawnRobot;
        }
    }

    #region Fire
    [Client]
    void HandleFire()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _shooting = true;
            _shootingCounter = ShootingCadency;
        }

        if (Input.GetButtonUp("Fire1"))
        {
            _shooting = false;
        }

        if (_shooting)
            _shootingCounter += Time.deltaTime;

        if (!(_shootingCounter >= ShootingCadency)) return;

        _shootingCounter = 0f;
        CmdFire();
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
    #endregion

    #region Rewind

    [Command]
    void CmdRewind()
    {
        GameManager.Instance.Rewind();
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

    #endregion
}