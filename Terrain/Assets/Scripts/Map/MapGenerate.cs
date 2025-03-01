
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapGenerate : MonoBehaviour
{
    public GameObject[] villagePrefab;  
    public GameObject[] landPrefab;     
    public GameObject[] mountainPrefab;
    public int margin = 10;
    public float prefabSpacing = 3f;  
    public int maxPrefabsPerRegion = 5; 
    public int maxSpawnAttempts = 10; 
    public LayerMask groundLayer;
    public Voronoi Voronoi;
    private List<Vector2> occupiedPositions = new List<Vector2>(); 
    private List<GameObject> spawnedPrefabs = new List<GameObject>();

    public IEnumerator GeneratePrefabs(int[,] mergedMap, int width, int height, Color[,] colorMap)
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

            bool prefabGenerated = false;

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
                        float randomScaleFactor = Random.Range(0.9f, 1.2f);
                        int maxScaleAttempts = 5;
                        while (randomScaleFactor >= 0.1f && maxScaleAttempts > 0)
                        {
                            GameObject spawnedPrefab = Instantiate(prefabToInstantiate, groundPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                            spawnedPrefab.transform.localScale = prefabToInstantiate.transform.localScale * randomScaleFactor;
                            NavTerrain navTerrain = spawnedPrefab.GetComponent<NavTerrain>();
                            if (navTerrain != null)
                            {
                                navTerrain.randomScaleFactor = randomScaleFactor;
                            }
                            if (!IsCollidingWithOtherPrefabs(spawnedPrefab))
                            {
                                occupiedPositions.Add(new Vector2(groundPosition.x, groundPosition.z));
                                spawnedPrefabs.Add(spawnedPrefab);
                                prefabGenerated = true;
                                break;
                            }
                            else
                            {
                                Destroy(spawnedPrefab);
                                randomScaleFactor = Mathf.Max(randomScaleFactor * 0.8f, 0.1f);
                                maxScaleAttempts--;
                            }
                        }
                    
                    }
                }
            }

            if (!prefabGenerated)
            {
                Vector2 fallbackPosition = GetWorldPositionFromGridCoords((int)regionCenter.x, (int)regionCenter.y, width, height);
                RaycastHit hit;
                Ray ray = new Ray(new Vector3(fallbackPosition.x, 1000f, fallbackPosition.y), Vector3.down);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
                {
                    Vector3 groundPosition = hit.point;

                    GameObject spawnedPrefab = Instantiate(prefabToInstantiate, new Vector3(groundPosition.x, groundPosition.y, groundPosition.z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                    Vector3 originalScale = prefabToInstantiate.transform.localScale;
                    float randomScaleFactor = Random.Range(0.7f, 1f);
                    spawnedPrefab.transform.localScale = originalScale* randomScaleFactor;
                    NavTerrain navTerrain = spawnedPrefab.GetComponent<NavTerrain>();
                    if (navTerrain != null)
                    {
                        navTerrain.randomScaleFactor = randomScaleFactor;
                    }

                    occupiedPositions.Add(new Vector2(groundPosition.x, groundPosition.z));
                    spawnedPrefabs.Add(spawnedPrefab);
                }
            }
            yield return null;
        }
    }
    private Dictionary<int, Vector2> CalculateRegionCenters(int[,] mergedMap, int width, int height)
    {
        Dictionary<int, Vector2> regionCenters = new Dictionary<int, Vector2>();
        Dictionary<int, int> regionCounts = new Dictionary<int, int>();

        for (int x = margin; x < width - margin; x++)
        {
            for (int y = margin; y < height - margin; y++)
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

            if (center.x > margin && center.x < width - margin &&
                center.y > margin && center.y < height - margin)
            {
                updatedCenters[regionIndex] = center;
            }
        }

        return updatedCenters;
    }

    private GameObject GetPrefabForColor(Color regionColor)
    {
        GameObject prefabToInstantiate = null;

 
        if (regionColor == Color.red && villagePrefab.Length > 0)
        {
            prefabToInstantiate = villagePrefab[Random.Range(0, villagePrefab.Length)];
        }
        else if (regionColor == Color.yellow && landPrefab.Length > 0)
        {
            prefabToInstantiate = landPrefab[Random.Range(0, landPrefab.Length)];
        }
        else if (regionColor == Color.gray && mountainPrefab.Length > 0)
        {
            prefabToInstantiate = mountainPrefab[Random.Range(0, mountainPrefab.Length)];
        }

        return prefabToInstantiate;
    }

    private Vector2 GetValidSpawnPosition(int[,] mergedMap, int regionIndex, int width, int height, GameObject prefab)
    {
        List<Vector2> availablePositions = new List<Vector2>();

        for (int x = margin; x < width - margin; x++)
        {
            for (int y = margin; y < height - margin; y++)
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


        float worldX = Mathf.Clamp(quadPosition.x + (x / (float)width - 0.5f) * quadScale.x, quadPosition.x - quadScale.x / 2f, quadPosition.x + quadScale.x / 2f);
        float worldZ = Mathf.Clamp(quadPosition.z + (y / (float)height - 0.5f) * quadScale.z, quadPosition.z - quadScale.z / 2f, quadPosition.z + quadScale.z / 2f);

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