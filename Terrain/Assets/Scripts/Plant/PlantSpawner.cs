using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawner : Spawner
{
    //protected override Vector3 GetRandomSpawnPosition()
    //{
    //    SpawnRadius = 5f;
    //    return base.GetRandomSpawnPosition();
    //}
    //protected override Vector3 GetRandomSpawnPosition(Transform SpawnPoint)
    //{
    //    Vector2 RandomCircle = Random.insideUnitCircle;
    //    return new Vector3(SpawnPoint.position.x + RandomCircle.x * SpawnRadius, SpawnPoint.position.y, SpawnPoint.position.z + RandomCircle.y * SpawnRadius);
    //}
    //protected override void SpawnAnimals()
    //{
    //    foreach (var point in Points)
    //    {
    //        if (Animals.Count == 0)
    //            continue;//?
    //        AnimalSettings AnimalType = Animals[Random.Range(0, Animals.Count)];
    //        int SpawnCount = Random.Range(AnimalType.MinSpawnCount, AnimalType.MaxSpawnCount);
    //        for (int i = 0; i < SpawnCount; i++)
    //        {
    //            Vector3 SpawnPointInBox = point.position + new Vector3(Random.Range(-SpawnBoxX, SpawnBoxX), 0, Random.Range(-SpawnBoxZ, SpawnBoxZ));
    //            if (!Physics.CheckBox(point.position, new Vector3(SpawnBoxX, Mathf.Min(SpawnBoxX, SpawnBoxZ), SpawnBoxZ) * 0.5f, Quaternion.identity, 10))
    //            {
    //                Quaternion Rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
    //                Instantiate(AnimalType.AnimalPrefab, SpawnPointInBox, Rotation, point);
    //            }
    //        }
    //    }
    //}
}
