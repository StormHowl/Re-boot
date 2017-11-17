using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
