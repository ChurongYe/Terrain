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
    public NPCNavigation NPCNavigation;
    public int npccount;
    void Update()
    {
        Clickmouse();
        if (Input.GetMouseButtonDown(1))
        {
           StartCoroutine(TreeGenerate.GenerateTrees());
        }
    }

    private void Clickmouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Voronoi.seed = Random.Range(0, int.MaxValue);

            StartCoroutine(BakeNavMeshAndPause());
        }
    }


    private IEnumerator BakeNavMeshAndPause()
    {
        Time.timeScale = 0f;

        yield return StartCoroutine(Voronoi.GenerateTexture());
        yield return StartCoroutine(Tilemap.TileGenerate());
        navMeshSurface.BuildNavMesh();
        Time.timeScale = 1f;
        yield return new WaitUntil(() => navMeshSurface.navMeshData != null);
        yield return StartCoroutine(TreeGenerate.GenerateTrees());
        yield return StartCoroutine(NPCNavigation.SpawnNPCs(npccount));

    }
}
