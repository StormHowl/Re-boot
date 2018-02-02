using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using LibNoise.Generator;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TerrainGeneration
{
    public class TerrainChunk
    {
        public int X { get; private set; }

        public int Z { get; private set; }

        public int SeedX, SeedY;

        private Terrain Terrain { get; set; }
        private TerrainData TerrainData { get; set; }

        private TerrainChunkSettings Settings { get; set; }

        private NoiseProvider NoiseProvider { get; set; }

        private System.Random _random;

        private float[,] _heightMap;

        private object HeightmapThreadLockObject { get; set; }

        public TerrainChunk(TerrainChunkSettings settings, NoiseProvider noiseProvider, int x, int z, int indexX,
            int indexY)
        {
            HeightmapThreadLockObject = new object();

            Settings = settings;
            NoiseProvider = noiseProvider;
            _random = new System.Random();

            X = x;
            Z = z;

            SeedX = indexX;
            SeedY = indexY;
        }

        public void GenerateHeightmap()
        {
            var thread = new Thread(GenerateHeightmapThread);
            thread.Start();
        }

        private void GenerateHeightmapThread()
        {
            lock (HeightmapThreadLockObject)
            {
                var heightmap = new float[Settings.HeightmapResolution, Settings.HeightmapResolution];

                for (var zRes = 0; zRes < Settings.HeightmapResolution; zRes++)
                {
                    for (var xRes = 0; xRes < Settings.HeightmapResolution; xRes++)
                    {
                        var xCoordinate = SeedX + (float) xRes / (Settings.HeightmapResolution - 1);
                        var zCoordinate = SeedY + (float) zRes / (Settings.HeightmapResolution - 1);

                        heightmap[zRes, xRes] = NoiseProvider.GetValue(xCoordinate, zCoordinate);
                    }
                }

                _heightMap = heightmap;
            }
        }

        public void CreateTerrain()
        {
            GenerateHeightmapThread();
            TerrainData = new TerrainData();
            TerrainData.heightmapResolution = Settings.HeightmapResolution;
            TerrainData.alphamapResolution = Settings.AlphamapResolution;
            TerrainData.SetHeights(0, 0, _heightMap);
            ApplyTextures(TerrainData);

            TerrainData.size = new Vector3(Settings.Length, Settings.Height, Settings.Length);
            var newTerrainGameObject = Terrain.CreateTerrainGameObject(TerrainData);
            newTerrainGameObject.transform.position = new Vector3(X * Settings.Length, 0, Z * Settings.Length);

            Terrain = newTerrainGameObject.GetComponent<Terrain>();
            Terrain.heightmapPixelError = 8;
            Terrain.materialType = UnityEngine.Terrain.MaterialType.Custom;
            Terrain.materialTemplate = Settings.TerrainMaterial;
            Terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            Terrain.Flush();
        }

        private void ApplyTextures(TerrainData terrainData)
        {
            var flatSplat = new SplatPrototype();
            var steepSplat = new SplatPrototype();

            flatSplat.texture = Settings.FlatTexture;
            steepSplat.texture = Settings.SteepTexture;

            terrainData.splatPrototypes = new SplatPrototype[]
            {
                flatSplat,
                steepSplat
            };

            terrainData.RefreshPrototypes();

            var splatMap = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, 2];

            for (var zRes = 0; zRes < terrainData.alphamapHeight; zRes++)
            {
                for (var xRes = 0; xRes < terrainData.alphamapWidth; xRes++)
                {
                    var normalizedX = (float) xRes / (terrainData.alphamapWidth - 1);
                    var normalizedZ = (float) zRes / (terrainData.alphamapHeight - 1);

                    var steepness = terrainData.GetSteepness(normalizedX, normalizedZ);
                    var steepnessNormalized = Mathf.Clamp(steepness / 1.5f, 0, 1f);

                    splatMap[zRes, xRes, 0] = 1f - steepnessNormalized;
                    splatMap[zRes, xRes, 1] = steepnessNormalized;
                }
            }

            terrainData.SetAlphamaps(0, 0, splatMap);
        }

        private float MeanHeight(int x, int y, int size)
        {
            float val = 0;
            int qty = 0;
            int startx = x * size, starty = y * size;

            for (int i = startx; i < startx + size; ++i)
            {
                for (int j = starty; j < starty + size; ++j)
                {
                    var height = GetHeight(i, j);
                    val += height;
                    qty++;
                }
            }

            return Mathf.Abs(val / qty);
        }

        private float MeanSlope(int x, int y, int size)
        {
            int startx = x * size, starty = y * size;
            int endx = startx + size - 1, endy = starty + size - 1;

            float heightCorner1 = GetHeight(startx, starty);
            float heightCorner2 = GetHeight(startx, endy);
            float heightCorner3 = GetHeight(endx, starty);
            float heightCorner4 = GetHeight(endx, endy);

            float slope12 = Mathf.Abs(heightCorner1 - heightCorner2);
            float slope13 = Mathf.Abs(heightCorner1 - heightCorner3);
            float slope14 = Mathf.Abs(heightCorner1 - heightCorner4);
            float slope23 = Mathf.Abs(heightCorner2 - heightCorner3);
            float slope24 = Mathf.Abs(heightCorner2 - heightCorner4);
            float slope34 = Mathf.Abs(heightCorner3 - heightCorner4);

            return Mathf.Abs(slope12 + slope13 + slope14 + slope23 + slope24 + slope34 / 6.0f);
        }

        private float GetHeight(int x, int y)
        {
            int valx = (int) (((float) x * Settings.HeightmapResolution) / Settings.Length),
                valy = (int) (((float) y * Settings.HeightmapResolution) / Settings.Length);
            return TerrainData.GetHeight(valx, valy);
        }

        public List<Vector3> GenerateTerrainAssets(float maxHeight)
        {
            int parts = 5;
            int step = Settings.Length / parts;

            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i < parts; ++i)
            {
                for (int j = 0; j < parts; ++j)
                {
                    if (MeanHeight(i, j, step) < maxHeight)
                    {
                        points.AddRange(GenerateTrees(Random.Range(4, 20), Random.Range(5, 20),
                                new Vector2Int(i * step, j * step), new Vector2Int(step, step))
                            .ConvertAll(p => new Vector3(p.x,
                                GetHeight((int) p.x, (int) p.y), p.y)));
                    }
                }
            }

            return points;
        }

        // returns a position in the chunk on which we can generate asset 
        // returns (-1,-1, -1) if we don't find a good part
        public Vector3 getPosInPartFree(int parts, float maxHeight)
        {
            int step = Settings.Length / parts;
            int xRes, zRes;
            for (int i = 0; i < parts; ++i)
            {
                for (int j = 0; j < parts; ++j)
                {
                    if (MeanHeight(i, j, step) < maxHeight && MeanSlope(i, j, step) == 0)
                    {
                        xRes = Random.Range(i * step, i * step + step);
                        zRes = Random.Range(j * step, j * step + step);

                        return new Vector3(xRes, GetHeight(xRes, zRes), zRes);
                    }
                }
            }

            return new Vector3(-1, -1, -1);
        }

        public List<Vector3> GenerateTerrainWalls(float maxHeight)
        {
            int parts = 5;
            int step = Settings.Length / parts;
            List<Vector3> points = new List<Vector3>();
            System.Random rnd = new System.Random();
            //between (130,75) and (140,210)
            for (int i = 0; i < parts; ++i)
            {
                for (int j = 0; j < parts; ++j)
                {
                    if (MeanHeight(i, j, step) < maxHeight && MeanSlope(i, j, step) == 0)
                    {
                        points.AddRange(GenerateRuins(i, j, step));
                    }
                }
            }

            return points;
        }

        public List<Vector2> GenerateTrees(float min_dist, int new_points_count, Vector2Int start, Vector2Int size)
        {
            Vector2 startingPoint = new Vector2(Random.Range(start.x, start.x + size.x),
                Random.Range(start.y, start.y + size.y));
            List<Vector2> positions = new List<Vector2>() {startingPoint};
            Queue<Vector2> processList = new Queue<Vector2>();
            processList.Enqueue(startingPoint);

            while (processList.Count > 0)
            {
                var point = processList.Dequeue();
                for (int i = 0; i < new_points_count; ++i)
                {
                    var newpoint = GenerateRandomPoint(point, min_dist);
                    if (CheckInCoordinates(newpoint, start, size) &&
                        CheckHasNoNeighbourhood(positions, newpoint, min_dist))
                    {
                        positions.Add(newpoint);
                        processList.Enqueue(newpoint);
                    }
                }
            }

            return positions;
        }

        //to check if random position are valid (if there are not at a position of walls previously placed)
        public bool CheckRuinsPos(List<Vector3> positions, int xPos, int zPos, int lengthWall)
        {
            foreach (Vector3 pos in positions)
            {
                if (xPos > pos.x - lengthWall && xPos < pos.x + lengthWall && zPos > pos.z - lengthWall &&
                    zPos < pos.z + lengthWall)
                    return false;
            }

            return true;
        }

        public List<Vector3> GenerateRuins(int xPartPos, int zPartPos, int lengthPart)
        {
            int maxConstruction = 1;
            int nbConstruction = Random.Range(0, maxConstruction + 1);
            List<Vector3> newPos = new List<Vector3>();

            //calculate the position of the part in the world
            int xPos = xPartPos * lengthPart;
            int zPos = zPartPos * lengthPart;

            int lengthWall = 3;
            int xPlacement, zPlacement;

            for (int i = 0; i < nbConstruction; i++)
            {
                do
                {
                    xPlacement = Random.Range(xPos + (lengthWall / 2), xPos + lengthPart - lengthWall / 2);
                    zPlacement = Random.Range(zPos + (lengthWall / 2), zPos + lengthPart - lengthWall / 2);
                } while (!CheckRuinsPos(newPos, xPlacement, zPlacement, lengthWall));

                newPos.Add(new Vector3(xPlacement, GetHeight(xPlacement, zPlacement), zPlacement));
            }

            return newPos;
        }

        private Vector2 GenerateRandomPoint(Vector2 point, float mindist)
        {
            //non-uniform, favours points closer to the inner ring, leads to denser packings
            double r1 = _random.NextDouble(); //random point between 0 and 1
            double r2 = _random.NextDouble();
            //random radius between mindist and 2 * mindist
            double radius = mindist * (r1 + 1);
            //random angle
            double angle = 2 * Mathf.PI * r2;
            //the new point is generated around the point (x, y)
            double newX = point.x + radius * Math.Cos(angle);
            double newY = point.y + radius * Math.Sin(angle);
            return new Vector2((int) newX, (int) newY);
        }

        private bool CheckInCoordinates(Vector2 point, Vector2 start, Vector2 size)
        {
            return point.x > start.x && point.x < start.x + size.x && point.y > start.y &&
                   point.y < start.y + size.y;
        }

        private bool CheckHasNoNeighbourhood(List<Vector2> points, Vector2 point, float mindist)
        {
            foreach (var p in points)
            {
                if (Vector2.Distance(p, point) > mindist)
                    return false;
            }

            return true;
        }

        public Vector3 GetCenterPosition()
        {
            int x = X * Settings.Length + Settings.Length / 2, z = Z * Settings.Length + Settings.Length / 2;
            return new Vector3(x, GetHeight(Settings.Length / 2, Settings.Length / 2), z);
        }

        public Vector3[] GetWayPoints()
        {
            List<Vector3> waypoints = new List<Vector3>();

            for (int i = 0; i < Settings.Length; i += GenerationManager.StepBetweenWayPoints)
            {
                for (int j = 0; j < Settings.Length; j += GenerationManager.StepBetweenWayPoints)
                {
                    if (GetHeight(i, j) < 2.0f)
                    {
                        waypoints.Add(new Vector3(X * Settings.Length + i, GetHeight(i, j), Z * Settings.Length + j));
                    }
                }
            }

            return waypoints.ToArray();
        }

        public bool IsFlatAroundWithRadius(int x, int y, float radius)
        {
            if (!(x > radius) || !(x < Settings.Length - radius) || !(y > radius) ||
                !(y < Settings.Length - radius)) return false;

            float height = GetHeight(x, y);
            float limit = 0.2f;
            return HeightBetween(height - limit, height + limit, GetHeight((int) (x + radius), y)) &&
                   HeightBetween(height - limit, height + limit, GetHeight((int) (x - radius), y)) &&
                   HeightBetween(height - limit, height + limit, GetHeight(x, (int) (y - radius))) &&
                   HeightBetween(height - limit, height + limit, GetHeight(x, (int) (y + radius)));

        }

        private bool HeightBetween(float a, float b, float height)
        {
            Debug.Log("Height : "+ height);
            return height > a && height < b;
        }
    }
}