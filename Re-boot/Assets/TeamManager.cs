using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;

public class TeamManager : NetworkBehaviour {

	public static TeamManager Instance = null;

	//private static List<IRewindEntity> _teamHuman;
	//private static List<IRewindEntity> _teamRobot;

	private static Dictionary<IRewindEntity, int> _teams;
	int _nbHuman;
	int _nbRobot;

	// Use this for initialization

	void Start() {
		/*_teamHuman = new List<IRewindEntity>();	
		_teamRobot = new List<IRewindEntity>();	
		Debug.Log("Initialise : " + _teamHuman.Count + " and " + _teamRobot.Count );*/
		_teams = new Dictionary<IRewindEntity, int> ();
		_nbHuman = 0;
		_nbRobot = 0;
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
		int type;
		_teams.TryGetValue (player, out type);

		return type;
	}

	[Server]
	public int AddPlayer(IRewindEntity player) {
		//TODO : check if there is too much player ? Or do it in server when try to connect ?
		if (_nbHuman < _nbRobot) {
			_teams.Add(player, 0);
			_nbHuman++;
			return 0;
		} else if (_nbHuman > _nbRobot) {
			_teams.Add(player, 1);
			_nbRobot++;
			return 1;
		} else {
			if (Random.value > 0.5) { //Random.value return a number between 0.0 and 1.0
				_teams.Add(player,1);
				_nbRobot++;
				return 1;
			}
			_teams.Add(player,0);
			_nbHuman++;
			return 0;
		}
	}


}
