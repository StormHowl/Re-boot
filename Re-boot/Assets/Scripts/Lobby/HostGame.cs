using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class HostGame : MonoBehaviour {

	[SerializeField] //to allow it to be change in game if we want
	private uint _roomSize = 6;
	private string _roomName;

	private NetworkManager _networkManager;
	private int _nbRoom;

	//TODO : not set in in public but just get it with a getComponent or something ?
	public InputField _input;

	// Use this for initialization
	void Start () {
		_networkManager = NetworkManager.singleton;
		if (_networkManager.matchMaker == null) {
			_networkManager.StartMatchMaker();
		}
		_networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
	}

	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {
		if (success) {
			_nbRoom = matches.Count;
		} else {
			_nbRoom = 0;
		}
		_roomName = "Room" + _nbRoom;
		_input.text = _roomName;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void CreateRoom() {
		if (_roomName != "" && _roomName != null) {
			Debug.Log ("Creation of the room " + _roomName + " with " + _roomSize + " players.");

			//Creation of the room
			_networkManager.matchMaker.CreateMatch(_roomName, _roomSize, true, "","", "", 0, 0, _networkManager.OnMatchCreate);
		}
	}

	public void SetRoomName( string name) {
		_roomName = name;
	}
}
