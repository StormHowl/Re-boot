using UnityEngine;

namespace TerrainGeneration
{
    [System.Serializable]
    public class TerrainChunkSettings
    {
        public int HeightmapResolution = 129;
        public int AlphamapResolution = 129;
        public int Length = 100;
        public int Height = 40;
        public Texture2D FlatTexture;
        public Texture2D SteepTexture;
        public Material TerrainMaterial;
    }
}