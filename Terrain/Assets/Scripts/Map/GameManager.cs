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
    public PlantSpawner plantSpawner;
    public Seasonchange seasonchange;
    public Animals flock;
    public ThirdPersonController thirdPersonController;
    void Start()
    {
        Clickmouse();
        //if (Input.GetMouseButtonDown(1))
        //{
        //   StartCoroutine(TreeGenerate.GenerateTrees());
        //}
    }

    public void Clickmouse()
    {
        Voronoi.seed = Random.Range(0, int.MaxValue);
        StartCoroutine(BakeNavMeshAndPause());

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Voronoi.seed = Random.Range(0, int.MaxValue);

        //    StartCoroutine(BakeNavMeshAndPause());
        //}
    }


    private IEnumerator BakeNavMeshAndPause()
    {
        Time.timeScale = 0f;
        yield return StartCoroutine(plantSpawner.Clear());
        yield return StartCoroutine(flock.Animalclear());
        yield return StartCoroutine(thirdPersonController.ClearRobots());
        yield return StartCoroutine(Voronoi.GenerateTexture());
        yield return StartCoroutine(Tilemap.TileGenerate());
        navMeshSurface.BuildNavMesh();
        Time.timeScale = 1f;
        yield return new WaitUntil(() => navMeshSurface.navMeshData != null);
        yield return StartCoroutine(TreeGenerate.GenerateTrees());
        yield return StartCoroutine(NPCNavigation.SpawnNPCs(npccount));
        yield return StartCoroutine(plantSpawner.PlantGenerate());
        yield return StartCoroutine(flock.AnimalSpawner());
        yield return StartCoroutine(seasonchange.StartColor());

    }
}
