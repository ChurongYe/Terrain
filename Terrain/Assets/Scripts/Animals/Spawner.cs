using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private List<AnimalSettings> Animals = new List<AnimalSettings>();
    //[SerializeField]
    //private List<Transform> SpawnPoints = new List<Transform>();
    [SerializeField]
    private List<BoxCollider> SpawnAreas;
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
            //Transform SpwanPoint = SpawnPoints[AnimalSettings.PointNumber];
            BoxCollider SpawnArea = SpawnAreas[AnimalSettings.PointNumber];
            for (int i = 0; i <SpawnCount; i++)
            {
                Vector3 SpawnPoint = GetRandomPointInBox(SpawnArea);
                Instantiate(AnimalSettings.AnimalPrefab, SpawnPoint, Quaternion.identity);
                //Vector3 SpawnPoint = GetRandomSpawnPosition(SpwanPoint);
                //Instantiate(AnimalSettings.AnimalPrefab, SpawnPoint, Quaternion.identity);
            }
        } 
    }
    //protected virtual Vector3 GetRandomSpawnPosition(Transform SpawnPoint)
    //{
    //    Vector2 RandomCircle = Random.insideUnitCircle;
    //    return new Vector3(SpawnPoint.position.x+RandomCircle.x*SpawnRadius,SpawnPoint.position.y,SpawnPoint.position.z+RandomCircle.y*SpawnRadius);
    //}
    protected virtual Vector3 GetRandomPointInBox(BoxCollider Box)//
    {
        Vector3 Center = Box.bounds.center;
        Vector3 Size = Box.bounds.size;
        float x = Random.Range(Center.x - Size.x * 0.5f, Center.x + Size.x * 0.5f);
        float z = Random.Range(Center.z - Size.z * 0.5f, Center.z + Size.z * 0.5f);
        return new Vector3(x, Center.y, z);

    }
}

[System.Serializable]//used in class/struct
public class AnimalSettings
{
    public GameObject AnimalPrefab;
    public int MinSpawnCount = 3;
    public int MaxSpawnCount = 8;
    public int PointNumber;
    //public int SpawnCount = 5;
}
