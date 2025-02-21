using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NavTerrain : MonoBehaviour
{
    public float randomScaleFactor;
    private NavMeshLink[] links;

    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return null;
        InitializeNavMeshLinks();
    }

    void InitializeNavMeshLinks()
    {
        links = GetComponents<NavMeshLink>();

        foreach (var link in links)
        {
            AdjustNavMeshLink(link);
        }
    }

    void AdjustNavMeshLink(NavMeshLink link)
    {
        Vector3 scale = Vector3.one * randomScaleFactor; 
        link.startPoint = Vector3.Scale(link.startPoint, scale);
        link.endPoint = Vector3.Scale(link.endPoint, scale);

        link.UpdateLink();
    }

}

