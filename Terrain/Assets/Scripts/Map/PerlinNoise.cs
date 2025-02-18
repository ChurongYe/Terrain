using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public static float Sample(float x, float y)
    {
        return Mathf.Clamp01(Mathf.PerlinNoise(x, y));
    }

    #region Matrix
    public static void Generate(float[,] map,
        float offsetX = 0f, float offsetY = 0f,
        float scale = 1f
        )
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float xCoord = offsetX + x / (float)w * scale;
                float yCoord = offsetY + y / (float)h * scale;
                map[x, y] = Sample(xCoord, yCoord);
            }
    }

    // Instantiates the matrix
    public static float[,] Generate(int w, int h,
        float offsetX = 0f, float offsetY = 0f,
        float scale = 1f
        )
    {
        float[,] map = new float[w, h];

        Generate(map, offsetX, offsetY, scale);

        return map;
    }
    #endregion


   
}

