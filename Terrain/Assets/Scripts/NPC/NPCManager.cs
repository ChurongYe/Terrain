using System.Collections;
using System.Collections.Generic;
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
        StartCoroutine(CheckForHarvesting());
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
        }
    }

    IEnumerator CheckForHarvesting()
    {
        while (true)
        {
            yield return new WaitForSeconds(harvestCheckInterval);

            if (npcState == NPCState.Normal && Random.value < harvestProbability)
            {
                Vector3? targetCrop = npcNavigation.FindNearestHarvestableCrop(transform.position);
                if (targetCrop.HasValue)
                {
                    npcState = NPCState.Harvesting;
                    npcNavigation.MoveToTarget(targetCrop.Value);
                }
            }
        }
    }

    public void SwitchToNextState()
    {
        if (npcState == NPCState.Normal)
        {
            npcState = NPCState.Resting;
        }
        else if (npcState == NPCState.Resting)
        {
            npcState = NPCState.Normal;
        }
        Debug.Log("NPC ：" + npcState);
    }
}