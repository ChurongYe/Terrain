using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public int CenterCount = 4;
    public float CenterSpreadRange = 60f;
    private List<Transform> SpawnCenters = new List<Transform>();
    public LayerMask AnimalLayer;
    public FlockAgent AgentPrefab;
    List<FlockAgent> agents = new List<FlockAgent>();
    public FlockBehavior Behavior;
    [Range(10, 500)]
    public int StartingCount = 50;
    const float AgentDensity = 0.08f;
    [Range(1f, 100f)]
    public float DriveFactor = 10f; //make the movement more obvious
    [Range(1f, 100f)]
    public float MaxSpeed = 5f;//limit the speed
    [Range(1f, 10f)]
    public float NeighbourRadius = 1.5f;
    [Range(0f, 1f)]
    public float AvoidanceRadiusMultiplier = 0.5f;
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
    public IEnumerator AnimalSpawner()
    {
        squareMaxSpeed = MaxSpeed * MaxSpeed;
        squareNeighbourRadius = NeighbourRadius * NeighbourRadius;
        squareAvoidanceRadius = squareNeighbourRadius * AvoidanceRadiusMultiplier * AvoidanceRadiusMultiplier;
        SpawnCenters.Clear(); // 先清空旧的
        for (int c = 0; c < CenterCount; c++)
        {
            Vector3 centerPos = transform.position + new Vector3(
                Random.Range(-CenterSpreadRange, CenterSpreadRange),
                100f, // 空中射线检测
                Random.Range(-CenterSpreadRange, CenterSpreadRange)
            );

            if (Physics.Raycast(centerPos, Vector3.down, out RaycastHit centerHit, 200f, AnimalLayer))
            {
                GameObject centerObj = new GameObject("AutoCenter_" + c);
                centerObj.transform.position = centerHit.point;
                centerObj.transform.SetParent(transform); // 可以不设父物体，自己决定
                SpawnCenters.Add(centerObj.transform);
            }
            else
            {
                Debug.LogWarning($"Center {c} 找不到地面，跳过");
            }
        }

        // 然后就是用生成好的 SpawnCenters 来正常生成动物了
        for (int i = 0; i < StartingCount; i++)
        {
            if (SpawnCenters.Count == 0)
            {
                Debug.LogWarning("没有可用的生成中心点！");
                yield break;
            }

            Transform center = SpawnCenters[Random.Range(0, SpawnCenters.Count)];
            Vector3 randomOffset = Random.insideUnitSphere * StartingCount * AgentDensity;
            randomOffset.y = 0f;

            Vector3 randomPos = center.position + randomOffset;
            randomPos.y = 100f;

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 200f, AnimalLayer))
            {
                Vector3 spawnPos = hit.point;
                spawnPos.y += 1.5f;

                FlockAgent newAgent = Instantiate(AgentPrefab, spawnPos, Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)), transform);
                newAgent.name = "Agent" + i;
                newAgent.Initialize(this);
                agents.Add(newAgent);
            }
            else
            {
                Debug.LogWarning($"Agent {i} 找不到地面，跳过生成");
            }
        }

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (FlockAgent agent in agents)
        {
            List<Transform> context = GetNearbyObjects(agent);
            Vector3 move = Behavior.CalculateMove(agent, context, this);
            move *= DriveFactor;
            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * MaxSpeed;
            }
            move.y = 0;//
            agent.Move(move);
        }
    }
    List<Transform> GetNearbyObjects(FlockAgent agent)
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(agent.transform.position, NeighbourRadius);
        foreach (Collider collider in contextColliders)
        {
            if (collider != agent.AgentCollider)
            {
                context.Add(collider.transform);
            }
        }
        return context;
    }
    public IEnumerator RefreshAnimals()
    {
        // clear
        foreach (FlockAgent agent in agents)
        {
            if (agent != null)
                Destroy(agent.gameObject);
        }
        agents.Clear();
        yield return null;

    }
}
