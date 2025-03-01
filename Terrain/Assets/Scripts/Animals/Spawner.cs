using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private List<AnimalSettings> Animals = new List<AnimalSettings>();
    public float SpawnRadius=15f;
    void Start()
    {
        SpawnAnimals();
    }
    protected virtual void SpawnAnimals()
    {
        foreach (var AnimalSettings in Animals)
        {
            int SpawnCount = Random.Range(AnimalSettings.MinSpawnCount, AnimalSettings.MaxSpawnCount);
            for (int i = 0; i <SpawnCount; i++)
            {
                Vector3 SpawnPoint = GetRandomSpawnPosition();
                Instantiate(AnimalSettings.AnimalPrefab, SpawnPoint, Quaternion.identity);
            }
        } 
    }
    protected virtual Vector3 GetRandomSpawnPosition()
    {
        Vector2 RandomCircle = Random.insideUnitCircle;
        return new Vector3(transform.position.x+RandomCircle.x*SpawnRadius,transform.position.y,transform.position.z+RandomCircle.y*SpawnRadius);
    }
}

[System.Serializable]//used in class/struct
public class AnimalSettings
{
    public GameObject AnimalPrefab;
    public int MinSpawnCount = 3;
    public int MaxSpawnCount = 8;
    //public int SpawnCount = 5;
}
