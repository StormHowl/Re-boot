using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;

public class TeamManager : NetworkBehaviour {

	public static TeamManager Instance = null;

	private static List<IRewindEntity> _teamHuman;
	private static List<IRewindEntity> _teamRobot;

	// Use this for initialization

	void Start() {
		_teamHuman = new List<IRewindEntity>();	
		_teamRobot = new List<IRewindEntity>();	
		Debug.Log("Initialise : " + _teamHuman.Count + " and " + _teamRobot.Count );	
	}

	void Awake () {
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//return the choosen team of the given player (return -1 if is not in a team)
	[Server]
	public int FindTeam(IRewindEntity player) {
		if ( _teamHuman.Contains(player))
			return 0;
		else if (_teamRobot.Contains(player))
			return 1;

		return -1;
	}

	[Server]
	public int AddPlayer(IRewindEntity player) {
		//TODO : check if there is too much player ? Or do it in server when try to connect ?
		if (_teamHuman.Count < _teamRobot.Count) {
			_teamHuman.Add(player);
			Debug.Log("Adding a new player in Human");
			return 0;
		} else if (_teamHuman.Count > _teamRobot.Count) {
			_teamRobot.Add (player);
			return 1;
		} else {
			if (Random.value > 0.5) { //Random.value return a number between 0.0 and 1.0
				_teamRobot.Add (player);
				return 1;
			} 

			_teamHuman.Add (player);
			return 0;
		}
	}


}
