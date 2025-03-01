using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawner : Spawner
{
    protected override Vector3 GetRandomSpawnPosition()
    {
        SpawnRadius = 5f;
        return base.GetRandomSpawnPosition();
    }
}
