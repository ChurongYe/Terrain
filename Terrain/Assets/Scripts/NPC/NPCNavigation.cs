using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavigation : MonoBehaviour
{
    public Transform village; // ��ׯ��λ��
    public Transform farmland; // ũ���λ��
    public float movementSpeed = 3.5f; // NPC���ƶ��ٶ�
    public float stoppingDistance = 0.5f; // ����Ŀ�ĵصľ�����ֵ
    private NavMeshAgent agent; // ���ڿ���NPC��NavMeshAgent

    void Start()
    {
        // ��ȡ NavMeshAgent ���
        agent = GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;
        agent.stoppingDistance = stoppingDistance;

        // ��ʼ�Ӵ�ׯ��ũ���·��
        MoveToDestination(village.position); // ��ʼ�Ӵ�ׯ��ʼ
    }

    void Update()
    {
        // ���NPC����Ŀ�ĵأ��ı�Ŀ�ĵ�Ϊũ��
        if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
        {
            if (agent.destination == village.position)
            {
                // NPC�����ׯ�󣬿�ʼ��ũ���ƶ�
                MoveToDestination(farmland.position);
            }
        }
    }

    // ����NPC��Ŀ��Ŀ�ĵ�
    void MoveToDestination(Vector3 targetPosition)
    {
        if (targetPosition != Vector3.zero)
        {
            agent.SetDestination(targetPosition);
        }
    }

}