
using System.Collections.Generic;
using UnityEngine;


public class MapGenerate : MonoBehaviour
{
    public GameObject villagePrefab;  
    public GameObject landPrefab;     
    public GameObject mountainPrefab; 

    public float prefabSpacing = 3f;  
    public int maxPrefabsPerRegion = 5; 
    public int maxSpawnAttempts = 10; 
    public LayerMask groundLayer;
    private List<Vector2> occupiedPositions = new List<Vector2>(); 
    private List<GameObject> spawnedPrefabs = new List<GameObject>();

    public void GeneratePrefabs(int[,] mergedMap, int width, int height, Color[,] colorMap)
    {

        ClearPrefabs();

        Dictionary<int, Vector2> regionCenters = CalculateRegionCenters(mergedMap, width, height);

        foreach (KeyValuePair<int, Vector2> entry in regionCenters)
        {
            int regionIndex = entry.Key;
            Vector2 regionCenter = entry.Value;
            Color regionColor = colorMap[(int)regionCenter.x, (int)regionCenter.y];

            GameObject prefabToInstantiate = GetPrefabForColor(regionColor);
            if (prefabToInstantiate == null)
            {
                continue;
            }
            int prefabCount = Mathf.RoundToInt(RandomGaussian.Range(1, maxPrefabsPerRegion));
            Debug.Log($"Prefab count for this region: {prefabCount}");

            for (int i = 0; i < prefabCount; i++)
            {
                Vector2 spawnPosition = GetValidSpawnPosition(mergedMap, regionIndex, width, height, prefabToInstantiate);

                spawnPosition = GetWorldPositionFromGridCoords((int)spawnPosition.x, (int)spawnPosition.y, width, height);

                if (spawnPosition != Vector2.zero)
                {
                    RaycastHit hit;
                    Ray ray = new Ray(new Vector3(spawnPosition.x, 1000f, spawnPosition.y), Vector3.down);
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
                    {
                        Vector3 groundPosition = hit.point;

                        GameObject spawnedPrefab = Instantiate(prefabToInstantiate, new Vector3(groundPosition.x, groundPosition.y, groundPosition.z), Quaternion.identity);

                        if (IsCollidingWithOtherPrefabs(spawnedPrefab))
                        {
                            Destroy(spawnedPrefab);
                        }
                        else
                        {
                            occupiedPositions.Add(new Vector2(groundPosition.x, groundPosition.z));
                            spawnedPrefabs.Add(spawnedPrefab);
                        }
                    }
                }
            }
        }
    }
    private Dictionary<int, Vector2> CalculateRegionCenters(int[,] mergedMap, int width, int height)
    {
        Dictionary<int, Vector2> regionCenters = new Dictionary<int, Vector2>();
        Dictionary<int, int> regionCounts = new Dictionary<int, int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = mergedMap[x, y];

                if (!regionCounts.ContainsKey(index))
                {
                    regionCounts[index] = 0;
                    regionCenters[index] = Vector2.zero;
                }

                regionCounts[index]++;
                regionCenters[index] += new Vector2(x, y);
            }
        }

        Dictionary<int, Vector2> updatedCenters = new Dictionary<int, Vector2>();
        foreach (var entry in regionCenters)
        {
            int regionIndex = entry.Key;
            Vector2 center = entry.Value / regionCounts[regionIndex];
            updatedCenters[regionIndex] = center;
        }

        return updatedCenters;
    }

    private GameObject GetPrefabForColor(Color regionColor)
    {
        if (regionColor == Color.red)
        {
            return villagePrefab;
        }
        else if (regionColor == Color.yellow)
        {
            return landPrefab;
        }
        else if (regionColor == Color.gray)
        {
            return mountainPrefab;
        }
        else
        {
            return null;
        }
    }

    private Vector2 GetValidSpawnPosition(int[,] mergedMap, int regionIndex, int width, int height, GameObject prefab)
    {
        List<Vector2> availablePositions = new List<Vector2>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mergedMap[x, y] == regionIndex)
                {
                    availablePositions.Add(new Vector2(x, y));
                }
            }
        }

        if (availablePositions.Count == 0) return Vector2.zero;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            int selectedIndex = Mathf.RoundToInt(RandomGaussian.Range(0, availablePositions.Count - 1));
            Vector2 selectedPosition = availablePositions[selectedIndex];

            if (EnsurePrefabSpacing(selectedPosition, prefab))
            {
                return selectedPosition;
            }
        }
        return Vector2.zero;
    }
    private bool IsCollidingWithOtherPrefabs(GameObject spawnedPrefab)
    {
        Collider collider = spawnedPrefab.GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning("Prefab does not have a Collider.");
            return false;
        }

        Collider[] colliders = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, Quaternion.identity);

        foreach (var otherCollider in colliders)
        {
            if (otherCollider.gameObject != spawnedPrefab && otherCollider.gameObject.CompareTag("Prefab"))
            {
                return true; 
            }
        }

        return false;
    }

    private bool EnsurePrefabSpacing(Vector2 position, GameObject prefab)
    {
        float prefabSize = prefab.GetComponent<Renderer>().bounds.extents.x; 
        foreach (Vector2 occupied in occupiedPositions)
        {
            if (Vector2.Distance(occupied, position) < prefabSize + prefabSpacing)
            {
                return false;
            }
        }
        return true;
    }

    public Vector2 GetWorldPositionFromGridCoords(int x, int y, int width, int height)
    {
        Renderer quadRenderer = GetComponent<Renderer>();
        Vector3 quadPosition = quadRenderer.transform.position;
        Vector3 quadScale = quadRenderer.transform.localScale;

        float worldX = quadPosition.x + (x / (float)width - 0.5f) * quadScale.x;
        float worldZ = quadPosition.z + (y / (float)height - 0.5f) * quadScale.z;

        return new Vector2(worldX, worldZ);
    }
    private void ClearPrefabs()
    {
        foreach (GameObject prefab in spawnedPrefabs)
        {
            if (prefab != null)
            {
                Destroy(prefab);
            }
        }

        occupiedPositions.Clear();
        spawnedPrefabs.Clear();
    }
}

public static class RandomGaussian
{
    public static float Range(float min, float max)
    {
        float u1 = Random.value;
        float u2 = Random.value;

        float standardGaussian = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);

        float mean = (min + max) / 2f;
        float stdDev = (max - min) / 6f;

        return Mathf.Clamp(mean + standardGaussian * stdDev, min, max);
    }
}