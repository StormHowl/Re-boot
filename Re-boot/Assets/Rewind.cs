using System.Collections;
using System.Collections.Generic;
using UnityEngine;
<<<<<<< HEAD

public struct GameState
{
	//public List<DestroyableItems> items;
	public List<TimePosition> playerPositions;
}

public class Rewind : MonoBehaviour
{

	private float _globalCooldown = 4.0f;
	private float _teamCooldown = 2.0f;
	private float _playerCooldown = 2.0f;

	private float _rewindTime = 3.0f;

	private List<GameState> _states;
	private List<GameObject> _players;

	// Use this for initialization
	void Start () {
		_states = new List<GameState>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void RegisterPlayer(GameObject player)
	{
		this._players.Add(player);
	}
}
=======
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
>>>>>>> 3d25ac42eb1e9ca5b00da569821f3dcfd4f98c60
