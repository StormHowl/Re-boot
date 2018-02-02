using System;
using System.Collections.Generic;
using System.Linq;
using Rewind;
using TerrainGeneration;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public struct SceneElementRewindChange
{
    public int ListPosition;
    public bool State;

    public SceneElementRewindChange(int listPosition, bool state)
    {
        ListPosition = listPosition;
        State = state;
    }
}

/// <summary>
/// Big component to manage the terrain generation. Also manages the creation of spawn points, asked by 
/// <see cref="NGameManager.CreateSpawnPoints"/>. 
/// </summary>
[RequireComponent(typeof(TeamManager))]
public class GenerationManager : NetworkBehaviour
{
    public int SizeMapX = 5;
    public int SizeMapY = 5;

    public TerrainChunkSettings Settings;
    private TerrainChunk[,] _terrainChunks;

    private NoiseProvider _noiseProvider;
    private System.Random _random;

    private List<Vector3> treePositions;
    private List<Vector3> wallPositions;
    private Vector3[] spawnPoints;

    public const int StepBetweenWayPoints = 10;
    private WaypointsManager wpManager;

    private List<RewindableSceneryElement> _sceneElements;

    public int SeedX, SeedY;

    // Use this for initialization
    public void Setup()
    {
        _noiseProvider = new NoiseProvider();
        _terrainChunks = new TerrainChunk[SizeMapX, SizeMapY];
        _random = new System.Random();
        treePositions = new List<Vector3>();
        wallPositions = new List<Vector3>();
        _sceneElements = new List<RewindableSceneryElement>();
        wpManager = new WaypointsManager();
    }

    public void GenerateTerrain()
    {
        for (int i = 0; i < SizeMapX; ++i)
        {
            for (int j = 0; j < SizeMapY; ++j)
            {
                _terrainChunks[i, j] = new TerrainChunk(Settings, _noiseProvider, i, j, SeedX + i, SeedY + j);
                _terrainChunks[i, j].CreateTerrain();
            }
        }
    }

    [Server]
    public void GenerateTerrainAssets()
    {
        float maxHeight = 0.5f;

        //Big assets
        int nbBigAsset = Random.Range(2, 5);
        int nbPartPerChunk = 5;
        Vector3 position;
        int xChunk, zChunk;

        for (int i = 0; i < nbBigAsset; i++)
        {
            do
            {
                xChunk = Random.Range(0, SizeMapX);
                zChunk = Random.Range(0, SizeMapY);
                position = _terrainChunks[xChunk, zChunk].getPosInPartFree(nbPartPerChunk, maxHeight);
            } while (position.x == -1);

            position = new Vector3(xChunk * Settings.Length + position.x, position.y,
                zChunk * Settings.Length + position.z);
            Instantiate(NGameManager.Instance.Assets.GetBigAsset(), position, Quaternion.identity);
        }

        //Mediumassets
        int nbMediumAsset = Random.Range(10, 18);

        for (int i = 0; i < nbMediumAsset; i++)
        {
            do
            {
                xChunk = Random.Range(0, SizeMapX);
                zChunk = Random.Range(0, SizeMapY);
                position = _terrainChunks[xChunk, zChunk].getPosInPartFree(nbPartPerChunk, maxHeight);
            } while (position.x == -1);

            position = new Vector3(xChunk * Settings.Length + position.x, position.y,
                zChunk * Settings.Length + position.z);
            Instantiate(NGameManager.Instance.Assets.GetMediumAsset(), position, Quaternion.identity);
        }

        //Generate "standard" assets
        for (int i = 0; i < SizeMapX; ++i)
        {
            for (int j = 0; j < SizeMapY; ++j)
            {
                treePositions.AddRange(_terrainChunks[i, j].GenerateTerrainAssets(maxHeight).ConvertAll(p =>
                    new Vector3(i * Settings.Length + p.x, p.y, j * Settings.Length + p.z)));

                _terrainChunks[i, j].GenerateTerrainAssets(maxHeight).ForEach(p =>
                    Instantiate(NGameManager.Instance.Assets.GetNaturalAsset(),
                    new Vector3(i * Settings.Length + p.x, p.y-0.2f, j * Settings.Length + p.z), Quaternion.identity));

                wallPositions.AddRange(_terrainChunks[i, j].GenerateTerrainWalls(maxHeight).ConvertAll(p =>
                    new Vector3(i * Settings.Length + p.x, p.y, j * Settings.Length + p.z)));
            }
        }

        treePositions.ForEach(p =>
        {
            _sceneElements.Add(Instantiate(NGameManager.Instance.Assets.GetTree(), p, Quaternion.identity)
                .GetComponent<RewindableSceneryElement>());
        });
        wallPositions.ForEach(p =>
        {
            Instantiate(NGameManager.Instance.Assets.GetWall(), new Vector3(p.x, p.y + 1, p.z - 1.5f),
                Quaternion.identity);
            Instantiate(NGameManager.Instance.Assets.GetWall(), new Vector3(p.x - 1.5f, p.y + 1, p.z),
                Quaternion.Euler(0, 90, 0));
            Instantiate(NGameManager.Instance.Assets.GetWall(), new Vector3(p.x + 1.5f, p.y + 1, p.z),
                Quaternion.Euler(0, 90, 0));
            Instantiate(NGameManager.Instance.Assets.GetWall(), new Vector3(p.x, p.y + 1, p.z + 1.5f),
                Quaternion.identity);
        });
    }

