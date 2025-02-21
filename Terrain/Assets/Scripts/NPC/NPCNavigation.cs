using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavigation : MonoBehaviour
{
    public Transform goal;  
    private NavMeshAgent agent;

    public LayerMask groundLayer; 
    public float groundRaycastHeight = 100f;

    public float normalSpeed = 3.5f;
    public float linkSpeed = 0.6f; // 通过Link时的速度
    public float normalAcceleration = 8f;
    public float linkAcceleration = 2f;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;
        agent.acceleration = normalAcceleration;
        if (agent == null)
        {
            return;
        }

        if (goal != null)
        {
            Vector3 adjustedGoalPosition = GetGroundHeight(goal.position);
            agent.destination = adjustedGoalPosition;
        }
 
    }

    void Update()
    {
        if (goal != null)
        {
            Vector3 adjustedGoalPosition = GetGroundHeight(goal.position);

            if (agent.destination != adjustedGoalPosition)
            {
                agent.destination = adjustedGoalPosition;
            }
        }
        if (agent.isOnOffMeshLink)
        {
            agent.speed = linkSpeed;
            agent.acceleration = linkAcceleration;
        }
        else
        {
            agent.speed = normalSpeed;
            agent.acceleration = normalAcceleration;
        }
    }

    Vector3 GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * groundRaycastHeight, Vector3.down, out hit, groundRaycastHeight * 2, groundLayer))
        {
            return hit.point; 
        }
        return position; 
    }
}

