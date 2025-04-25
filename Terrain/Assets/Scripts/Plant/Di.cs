using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Di : MonoBehaviour
{
    public Spawner spawner;
    void Start()
    {
        GameObject spawnerObj = GameObject.FindWithTag("PlantSpawner");
        spawner = spawnerObj.GetComponent<Spawner>();

        if (spawner != null)
        {
            spawner.AddDi(transform.position);
        }
        else
        {
            Debug.LogWarning("Spawner not found in the scene.");
        }
    }

}
