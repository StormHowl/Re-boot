using System.Collections;
using System.Collections.Generic;
using Rewind;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Rewind manager which receives calls for rewind and broadcasts it. Only consists really in a List referencing the entities
/// which can be rewinded, and a method which calls rewind on all of them.
/// </summary>
public class RewindManager : NetworkBehaviour {

    private List<IRewindEntity> _entities;

    public void Setup () {
		_entities = new List<IRewindEntity>();
	}

    [Server]
    public void AddEntity(IRewindEntity entity)
    {
        _entities.Add(entity);
    }

    /// <summary>
    /// Method called by <see cref="PlayerRewind.CmdUseRewind"/> to say we want to rewind the entire game.
    /// Sends the rewind request to every game object referenced as an <see cref="IRewindEntity"/> (interface only containing a method Rewind()).
    /// </summary>
    /// <param name="player"></param>
    [Server]
    public void Rewind(PlayerRewind player)
    {
        _entities.ForEach(e => e.Rewind());
        NGameManager.Instance.GenerationManager.RewindSceneryElements();
        NGameManager.Instance.TeamManager.UpdatePlayersRewindCooldown(player);
    }

    [Server]
    public void RemoveEntity(IRewindEntity entity)
    {
        _entities.Remove(entity);
    }

    void OnDestroy()
    {
        _entities.Clear();
    }
}
