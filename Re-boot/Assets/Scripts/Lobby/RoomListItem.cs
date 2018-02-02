using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class RoomListItem : MonoBehaviour {

	public delegate void JoinRoomDelegate(MatchInfoSnapshot match);
	private JoinRoomDelegate _joinRoomCallback;

	[SerializeField]
	private Text _roomInfo;

	private MatchInfoSnapshot _match;

	public void Setup(MatchInfoSnapshot match, JoinRoomDelegate joinRoomCallback) {
		_match = match;
		_joinRoomCallback = joinRoomCallback;

		_roomInfo.text = _match.name + "(" + _match.currentSize + "/" + _match.maxSize + ")";
	}

	public void JoinRoom() {
		_joinRoomCallback.Invoke(_match);
	}
}
