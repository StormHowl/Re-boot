using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{

    public static GameManager Instance = null;

    public GameObject terrain;

    private List<IRewindEntity> _entities;

    void Start()
    {
        Instantiate(terrain, new Vector3(0, 0, 0), Quaternion.identity);
        this._entities = new List<IRewindEntity>();
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

    [Server]
    public void AddEntity(IRewindEntity entity)
    {
        _entities.Add(entity);
    }

    [Server]
    public void Rewind()
    {
        _entities.ForEach(e => e.Rewind());
    }
}
