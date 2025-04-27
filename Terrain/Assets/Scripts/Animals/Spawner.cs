using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using static NPCManager;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    protected List<AnimalSettings> Animals = new List<AnimalSettings>();
    //[SerializeField]
    //private List<Transform> SpawnPoints = new List<Transform>();
    //[SerializeField]
    //private List<BoxCollider> SpawnAreas;
    public float SpawnRadius=15f;
    public int SpawnPointAmount=5;//
    public float SpawnBoxX = 5f;
    public float SpawnBoxZ = 4f;
    public LayerMask CreatureMask;
    //public int LandMask;
    public LayerMask LandMask;
    public float RayDistance = 10f;
    public float Upset = 1f;
    public float MinSpawnDistance;
    protected List<Transform> Points = new List<Transform>();
    private Dictionary<Transform, List<GameObject>> SpawnedCrops = new(); // Crops of Point
    private Dictionary<Transform, NPCManager> AssignedHarvesters = new(); // NPC of Point
    private List<Vector3> diPositions = new List<Vector3>();

    void Start()
    {
        StartCoroutine(CheckForMatureCrops());
    }
    public IEnumerator PlantGenerate()
    {
        yield return StartCoroutine(ClearAllSpawnedObjects());
    }
    public void AddDi( Vector3 diposition)
    {
        diPositions.Add(diposition);
    }
    public IEnumerator Clear()
    {
        diPositions.Clear();
        yield return null;
    }

    public IEnumerator ClearAllSpawnedObjects()
    {
        foreach (var kvp in SpawnedCrops)
        {
            foreach (var crop in kvp.Value)
            {
                if (crop != null)
                    Destroy(crop);
            }
        }

        foreach (var point in Points)
        {
            if (point != null)
                Destroy(point.gameObject);
        }

        SpawnedCrops.Clear();
        AssignedHarvesters.Clear();
        Points.Clear();
        yield return SpawnPoint();
        yield return SpawnAnimals();
    }
    private IEnumerator SpawnPoint()
    {
        SpawnPointAmount = diPositions.Count;
        for (int i = 0; i < SpawnPointAmount; i++)
        {
            Vector3 pointPos = diPositions[i];
            GameObject PointObj = new GameObject("SpawnPoint_" + i);
            PointObj.transform.position = pointPos;
            //GameObject PointObj=Instantiate(new GameObject(), RandomPoint, Quaternion.identity);
            Transform pointTransform = PointObj.transform;
            Points.Add(pointTransform);
            SpawnedCrops[pointTransform] = new List<GameObject>(); 
            AssignedHarvesters[pointTransform] = null; //No NPC     
        }
        yield return null;
    }
    protected virtual IEnumerator SpawnAnimals()//actually plant
    {
        foreach (var point in Points)
        {
            if (Animals.Count == 0)
                continue;//?
            AnimalSettings AnimalType = Animals[Random.Range(0, Animals.Count)];
            int SpawnCount = Random.Range(AnimalType.MinSpawnCount, AnimalType.MaxSpawnCount);
            for (int i = 0; i < SpawnCount; i++)
            {
                Vector3 SpawnPointInBox = point.position + new Vector3(Random.Range(-SpawnBoxX, SpawnBoxX), 0, Random.Range(-SpawnBoxZ, SpawnBoxZ));
                SpawnPointInBox.y = CheckLandHeight(SpawnPointInBox) + Upset;
                bool CanSpawnHere = true;
                foreach (var existingAnimal in SpawnedCrops[point])
                {
                    if (Vector3.Distance(existingAnimal.transform.position, SpawnPointInBox) < MinSpawnDistance)
                    {
                        CanSpawnHere = false;
                        break;
                    }
                }    
                //if (!Physics.CheckBox(point.position, new Vector3(SpawnBoxX, Mathf.Min(SpawnBoxX, SpawnBoxZ), SpawnBoxZ)*0.5f, Quaternion.identity, CreatureMask))//avoid overlap in spawner box
                //{
                //    SpawnPointInBox.y = CheckLandHeight(SpawnPointInBox)+ Upset;
                //    Quaternion Rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                //    //Instantiate(AnimalType.AnimalPrefab, SpawnPointInBox, Rotation, point);
                //    GameObject crop = Instantiate(AnimalType.AnimalPrefab, SpawnPointInBox, Rotation, point);
                //    SpawnedCrops[point].Add(crop); 
                
                if (CanSpawnHere)
                {
                    Quaternion Rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    GameObject Crop = Instantiate(AnimalType.AnimalPrefab, SpawnPointInBox, Rotation, point);
                    SpawnedCrops[point].Add(Crop);
                }
            }
        }
        yield return null;
    }
    private IEnumerator CheckForMatureCrops()
    {
        while (true)
        {
            foreach (var kvp in SpawnedCrops)
            {
                Transform point = kvp.Key;
                List<GameObject> crops = kvp.Value;

                if (crops.Count ==0)
                {
                    Debug.Log("newwwwwwwwww");
                    AssignedHarvesters[point] = null;
                    StartCoroutine(RespawnCrops(point));
                }
                // add new plant
                if (crops.Count > 0 && AssignedHarvesters[point] == null) 
                {
                    bool allMature = true;

                    foreach (GameObject crop in crops)
                    {
                        Crop thiscrop = crop.GetComponent<Crop>();
                        if (thiscrop != null && !thiscrop.IsMature)
                        {
                            allMature = false;
                            break;
                        }
                    }

                    if (allMature)
                    {
                        //Debug.Log($"[Spawner] {point.name} 下所有作物成熟，准备收获！");

                        NPCManager npc = FindNearestNormalNPC();
                        if (npc != null)
                        {
                            AssignedHarvesters[point] = npc; // Record NPC
                            npc.StartHarvesting(crops);
                        }
                        else
                        {
                            StartCoroutine(WaitForAvailableNPC(point)); // wait NPC
                        }
                    }
                }
            }

            yield return new WaitForSeconds(2f);
        }
    }
    
    private IEnumerator RespawnCrops(Transform point)
    {
        if (point != null)
        {
            yield return new WaitForSeconds(Random.Range(20f, 35f));
            AnimalSettings AnimalType = Animals[Random.Range(0, Animals.Count)];
            int SpawnCount = Random.Range(AnimalType.MinSpawnCount, AnimalType.MaxSpawnCount);

            for (int i = 0; i < SpawnCount; i++)
            {
                Vector3 SpawnPointInBox = point.position + new Vector3(Random.Range(-SpawnBoxX, SpawnBoxX), 0, Random.Range(-SpawnBoxZ, SpawnBoxZ));
                SpawnPointInBox.y = CheckLandHeight(SpawnPointInBox) + Upset;
                bool CanSpawnHere = true;
                foreach (var existingAnimal in SpawnedCrops[point])
                {
                    if (existingAnimal == null) continue;
                    if (Vector3.Distance(existingAnimal.transform.position, SpawnPointInBox) < MinSpawnDistance)
                    {
                        CanSpawnHere = false;
                        break;
                    }
                }

                if (CanSpawnHere)
                {
                    Quaternion Rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    GameObject Crop = Instantiate(AnimalType.AnimalPrefab, SpawnPointInBox, Rotation, point);
                    SpawnedCrops[point].Add(Crop);
                }
            }
        }
        yield return null;
    }
    private IEnumerator WaitForAvailableNPC(Transform point)
    {
        while (FindNearestNormalNPC() == null)
        {
            yield return new WaitForSeconds(3f);
        }
        if (point != null && AssignedHarvesters.ContainsKey(point))
        {
            AssignNPCToHarvest(point);
        }
    }
    private void AssignNPCToHarvest(Transform point)
    {
        if (!AssignedHarvesters.ContainsKey(point))
        {
            return;
        }

        if (AssignedHarvesters[point] == null)
        {
            NPCManager npc = FindNearestNormalNPC();
            if (npc != null)
            {
                AssignedHarvesters[point] = npc;
                npc.StartHarvesting(SpawnedCrops[point]);
                Debug.Log($"[Spawner] 分配 NPC {npc.name} 到 {point.name} 进行收割");
            }
        }
    }
    private NPCManager FindNearestNormalNPC()
    {
        NPCManager nearestNPC = null;
        float nearestDistance = float.MaxValue;
        Vector3 spawnerPosition = transform.position;

        foreach (NPCManager npc in FindObjectsOfType<NPCManager>())
        {
            if (npc.npcState == NPCState.Normal)
            {
                float distance = Vector3.Distance(npc.transform.position, spawnerPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestNPC = npc;
                }
            }
        }
        return nearestNPC;
    }
    private float CheckLandHeight(Vector3 SpawnPoint)//check spawner point of single objects(on the specfic layer)
    {
        RaycastHit[] hits = Physics.RaycastAll(SpawnPoint, Vector3.down, 1000f, LandMask);

        if (hits.Length == 0)
            return SpawnPoint.y;

        float topY = SpawnPoint.y;
        foreach (var hit in hits)
        {
            if (hit.point.y > topY)
            {
                topY = hit.point.y;
            }
        }
        return topY;
    }
    //    foreach (var AnimalSettings in Animals)
    //    {
    //        int SpawnCount = Random.Range(AnimalSettings.MinSpawnCount, AnimalSettings.MaxSpawnCount);
    //        //Transform SpwanPoint = SpawnPoints[AnimalSettings.PointNumber];
    //        BoxCollider SpawnArea = SpawnAreas[AnimalSettings.PointNumber];
    //        for (int i = 0; i <SpawnCount; i++)
    //        {
            
     
    //            if (!Physics.CheckBox(SpawnArea.bounds.center, SpawnArea.bounds.size * 0.5f, Quaternion.identity, CreatureMask)) //QueryTriggerInteraction.Collide is for trigger
    //            {
    //                Quaternion Rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
    //                //Instantiate(AnimalSettings.AnimalPrefab, SpawnPoint, Rotation, transform);
    //            }
            
    //            //Vector3 SpawnPoint = GetRandomSpawnPosition(SpwanPoint);
    //            //Instantiate(AnimalSettings.AnimalPrefab, SpawnPoint, Quaternion.identity);
    //        }
    //    } 
    //}
    //protected virtual Vector3 GetRandomSpawnPosition(Transform SpawnPoint)
    //{
    //    Vector2 RandomCircle = Random.insideUnitCircle;
    //    return new Vector3(SpawnPoint.position.x+RandomCircle.x*SpawnRadius,SpawnPoint.position.y,SpawnPoint.position.z+RandomCircle.y*SpawnRadius);
    //}

    //protected virtual Vector3 GetRandomPointInBox(BoxCollider Box)//
    //{
    //    Vector3 Center = Box.bounds.center;//bounds is a world coordinate
    //    Vector3 Size = Box.bounds.size;
    //    float x = Random.Range(Center.x - Size.x * 0.5f, Center.x + Size.x * 0.5f);
    //    float z = Random.Range(Center.z - Size.z * 0.5f, Center.z + Size.z * 0.5f);
    //    return new Vector3(x, Center.y, z);

    //}
}

[System.Serializable]//used in class/struct
public class AnimalSettings
{
    public GameObject AnimalPrefab;
    public int MinSpawnCount = 3;
    public int MaxSpawnCount = 8;
    //public int PointNumber;
    //public int SpawnCount = 5;
}
