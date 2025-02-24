using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private List<AnimalSettings> Animals = new List<AnimalSettings>();
    private float SpawnRadius=15f;
    private void Start()
    {
        SpawnAnimals();
    }
    private void SpawnAnimals()
    {
        foreach (var AnimalSettings in Animals)
        {
            for (int i = 0; i < AnimalSettings.SpawnCount; i++)
            {
                Vector3 SpawnPoint = GetRandomSpawnPosition();
                Instantiate(AnimalSettings.AnimalPrefab, SpawnPoint, Quaternion.identity);
            }
        } 
    }
    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 RandomCircle = Random.insideUnitCircle;
        return new Vector3(transform.position.x+RandomCircle.x*SpawnRadius,0,transform.position.z+RandomCircle.y*SpawnRadius);
    }
}

[System.Serializable]//used in class/struct
public class AnimalSettings
{
    public GameObject AnimalPrefab;
    public int SpawnCount = 5;
}
