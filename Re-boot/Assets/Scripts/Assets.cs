using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Assets
{

    public List<GameObject> Trees;
    public List<GameObject> Walls;
    public List<GameObject> Natural;
    public List<GameObject> Mediums;
    public List<GameObject> Bigs;

    public GameObject GetTree()
    {
        return Trees[Random.Range(0, Trees.Count)];
    }
    public GameObject GetWall()
    {
        return Walls[Random.Range(0, Walls.Count)];
    }

    public GameObject GetNaturalAsset()
    {
        return Natural[Random.Range(0, Natural.Count)];
    }

    public GameObject GetMediumAsset()
    {
        return Mediums[Random.Range(0, Mediums.Count)];
    }

    public GameObject GetBigAsset()
    {
        return Bigs[Random.Range(0, Bigs.Count)];
    }
}
