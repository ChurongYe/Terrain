using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TerrainControl : MonoBehaviour
{
    [Header("Texture")]
    public Renderer Renderer;
    public int Size;

    [Header("Perlin Noise")]
    public float OffsetX;
    public float OffsetY;
    [Min(0)]
    public float Scale;

    [Header("fBM Noise")]
    [Min(0)]
    public int Octaves = 8;
    [Range(0f, 1f)]
    public float Gain = 0.5f;

    [Space]
    [Range(0f, 10f)]
    public float Power;

    [Header("Maps")]
    private float[,] HeightMap; // [x,y] = height
    private bool[,] WaterMap;   // [x,y] = true if it is water (=river only)


    [Header("Water")]
    public AnimationCurve IslandCurve;

    [Range(0f, 1f)]
    public float WaterThreshold;
    public Color WaterColor;

    [Header("Colour")]
    public Gradient HeightGradient;

    [Header("River")]
    [Min(0)]
    public int RiverCount = 10;
    [Min(0)]
    public int RiverLength;

    [Header("Unity Terrain")]
    public Terrain Terrain;
    public TerrainLayer TerrainLayer;
    private Texture2D Texture;

    void Start()
    {
        Renderer = GetComponent<Renderer>();
        HeightMap = FBMNoise.Generate(Size, Size, OffsetX, OffsetY, Scale, Octaves, Gain);

        PowerPass();

        IslandPass();

        WaterMap = new bool[Size, Size];


        for (int i = 0; i < RiverCount; i++)
        {
            Vector2Int riverStart = new Vector2Int(
                Random.Range((int)(Size * 0.1f), (int)(Size * 0.8f)),
                Random.Range((int)(Size * 0.1f), (int)(Size * 0.8f))
                );
            RiverPass(riverStart.x, riverStart.y);
        }

        TexturePass();

        TerrainPass();
    }

    private void PowerPass()
    {
        int w = HeightMap.GetLength(0);
        int h = HeightMap.GetLength(1);

        float min = HeightMap.Cast<float>().Min();
        float max = HeightMap.Cast<float>().Max();

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float height = (HeightMap[x, y] - min) / (max - min);
                HeightMap[x, y] = Mathf.Pow(height, Power);
            }
    }

    private void IslandPass()
    {
        int w = HeightMap.GetLength(0);
        int h = HeightMap.GetLength(1);

        Vector2 center = new Vector2(w / 2f, h / 2f);

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                Vector2 position = new Vector2(x, y);
                float distance = Vector2.Distance(center, position);

                HeightMap[x, y] *= IslandCurve.Evaluate(distance / (Size / 2f));
            }
    }

    private void RiverPass(int x, int y)
    {
        // The directions in which the river can flow
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, +1),
            new Vector2Int(0, -1),
            new Vector2Int(+1, 0),
            new Vector2Int(-1, 0),

            new Vector2Int(+1, +1),
            new Vector2Int(+1, -1),
            new Vector2Int(-1, +1),
            new Vector2Int(-1, -1)
        };

        Vector2Int position = new Vector2Int(x, y);
        for (int i = 0; i < RiverLength; i++)
        {
            WaterMap[position.x, position.y] = true;

            var dd = directions.Where
                (d =>
                {
                    Vector2Int target = position + d;
                    return !WaterMap[target.x, target.y];
                }
                );
            // No directions left?
            if (dd.Count() == 0)
                break;
            Vector2Int direction = dd
    .OrderBy(d =>
    {
        Vector2Int target = position + d;
        return HeightMap[target.x, target.y]; 
    })
    .First();

            position += direction;

            // Stops if we touched the ocean
            if (HeightMap[position.x, position.y] <= WaterThreshold)
                break;
        }

    }

    private void TexturePass()
    {
        int w = HeightMap.GetLength(0);
        int h = HeightMap.GetLength(1);

        Texture = new Texture2D(Size, Size);
        Texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[w * h];
        int i = 0;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color c;

                if (WaterMap[x, y])
                    c = WaterColor;
                else
                {
                    float height = HeightMap[x, y];
                    c = HeightGradient.Evaluate(height);
                }

                pixels[i++] = c;
            }

        Texture.SetPixels(pixels);
        Texture.Apply();

        Renderer.material.mainTexture = Texture;
    }

    private void TerrainPass()
    {
        int w = HeightMap.GetLength(0);
        int h = HeightMap.GetLength(1);

        float[,] heights = new float[h, w];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                heights[x, y] = HeightMap[y, x];


        Terrain.terrainData.SetHeights(0, 0, heights);

        // Sets the texture
        TerrainLayer.diffuseTexture = Texture;
        TerrainLayer.tileSize = new Vector2(Size, Size);
        TerrainData terrainData = Terrain.terrainData;

    }
}
