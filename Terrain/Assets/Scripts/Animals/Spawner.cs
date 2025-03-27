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
    public int CreatureMask;
    public int LandMask;
    public float RayDistance = 10f;
    protected List<Transform> Points = new List<Transform>();
    public List<GameObject> SpawnedCrops = new List<GameObject>(); // 存储当前 Spawner 生成的所有农作物
    void Start()
    {
        SpawnPoint();
        SpawnAnimals();

    }
    private void SpawnPoint()
    {
        SpawnedCrops.Clear();
        for (int i = 0; i < SpawnPointAmount; i++)
        {
            Vector3 RandomPoint = transform.position +
                new Vector3
                (
                    Random.Range(-SpawnRadius, SpawnRadius),
                0,
                Random.Range(-SpawnRadius, SpawnRadius)
                );
            GameObject PointObj = new GameObject("SpawnPoint_" + i);
            PointObj.transform.position = RandomPoint;
            //GameObject PointObj=Instantiate(new GameObject(), RandomPoint, Quaternion.identity);
            Points.Add(PointObj.transform);         
        }
    }
    protected virtual void SpawnAnimals()
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
                if (!Physics.CheckBox(point.position, new Vector3(SpawnBoxX, Mathf.Min(SpawnBoxX, SpawnBoxZ), SpawnBoxZ)*0.5f, Quaternion.identity, 10))
                {
                    SpawnPointInBox.y = CheckLandHeight(SpawnPointInBox);
                    Quaternion Rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    //Instantiate(AnimalType.AnimalPrefab, SpawnPointInBox, Rotation, point);
                    GameObject crop = Instantiate(AnimalType.AnimalPrefab, SpawnPointInBox, Rotation, point);
                    SpawnedCrops.Add(crop);
                }   
            }
        }
        StartCoroutine(CheckForMatureCrops());
    }
    private IEnumerator CheckForMatureCrops()
    {
        while (true)
        {
            if (SpawnedCrops.Count > 0)
            {
                bool allMature = true;
                foreach (GameObject crop in SpawnedCrops)
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
                    Debug.Log(" need to harvesting");
                    AssignNPCToHarvest(); // 让 NPC 收获
                }
            }
            yield return new WaitForSeconds(2f); // 每 2 秒检查一次
        }
    }
    private void AssignNPCToHarvest()
    {
        NPCManager nearestNPC = FindNearestNormalNPC();
        if (nearestNPC != null)
        {
            nearestNPC.StartHarvesting(SpawnedCrops);
        }
        else
        {
            StartCoroutine(WaitForAvailableNPC()); // 等待有 NPC 变回 Normal 状态
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
    private IEnumerator WaitForAvailableNPC()
    {
        while (FindNearestNormalNPC() == null)
        {
            yield return new WaitForSeconds(3f);
        }
        AssignNPCToHarvest(); // 重新尝试分配 NPC
    }
    private float CheckLandHeight(Vector3 SpawnPoint)
    {
        RaycastHit HitInfo;
        bool Hit = Physics.Raycast(SpawnPoint, Vector3.down, out HitInfo,20f,LandMask);
        if (!Hit)
            return SpawnPoint.y;
        else
            return HitInfo.point.y;
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