    public Vector3[] GenerateSpawnPoints()
    {
        var first = GetFlatRandomLocation(new Vector2Int(0, 0));
        var second = GetFlatRandomLocation(first);
        spawnPoints = new[] {new Vector3(first.x, 1f, first.y), new Vector3(second.x, 1f, second.y)};
        return spawnPoints;
    }

    private Vector2Int GetFlatRandomLocation(Vector2Int butNot)
    {
        Vector2Int loc;
        float flatMin = 3.0f;
        int tries = 0;

        do
        {
            tries++;
            loc = new Vector2Int(Random.Range(0, SizeMapX * Settings.Length),
                Random.Range(0, SizeMapY * Settings.Length));

            if (tries > 100)
                flatMin = 1.0f;
        } while (Vector2Int.Distance(butNot, loc) > flatMin &&
                 !GetCorrespondingChunk(loc.x, loc.y)
                     .IsFlatAroundWithRadius(loc.x % Settings.Length, loc.y % Settings.Length, flatMin));

        return loc;
    }

    public TerrainChunk GetCorrespondingChunk(int x, int y)
    {
        return _terrainChunks[x / Settings.Length, y / Settings.Length];
    }

    public void GenerateWayPoints()
    {
        foreach (var terrainChunk in _terrainChunks)
        {
            var t = terrainChunk.GetWayPoints();
            NGameManager.Instance.AddWayPoints(t);
            wpManager.AddRange(t.Select(v => new Vector3(v.x, 0, v.z)).ToArray());
        }

        wpManager.CreateRelations();
    }

    public Vector3[] GetPathTo(Vector3 position, Vector3 destination)
    {
        var result = wpManager.GetPathBetween(position, destination);
        NGameManager.Instance.IlluminatePath(result);
        return result;
    }

    [Server]
    public void RewindSceneryElements()
    {
        List<SceneElementRewindChange> changes = new List<SceneElementRewindChange>();
        for (var i = 0; i < _sceneElements.Count; i++)
        {
            if (_sceneElements[i].GetStateBackTime(Time.time - RewindableEntity.RewindTime))
            {
                if (_sceneElements[i].LastState != SceneryStateEnum.CREATED)
                    changes.Add(new SceneElementRewindChange(i, true));
            }
            else
            {
                if (_sceneElements[i].LastState != SceneryStateEnum.DESTROYED)
                    changes.Add(new SceneElementRewindChange(i, false));
            }
        }

        RpcDoRewind(changes.ToArray());
    }

    [ClientRpc]
    public void RpcDoRewind(SceneElementRewindChange[] changes)
    {
        foreach (var change in changes)
        {
            _sceneElements[change.ListPosition].SetState(change.State);
        }
    }

    #region for clients

    [Command]
    public void CmdAskForGenerationAndAssets()
    {
        RpcGenerateTerrain(SeedX, SeedY);
        RpcAddWalls(wallPositions.ToArray());
        RpcAddTrees(treePositions.ToArray());
        RpcSpawnPoints(spawnPoints);
    }

    [ClientRpc]
    public void RpcGenerateTerrain(int seedX, int seedY)
    {
        SeedX = seedX;
        SeedY = seedY;

        GenerateTerrain();
    }

    [ClientRpc]
    private void RpcAddWalls(Vector3[] walls)
    {
        foreach (var p in walls)
        {
            Instantiate(NGameManager.Instance.Assets.GetWall(), new Vector3(p.x, p.y + 1, p.z - 1.5f),
                Quaternion.identity);
            Instantiate(NGameManager.Instance.Assets.GetWall(), new Vector3(p.x - 1.5f, p.y + 1, p.z),
                Quaternion.Euler(0, 90, 0));
            Instantiate(NGameManager.Instance.Assets.GetWall(), new Vector3(p.x + 1.5f, p.y + 1, p.z),
                Quaternion.Euler(0, 90, 0));
            Instantiate(NGameManager.Instance.Assets.GetWall(), new Vector3(p.x, p.y + 1, p.z + 1.5f),
                Quaternion.identity);
        }
    }

    [ClientRpc]
    private void RpcAddTrees(Vector3[] trees)
    {
        foreach (var p in trees)
        {
            Instantiate(NGameManager.Instance.Assets.GetTree(), p, Quaternion.identity);
        }
    }

    [ClientRpc]
    private void RpcSpawnPoints(Vector3[] points)
    {
        NGameManager.Instance.InitializeSpawnPoints(points);
    }

    public override void OnStartServer()
    {
        SeedX = Random.Range(0, 500);
        SeedY = SeedX;

        GenerateTerrain();
        GenerateTerrainAssets();
        GenerateWayPoints();

        NGameManager.Instance.InitializeSpawnPoints(GenerateSpawnPoints());
    }

    public override void OnStartClient()
    {
        CmdAskForGenerationAndAssets();
    }

    #endregion
}