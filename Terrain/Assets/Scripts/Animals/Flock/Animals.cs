using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animals : MonoBehaviour
{
    public IEnumerator AnimalSpawner()
    {
        yield return StartCoroutine(SpawnAllFlocks());
    }
    private IEnumerator SpawnAllFlocks()
    {
        Flock[] allFlocks = FindObjectsOfType<Flock>();

        foreach (Flock flock in allFlocks)
        {
            if (flock != null)
            {
                yield return StartCoroutine(flock.AnimalSpawner());
            }
        }
    }
    public IEnumerator Animalclear()
    {
        Flock[] allFlocks = FindObjectsOfType<Flock>();

        foreach (Flock flock in allFlocks)
        {
            if (flock != null)
            {
                yield return StartCoroutine(flock.RefreshAnimals());
            }
        }
    }
}
