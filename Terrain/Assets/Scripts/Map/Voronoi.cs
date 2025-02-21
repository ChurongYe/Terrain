
using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour
{
    public int seed;
    public MapGenerate mapGenerate;
    [Header("Map")]
    public int width = 256;
    public int height = 256;
    public int[] units = { 4, 3, 2 };

    [Header("Color")]
    [Range(0f, 1f)] public float redProbability = 0.2f;
    [Range(0f, 1f)] public float yellowProbability = 0.2f;
    [Range(0f, 1f)] public float grayProbability = 0.2f;

    private Renderer quadRenderer;
    private Dictionary<Vector2Int, int> cellIndexCache = new Dictionary<Vector2Int, int>();
    public void GenerateTexture()
    {
        int[,] voronoiMap = GenerateVoronoi(width, height);
        Dictionary<int, int> regionSizes = ComputeRegionSizes(voronoiMap); 
        int totalArea = width * height;  
        int areaOneTwentieth = totalArea / 100;  
        int[,] mergedMap = MergeSmallRegions(voronoiMap, regionSizes, areaOneTwentieth);

        Color[,] colorMap = new Color[width, height];

        Texture2D voronoiTexture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = mergedMap[x, y];
                Color color = GetCellColor(index, x, y);
                colorMap[x, y] = color;
                voronoiTexture.SetPixel(x, y, color);
            }
        }

        mapGenerate.GeneratePrefabs(mergedMap, width, height, colorMap);

        voronoiTexture.Apply();
        quadRenderer = GetComponent<Renderer>();
        if (quadRenderer != null)
        {
            quadRenderer.material.mainTexture = voronoiTexture;
        }

    }
    public Color GetCellColor(int index, int x, int y)
    {
        float centerBias = GetCenterBias(x, y);

        float probability = (index % 10) / 10f;

        float adjustedYellowProbability = yellowProbability + centerBias * 0.3f;
        float totalProbability = redProbability + adjustedYellowProbability + grayProbability;
        if (IsNearBorder(x, y, 0.1f))
        {
            adjustedYellowProbability = 0f; 
            totalProbability = redProbability + grayProbability; 
        }
        if (probability < redProbability / totalProbability)
        {
            return Color.red;
        }
        else if (probability < (redProbability + adjustedYellowProbability) / totalProbability)
        {
            return Color.yellow;
        }
        else
        {
            return Color.gray;
        }
    }
    private float GetCenterBias(int x, int y)
    {
        float centerX = width / 2f;
        float centerY = height / 2f;
        float maxDistance = Mathf.Sqrt(centerX * centerX + centerY * centerY);
        float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

        float bias = 1f - (distance / maxDistance);
        return bias;
    }
    private bool IsNearBorder(int x, int y, float borderRatio)
    {
        int borderSizeX = (int)(width * borderRatio);
        int borderSizeY = (int)(height * borderRatio);

        return (x < borderSizeX || x >= width - borderSizeX ||
                y < borderSizeY || y >= height - borderSizeY);
    }
    private Dictionary<int, int> ComputeRegionSizes(int[,] voronoiMap)
    {
        Dictionary<int, int> regionSizes = new Dictionary<int, int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = voronoiMap[x, y];
                if (!regionSizes.ContainsKey(index))
                    regionSizes[index] = 0;
                regionSizes[index]++;
            }
        }
        return regionSizes;
    }
    private int[,] MergeSmallRegions(int[,] voronoiMap, Dictionary<int, int> regionSizes, int minSize)
    {
        int[,] mergedMap = (int[,])voronoiMap.Clone();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = voronoiMap[x, y];
                if (regionSizes[index] < minSize && !visited.Contains(new Vector2Int(x, y)))
                {
                    int newIndex = FindNearestLargerRegion(voronoiMap, regionSizes, x, y);

                    FloodFillMerge(mergedMap, x, y, index, newIndex, visited);
                }
            }
        }

        return mergedMap;
    }

    private int FindNearestLargerRegion(int[,] voronoiMap, Dictionary<int, int> regionSizes, int x, int y)
    {
        int maxSearchRadius = 10;
        for (int r = 1; r <= maxSearchRadius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;

                    int neighborIndex = voronoiMap[nx, ny];

                    if (regionSizes.ContainsKey(neighborIndex) && regionSizes[neighborIndex] >= 20)
                    {
                        return neighborIndex;  
                    }
                }
            }
        }
        return voronoiMap[x, y];
    }

    private void FloodFillMerge(int[,] voronoiMap, int startX, int startY, int oldIndex, int newIndex, HashSet<Vector2Int> visited)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited.Add(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            int x = pos.x, y = pos.y;

            if (x < 0 || y < 0 || x >= width || y >= height || voronoiMap[x, y] != oldIndex)
                continue;

            voronoiMap[x, y] = newIndex;

            foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int nextPos = new Vector2Int(x + dir.x, y + dir.y);
                if (!visited.Contains(nextPos))
                {
                    queue.Enqueue(nextPos);
                    visited.Add(nextPos);
                }
            }
        }
    }
    public int[,] GenerateVoronoi(int width, int height)
    {
        int[,] voronoiMap = new int[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                voronoiMap[x, y] = GetVoronoiIndexAt(0, x, y);
            }
        }
        return voronoiMap;
    }

    private int GetVoronoiIndexAt(int level, int x, int y)
    {
        if (level == units.Length - 1)
        {
            return GetClosestRootIndexFrom(level, x, y);
        }
        var next = GetClosestRootPositionFrom(level, x, y);
        return GetVoronoiIndexAt(level + 1, next.x, next.y);
    }

    private int GetCellRootIndex(int level, int cell_x, int cell_y)
    {
      Vector2Int key = new Vector2Int(cell_x, cell_y);
        if (!cellIndexCache.TryGetValue(key, out int index))
        {
            var rand = new System.Random(cell_x ^ cell_y * 0x123456 + level + (seed << 2) + 66666);
            index = rand.Next(256);
            cellIndexCache[key] = index;
        }
        return index;
    }

    private Vector2Int GetCellRootPosition(int level, int cell_x, int cell_y)
    {
        int hash = (cell_x * 73856093) ^ (cell_y * 19349663) ^ (level * 83492791) ^ seed;

        int pseudoRandomX = (hash >> 16) & 0xFF;
        int pseudoRandomY = (hash >> 8) & 0xFF;

        int unitSize = 1 << units[level];

        return new Vector2Int(
            (cell_x << units[level]) + (pseudoRandomX % unitSize),
            (cell_y << units[level]) + (pseudoRandomY % unitSize)
        );
    }

    private int GetClosestRootIndexFrom(int level, int x, int y)
    {
        int cx = x >> units[level], cy = y >> units[level];
        var min_dist = int.MaxValue;
        var ret = 0;

        for (var i = -1; i <= 1; ++i)
        {
            for (var j = -1; j <= 1; ++j)
            {
                int ax = cx + i, ay = cy + j;
                var pos = GetCellRootPosition(level, ax, ay);
                var index = GetCellRootIndex(level, ax, ay);
                var dist = DistanceSqr(pos, new Vector2Int(x, y));

                if (dist < min_dist)
                {
                    min_dist = dist;
                    ret = index;
                }
            }
        }
        return ret;
    }

    private Vector2Int GetClosestRootPositionFrom(int level, int x, int y)
    {
        int cx = x >> units[level], cy = y >> units[level];
        var min_dist = int.MaxValue;
        var ret = Vector2Int.zero;

        for (var i = -1; i <= 1; ++i)
        {
            for (var j = -1; j <= 1; ++j)
            {
                int ax = cx + i, ay = cy + j;
                var pos = GetCellRootPosition(level, ax, ay);
                var dist = DistanceSqr(pos, new Vector2Int(x, y));

                if (dist < min_dist)
                {
                    min_dist = dist;
                    ret = pos;
                }
            }
        }
        return ret;
    }

    private static int DistanceSqr(Vector2Int a, Vector2Int b)
    {
        return (a - b).sqrMagnitude;
    }

}
