using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavigation : MonoBehaviour
{
    public Transform village; // 村庄的位置
    public Transform farmland; // 农田的位置
    public float movementSpeed = 3.5f; // NPC的移动速度
    public float stoppingDistance = 0.5f; // 到达目的地的距离阈值
    private NavMeshAgent agent; // 用于控制NPC的NavMeshAgent

    void Start()
    {
        // 获取 NavMeshAgent 组件
        agent = GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;
        agent.stoppingDistance = stoppingDistance;

        // 开始从村庄到农田的路径
        MoveToDestination(village.position); // 初始从村庄开始
    }

    void Update()
    {
        // 如果NPC到达目的地，改变目的地为农田
        if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
        {
            if (agent.destination == village.position)
            {
                // NPC到达村庄后，开始向农田移动
                MoveToDestination(farmland.position);
            }
        }
    }

    // 设置NPC的目标目的地
    void MoveToDestination(Vector3 targetPosition)
    {
        if (targetPosition != Vector3.zero)
        {
            agent.SetDestination(targetPosition);
        }
    }

}