//using System.Collections.Generic;
//using UnityEngine;


//public class Harvestposition : MonoBehaviour
//{
//    public static Harvestposition Instance { get; private set; }
//    public List<GameObject> Harvestcrops = new List<GameObject>();// 存储所有成熟的作物位置
//    private float updateInterval = 5f;
//    public bool neednpc = false;
//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }
//    private void Start()
//    {
//        //InvokeRepeating(nameof(UpdateHarvestCrops), 0f, updateInterval);
//    }
//    public void AddHarvestCrops(GameObject crop)
//    {
//        //Harvestcrops.Clear();
//        //GameObject[] crops = GameObject.FindGameObjectsWithTag("Crops");
//        if (!Harvestcrops.Contains(crop)) // 避免重复添加
//        {
//            Harvestcrops.Add(crop);
//        }
//        if (Harvestcrops.Count != 0)
//        {
//            neednpc = true;
//        }
//        else
//        {
//            neednpc = false;
//        }
//    }
//    public void RemoveCrop(GameObject crop)
//    {
//        if (Harvestcrops.Contains(crop))
//        {
//            Crop plant = crop.GetComponent<Crop>();
//            if (plant != null)
//            {
//                plant.Harvested = true; // 设置为未成熟
//            }
//            Harvestcrops.Remove(crop);
//        }
//    }
//    public (Vector2?, GameObject) GetNearestCrop(Vector2 npcPosition)
//    {
//        if (Harvestcrops.Count == 0)
//            return (null, null);

//        GameObject closestCrop = null;
//        Vector2 closestPosition = Vector2.zero;
//        float minDistance = float.MaxValue;

//        foreach (GameObject crop in Harvestcrops)
//        {
//            Crop plant = crop.GetComponent<Crop>();
//            if (plant != null)
//            {
//                Vector2 cropPosition = new Vector2(plant.HarvestPoint.position.x, plant.HarvestPoint.position.z);
//                float distance = Vector3.Distance(npcPosition, cropPosition);
//                if (distance < minDistance)
//                {
//                    closestCrop = crop;
//                    closestPosition = cropPosition;
//                    minDistance = distance;
//                }
//            }
//        }
//        return closestCrop != null ? ((Vector2?)closestPosition, closestCrop) : (null, null);
//    }
//}