using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public enum NPCState { Normal, Resting, Harvesting }

    public NPCState npcState = NPCState.Normal;
    public float harvestCheckInterval = 5f; // 检查是否进入 Harvesting 状态的间隔
    public float harvestProbability = 0.8f; // 80% 概率
    public NPCNavigation npcNavigation;
    private void Start()
    {
        npcNavigation = FindObjectOfType<NPCNavigation>();
        StartCoroutine(StateLoop());
        //StartCoroutine(CheckForHarvesting());
    }

    IEnumerator StateLoop()
    {
        while (true)
        {
            if (npcState == NPCState.Normal)
                yield return new WaitForSeconds(Random.Range(15f, 50f));
            else if (npcState == NPCState.Resting)
                yield return new WaitForSeconds(Random.Range(5f, 10f));

            SwitchToNextState();
            yield return null;
        }
    }

    //IEnumerator CheckForHarvesting()
    //{
    //    while (true)
    //    {
    //        if (Harvestposition.Instance != null)
    //        {
    //            if (npcState == NPCState.Normal)
    //            {
    //                yield return new WaitForSeconds(harvestCheckInterval);
    //                Debug.Log("Checking for harvesting...");
    //                if (Harvestposition.Instance.neednpc)//&& Random.value < harvestProbability
    //                {
    //                    Vector2 self = new Vector2(transform.position.x, transform.position.z);
    //                    var (targetPosition, targetCrop) = Harvestposition.Instance.GetNearestCrop(self);
    //                    if (targetPosition.HasValue && targetCrop != null)
    //                    {
    //                        npcNavigation.MoveToTarget(targetPosition.Value);
    //                        Debug.Log($"Moving to crop at {targetPosition.Value}");
    //                        CropInfor(targetPosition.Value, targetCrop);
    //                        npcState = NPCState.Harvesting;
    //                    }
    //                }
    //            }
    //            if(npcState == NPCState.Harvesting && npcNavigation.Movetocrop == true)
    //            {
    //                Vector2 targetPosition = cropInfor.Item1 ;
    //                GameObject targetCrop = cropInfor.Item2;
    //                 Debug.Log($"Arrived at {targetPosition}, harvesting crop.");
    //                Harvestposition.Instance.RemoveCrop(targetCrop);
    //            }
    //            if (npcState == NPCState.Harvesting && !Harvestposition.Instance.neednpc)
    //            {
    //                Debug.Log("No crops left, returning to normal state.");
    //                npcState = NPCState.Normal;
    //            }
    //        }
    //        yield return null;
    //    }
    //}
    public void StartHarvesting(List<GameObject> crops)
    {
        if (npcState != NPCState.Normal || crops.Count == 0)
            return;

        npcState = NPCState.Harvesting;
        StartCoroutine(HarvestCrops(crops));
    }
    private IEnumerator HarvestCrops(List<GameObject> crops)
    {
        foreach (GameObject crop in crops)
        {
            if (crop == null) continue;
            Crop thiscrop = crop.GetComponent<Crop>();
            Vector3 targetPos = thiscrop.HarvestPoint.position ;
            //Vector3 targetPos = crop.transform.position;
            npcNavigation.MoveToTarget(targetPos, this.gameObject);

            while (!this.gameObject.GetComponent<NPCController>().Movetocrop) // 等待 NPC 到达目标位置
            {
                Debug.Log($"NPC {name} harvesting crop at {targetPos}");
                thiscrop.Harvested = true;
                yield return null;
            }

            yield return new WaitForSeconds(1f); // 等待一小段时间，模拟收获
        }
        npcState = NPCState.Normal; // 收获完成，恢复 Normal 状态
    }
    //private void CropInfor(Vector2 pos, GameObject crop)
    //{
    //    cropInfor = (pos,crop);
    //}
    public void SwitchToNextState()
    {
        if (npcState == NPCState.Normal)
        {
            npcState = NPCState.Resting;
            //Debug.Log("NPC ：" + npcState);

        }
        else if (npcState == NPCState.Resting)
        {
            npcState = NPCState.Normal;
            //Debug.Log("NPC ：" + npcState);
        }
        //Debug.Log("NPC ：" + npcState);
    }
}