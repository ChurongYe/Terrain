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

        yield return GenerateVoronoi();
        yield return StartCoroutine(GenerateTrees());

        yield return StartCoroutine(GenerateTilemap());
        navMeshSurface.BuildNavMesh();

        yield return new WaitUntil(() => navMeshSurface.navMeshData != null); 

        yield return StartCoroutine(GenerateNPCs());

        Time.timeScale = 1f; 
    }

    private IEnumerator GenerateTilemap()
    {
        Tilemap.TileGenerate();
        yield return null;  
    }

    private IEnumerator GenerateTrees()
    {
        TreeGenerate.GenerateTrees();
        yield return null;  
    }

    private IEnumerator GenerateNPCs()
    {
        NPCNavigation.SpawnNPCs(npccount);
        yield return null;  
    }
    private IEnumerator GenerateVoronoi()
    {
        Voronoi.GenerateTexture();
        yield return null;
    }
}
