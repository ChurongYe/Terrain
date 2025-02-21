using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Voronoi Voronoi;
    public NavMeshSurface navMeshSurface;

    void Update()
    {
        Clickmouse();
    }

    private void Clickmouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Voronoi.seed = Random.Range(0, int.MaxValue);
            Voronoi.GenerateTexture();

            StartCoroutine(BakeNavMeshAndPause());
        }
    }


    private IEnumerator BakeNavMeshAndPause()
    {

        Time.timeScale = 0f;

        yield return null;

        navMeshSurface.BuildNavMesh();

        Time.timeScale = 1f;
    }
}
