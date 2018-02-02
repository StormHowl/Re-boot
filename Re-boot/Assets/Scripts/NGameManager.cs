using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// "Heart" of the game which is in charge to play the Controller. Registers players, manages interactions between principal managers (<see cref="RewindManager"/>,
/// <see cref="GenerationManager"/>, <see cref="TeamManager"/>).
/// </summary>
[RequireComponent(typeof(GenerationManager))]
[RequireComponent(typeof(RewindManager))]
[RequireComponent(typeof(TeamManager))]
public class NGameManager : MonoBehaviour
{
    #region static instance creation
    public static NGameManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager in scene");
        }
        else
        {
            Instance = this;
            Setup();
        }
    }
    #endregion

    #region match settings
    public MatchSettings Settings;
    public Assets Assets;
    #endregion

    #region Player registering
    private const string PlayerIdPrefix = "Player ";
    private readonly Dictionary<string, Player> _players = new Dictionary<string, Player>();
    private int _playerIncrement = 0;

    /// <summary>
    /// Method called by <see cref="PlayerSetup.OnStartClient"/>, to register the player when he enters the game.
    /// </summary>
    /// <param name="netId"></param>
    /// <param name="player"></param>
    public void RegisterPlayer(string netId, Player player)
    {
        string id = PlayerIdPrefix + netId;
        _players.Add(id, player);
        player.transform.name = id;
    }

    public void UnregisterPlayer(string playerId)
    {
        _players.Remove(playerId);
    }

    public Player GetPlayer(string playerId)
    {
        return _players[playerId];
    }

    /// <summary>
    /// Called by <see cref="Player.Setup"/> to say to the team manager that we want our "Player" to be added to a new team.
    /// The affectation method is not definitive and is just a stupid modulo on a static incremental value.
    /// </summary>
    /// <param name="player"></param>
    public void AffectTeam(Player player)
    {
        TeamManager.AddPlayer(player, (TeamManager.Team) (_playerIncrement++%2));
    }
    #endregion

    #region game management
    public NetworkManager NetworkManager;
    public RewindManager RewindManager;
    public GenerationManager GenerationManager;
    public TeamManager TeamManager;

    /// <summary>
    /// Method called on the creation of the instance by <see cref="Awake"/> to setup all manager.
    /// Used to avoid the call of method "Start" on each component, potentially causing instanciation order problems, which may
    /// result in difficulties concerning calls to these managers (from <see cref="PlayerRewind"/> for instance).
    /// </summary>
    void Setup()
    {
        TeamManager.Setup();
        GenerationManager.Setup();
        RewindManager.Setup();

        var ia = Instantiate(AIPlayer, Vector3.zero, Quaternion.identity);
    }

    public void InitializeSpawnPoints(Vector3[] points)
    {
        TeamManager.InitializeSpawnPoints(points);
    }

    public void EndGame(TeamManager.Team team) 
    {
        Debug.Log("GameManager: " + team + " won");
    }
    #endregion

    #region IA

    public GameObject AIPlayer;

    public GameObject Waypoints;
    public GameObject Waypoint;
    public Material DeactivatedMaterial;
    public Material ActivatedMaterial;

    private Dictionary<Vector3, GameObject> _waypoints = new Dictionary<Vector3, GameObject>();
    private List<GameObject> _illuminatedPath = new List<GameObject>();


    public void AddWayPoint(Vector3 position)
    {
        var t = Instantiate(Waypoint, position, Quaternion.identity);
        t.transform.SetParent(Waypoints.transform);
        t.transform.GetChild(0).transform.localPosition = new Vector3(0, 2, 0);
        _waypoints.Add(position, t);
    }

    public void AddWayPoints(Vector3[] positions)
    {
        foreach (var position in positions)
        {
            AddWayPoint(position);
        }
    }

    public void IlluminatePath(Vector3[] positions)
    {
        _illuminatedPath.ForEach(g => g.GetComponentInChildren<MeshRenderer>().sharedMaterial = DeactivatedMaterial);
        _illuminatedPath.Clear();

        foreach (var position in positions)
        {
            _waypoints[position].GetComponentInChildren<MeshRenderer>().sharedMaterial = ActivatedMaterial;
            _illuminatedPath.Add(_waypoints[position]);
        }
    }

    public void ToggleWaypointsVisibility(bool visible)
    {
        Waypoints.SetActive(visible);
    }
    #endregion
}
