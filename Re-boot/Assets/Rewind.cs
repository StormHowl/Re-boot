using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct GameFlash
{
    public float deltaTime;
    public Dictionary<GameObject, Vector3> objects;

    public GameFlash(float deltaTime)
    {
        this.deltaTime = deltaTime;
        this.objects = new Dictionary<GameObject, Vector3>();
    }

    public void AddObjectTimePosition(GameObject obj, Vector3 position)
    {
        this.objects.Add(obj, position);
    }
}

public struct TimeObjectPosition
{
    public GameObject obj;
    public Vector3 position;
}

public class Rewind : NetworkBehaviour
{
    private List<GameFlash> _flashes;
    private const float SavedTimeLimit = 50.0f;
    private float _totalSavedTime;

    // Use this for initialization
    [Server]
    void Start()
    {
        if (isServer)
        {
            _flashes = new List<GameFlash>();
            _totalSavedTime = 0.0f;
        }
    }

    // Update is called once per frame
    [Server]
    void Update()
    {
        if (isServer)
        {
            CreateGameFlash();
        }
    }

    [Server]
    private void CreateGameFlash()
    {
        GameFlash flash = new GameFlash(Time.deltaTime);
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            flash.AddObjectTimePosition(player, player.transform.position);
        }
        _flashes.Add(flash);
        _totalSavedTime += flash.deltaTime;

        if (!(_totalSavedTime > SavedTimeLimit)) return;

        _totalSavedTime -= _flashes[0].deltaTime;
        _flashes.RemoveAt(0);
    }

    [Command]
    public void CmdDoRewind()
    {
        RpcRewindTo(_flashes[0]);
    }

    [ClientRpc]
    void RpcRewindTo(GameFlash flash)
    {
    }
}