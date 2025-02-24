using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerate : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public GameObject[] grassflowers;
    public int treeCount =20;
    public int grassflowerCount = 200;
    public Vector2 areaSize = new Vector2(10, 10); 
    public LayerMask treegroundLayer;
    public LayerMask grassgroundLayer;
    public LayerMask obstacleLayer; 
    public float minScale = 0.8f;
    public float maxScale = 1.5f;
    public int maxAttempts = 10; 
    public float overlapCheckRadius = 0.5f; 
    private List<GameObject> trees = new List<GameObject>();
    private List<GameObject> Grassflower = new List<GameObject>();
    public void GenerateTrees()
    {
        foreach (var tree in trees)
        {
            Destroy(tree);
        }
        foreach (var grassflower in Grassflower)
        {
            Destroy(grassflower);
        }
        trees.Clear();
        Grassflower.Clear();
        for (int i = 0; i < treeCount; i++)
        {
            bool IfPlaced = false;
            int attempts = 0;

            while (!IfPlaced && attempts < maxAttempts)
            {
                attempts++;
                Vector3 spawnPos = new Vector3(
                    RandomGaussian.Range(-areaSize.x / 2, areaSize.x / 2),
                    10, 
                    RandomGaussian.Range(-areaSize.y / 2, areaSize.y / 2)
                );


                if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 20f, treegroundLayer))
                {
                    Vector3 finalPos = hit.point;
                    if (!Physics.CheckSphere(finalPos, overlapCheckRadius, obstacleLayer))
                    {
                        GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                        GameObject tree = Instantiate(treePrefab, finalPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
                        float scale = Random.Range(minScale, maxScale);
                        tree.transform.localScale = Vector3.one * scale;
                        trees.Add(tree);
                        IfPlaced = true;
                    }
                }
            }
        }
        for (int i = 0; i < grassflowerCount; i++)
        {
            bool IfPlaced = false;
            int attempts = 0;

            while (!IfPlaced && attempts < maxAttempts)
            {
                attempts++;
                Vector3 spawnPos = new Vector3(
                    RandomGaussian.Range(-areaSize.x / 2, areaSize.x / 2),
                    10,
                    RandomGaussian.Range(-areaSize.y / 2, areaSize.y / 2)
                );


                if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 20f, grassgroundLayer))
                {
                    Vector3 finalPos = hit.point;
                    if (!Physics.CheckSphere(finalPos, overlapCheckRadius, obstacleLayer))
                    {
                        GameObject grassflowerPredab = grassflowers[Random.Range(0, grassflowers.Length)];
                        GameObject grassflower = Instantiate(grassflowerPredab, finalPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
                        float scale = Random.Range(minScale, maxScale);
                        grassflower.transform.localScale = Vector3.one * scale;
                        Grassflower.Add(grassflower);
                        IfPlaced = true;
                    }
                }
            }
        }
    }

}
