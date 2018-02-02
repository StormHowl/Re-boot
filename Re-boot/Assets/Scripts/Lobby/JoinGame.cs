using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class JoinGame : MonoBehaviour {

	List<GameObject> _roomList= new List<GameObject>();

	[SerializeField]
	private Text _status;

	[SerializeField]
	private GameObject _roomListItemPrefab;

	[SerializeField]
	private Transform _roomListParent;

	private NetworkManager _networkManager;


	// Use this for initialization
	void Start () {
		_networkManager = NetworkManager.singleton;
		if (_networkManager.matchMaker == null) {
			_networkManager.StartMatchMaker ();
		}

		RefreshRoomList ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void RefreshRoomList() {
		ClearRoomList();
		//just list the 20 first rooms
		_networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
		_status.text = "Loading...";
	}

	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {
		_status.text = "";

		if (!success) {
			_status.text = "Couldn't get room list..:c";
			return;
		}

		foreach(MatchInfoSnapshot match in matches) {
			GameObject newRoomListItem = Instantiate(_roomListItemPrefab);
			//that will take care of setting up th name/amount of users
			// as weell as setting up a callback function that will join the game.

			RoomListItem roomListItem = newRoomListItem.GetComponent<RoomListItem>();
			roomListItem.transform.SetParent(_roomListParent);
			if (roomListItem != null) {
				roomListItem.Setup(match, JoinRoom);
			}
			_roomList.Add(newRoomListItem);
		}

		if (_roomList.Count == 0) {
			_status.text = "No rooms available at the moment..";
		}
	}

	public void JoinRoom(MatchInfoSnapshot match) {
		_networkManager.matchMaker.JoinMatch (match.networkId, "", "", "", 0, 0, _networkManager.OnMatchJoined);
		ClearRoomList ();
		_status.text = "Joining...";
	}

	void ClearRoomList() {
		for (int i = 0; i < _roomList.Count; i++) {
			Destroy (_roomList[i]);
		}

		_roomList.Clear();
	}
}
