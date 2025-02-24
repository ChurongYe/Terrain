using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Voronoi Voronoi;
    public NavMeshSurface navMeshSurface;
    public TreeGenerate TreeGenerate;
    public Tilemap Tilemap;
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
        Tilemap.TileGenerate();
        yield return null;

        navMeshSurface.BuildNavMesh();

        Time.timeScale = 1f;
        yield return null;
        TreeGenerate.GenerateTrees();
    }
}
