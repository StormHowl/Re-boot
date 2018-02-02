using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewind;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manager in charge of the Teams.
/// </summary>
public class TeamManager : NetworkBehaviour
{
    #region Team enum
    public enum Team
    {
        Humans = 0,
        Robots = 1
    };
    #endregion

    private Dictionary<Player, Team> _teams;
    private int[] _remainingLives;

    public GameObject[] SpawnPoints;

    public void Setup()
    {
        _teams = new Dictionary<Player, Team>();
        _remainingLives = new int[2];
        SpawnPoints = new GameObject[2];

        ResetLives();
    }

    #region lives management
    void ResetLives()
    {
        _remainingLives[0] = NGameManager.Instance.Settings.StartingLives;
        _remainingLives[1] = NGameManager.Instance.Settings.StartingLives;
    }

    [Server]
    public void RemoveLifeToTeam(Team team)
    {
        _remainingLives[(int) team]--;

        CheckRemainingLives();
    }

    void CheckRemainingLives()
    {
        if (_remainingLives[(int) Team.Humans] <= 0)
            NGameManager.Instance.EndGame(Team.Humans);
        else if (_remainingLives[(int) Team.Robots] <= 0)
            NGameManager.Instance.EndGame(Team.Humans);
    }

    [Command]
    public void CmdNotifyTeamLostLive(string playerName)
    {
        Player player = NGameManager.Instance.GetPlayer(playerName);
        int lives = --_remainingLives[(int) _teams[player]];
        _teams
            .Keys
            .ToList()
            .ForEach(k =>
            {
                if(_teams[k] == _teams[player])
                    k.RpcUpdateTeamRemainingLives(lives);
            });
    }
    #endregion

    #region players management
    public Team GetTeamOfPlayer(Player player)
    {
        return _teams[player];
    }

    public void AddPlayer(Player player, Team team)
    {
        _teams.Add(player, team);
    }

    public void RemovePlayer(Player player)
    {
        _teams.Remove(player);
    }
    #endregion

    #region spawn points
    public void InitializeSpawnPoints(Vector3[] points)
    {
        SpawnPoints[0] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        SpawnPoints[0].transform.position = points[(int)Team.Humans] - new Vector3(0, 1, 0);
        SpawnPoints[0].transform.localScale = new Vector3(10.5f, 0.1f, 10.5f);
        SpawnPoints[0].GetComponent<CapsuleCollider>().enabled = false;
        SpawnPoints[0].GetComponent<MeshRenderer>().material.color = Color.blue;

        SpawnPoints[1] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        SpawnPoints[1].transform.position = points[(int)Team.Robots] - new Vector3(0, 0.9f, 0);
        SpawnPoints[1].transform.localScale = new Vector3(10.5f, 0.1f, 10.5f);
        SpawnPoints[1].GetComponent<CapsuleCollider>().enabled = false;
        SpawnPoints[1].GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public Vector3 GetSpawnPointOfPlayer(Player player)
    {
        return SpawnPoints[(int) _teams[player]].transform.position;
    }
    #endregion

    [Server]
    public void UpdatePlayersRewindCooldown(PlayerRewind player)
    {
        foreach (var playerKey in _teams.Keys)
        {
            if (playerKey.transform.name == player.transform.name)
            {
                playerKey
                    .GetComponent<PlayerRewind>()
                    .RpcSetCooldown(NGameManager.Instance.Settings.GlobalCooldown + NGameManager.Instance.Settings.TeamCooldown + NGameManager.Instance.Settings.CooldownPerPlayer * _teams.Count);
            } else if (_teams[playerKey] == _teams[player.GetComponent<Player>()])
            {
                playerKey
                    .GetComponent<PlayerRewind>()
                    .RpcSetCooldown(NGameManager.Instance.Settings.GlobalCooldown + NGameManager.Instance.Settings.TeamCooldown);
            }
            else
            {
                playerKey
                    .GetComponent<PlayerRewind>()
                    .RpcSetCooldown(NGameManager.Instance.Settings.GlobalCooldown);
            }
        }
    }
}
