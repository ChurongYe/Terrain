using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public LayerMask animalLayer;
    public FlockAgent agentPrefab;
    List<FlockAgent> agents = new List<FlockAgent>();
    public FlockBehavior behavior;
    [Range(10, 500)]
    public int startingCount = 50;
    const float AgentDensity = 0.08f;
    [Range(1f, 100f)]
    public float driveFactor = 10f; //make the movement more obvious
    [Range(1f, 100f)]
    public float maxSpeed = 5f;//limit the speed
    [Range(1f, 10f)]
    public float neighbourRadius = 1.5f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;
    float squareMaxSpeed;
    float squareNeighbourRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius
    {
        get
        {
            return squareAvoidanceRadius;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighbourRadius = neighbourRadius * neighbourRadius;
        squareAvoidanceRadius = squareNeighbourRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
        for (int i = 0; i < startingCount; i++)
        {
            Vector3 randomPos = Random.insideUnitSphere * startingCount * AgentDensity;
            randomPos.y = 10f; // 从空中往下射线
            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 20f, animalLayer))
            {
                Vector3 spawnPos = hit.point;
                spawnPos.y = hit.point.y + 15f;//avoid animals embeded in ground
                //FlockAgent newAgent = Instantiate(agentPrefab, Random.insideUnitSphere * startingCount * AgentDensity, Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)), transform);
                FlockAgent newAgent = Instantiate(agentPrefab, spawnPos, Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)), transform);
                newAgent.name = "Agent" + i;
                newAgent.Initialize(this);//
                agents.Add(newAgent);
            }
            else
            {
                Debug.LogWarning($"Agent {i} 找不到地面，跳过生成");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (FlockAgent agent in agents)
        {
            List<Transform> context = GetNearbyObjects(agent);
            Vector3 move = behavior.CalculateMove(agent, context, this);
            move *= driveFactor;
            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }
            move.y = 0;//
            agent.Move(move);
        }
    }
    List<Transform> GetNearbyObjects(FlockAgent agent)
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(agent.transform.position, neighbourRadius);
        foreach (Collider collider in contextColliders)
        {
            if (collider != agent.AgentCollider)
            {
                context.Add(collider.transform);
            }
        }
        return context;
    }
}
