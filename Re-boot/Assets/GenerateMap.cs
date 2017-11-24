using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour {

	public int width=100, height=100, depth = 256;
    private Terrain terrain;
	private float [,] heights;
	public float scale = 0.2f;
	public float frequency = 0.1f;
	public GameObject tree, rock;
	public float offSetX=10.213f, offSetY=10.123f;
	public int octave=3;
	public float persistance=2;

	float CalculateHeights(int x, int y){
		float h = 0;
		float s,f;
		f = frequency;
		s = scale;
		for (int i = 0; i < octave; i++) {
			h += s*Mathf.PerlinNoise (x * f + offSetX, y * f + offSetY);
			s = s / persistance;
			f = f * persistance;
		}
		return h * scale;
	}

	void GenerateTrees(int w, int h){
		int octave2=2;
		float persistance2 = -0.38f;
		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				float h2 = 0;
				float s, f;
				f = 0.19f;
				s = 0.7f;
				for (int i = 0; i < octave2; i++) {
					h2 += s * Mathf.PerlinNoise (x * f + offSetX, y * f + offSetY);
					s = s / persistance2;
					f = f * persistance2;
				}
				if(h2 > 0){
                    Instantiate(tree, new Vector3(x, CalculateHeights(x, y)*depth, y), Quaternion.identity);
                    Instantiate(tree, new Vector3(x, CalculateHeights(x, height - y - 1) *depth, height-y-1), Quaternion.identity);
                    x += 2;
					y += 2;
				}
			}
		}
	}

	// Use this for initialization
	void Start ()
    {
        terrain = GetComponent<Terrain>();
		heights = new float[width,height];
        terrain.terrainData.size = new Vector3(width, depth, height);
        terrain.terrainData.heightmapResolution = width + 1;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height/2; j++)
            {
                heights[i, j] = CalculateHeights(i, j);
                heights[i, width-j-1] = CalculateHeights(i, j);
            }
            heights[i, width / 2] = CalculateHeights(i, width / 2);
        }
        terrain.terrainData.SetHeights(0, 0, heights);
        GenerateTrees(width, height/2);
    }

	// Update is called once per frame
	void Update () {
		
    }


}
