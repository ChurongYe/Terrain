using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavigation : MonoBehaviour
{
    public GameObject[] npcPrefab; 
    public LayerMask groundLayer;
    public LayerMask obstacleLayer; 
    public float groundRaycastHeight = 100f;
    public List<Vector3> crops = new List<Vector3>();//harvest position
    public float minMoveInterval = 3f;
    public float maxMoveInterval = 15f;
    public float stopTimeLimit = 5f;

    public float normalSpeed = 3.5f;
    public float linkSpeed = 0.6f;
    public float normalAcceleration = 8f;
    public float linkAcceleration = 2f;

    private List<GameObject> npcs = new List<GameObject>();

    void Update()
    {
        if (npcs == null || npcs.Count == 0)
            return;

        for (int i = npcs.Count - 1; i >= 0; i--) 
        {
            GameObject npc = npcs[i];
            if (npc == null)
            {
                npcs.RemoveAt(i);
                continue; 
            }

            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                npcs.RemoveAt(i); 
                continue;
            }

            Animator animator = npc.GetComponent<Animator>();
            NPCController npcController = npc.GetComponent<NPCController>();
            NPCManager npcManager = npc.GetComponent<NPCManager>();
            if (npcManager == null)
            {
                npcManager = npc.AddComponent<NPCManager>();
            }
            if (npcController != null && npcManager != null)
            {
                if (npcManager.npcState == NPCManager. NPCState.Normal)
                {
                    if (agent.isOnOffMeshLink)
                    {
                        agent.speed = npcController.speed * 0.5f;
                        agent.acceleration = npcController.acceleration * 0.5f;
                    }
                    else
                    {
                        agent.speed = npcController.speed;
                        agent.acceleration = npcController.acceleration;
                    }
                    //IfNPCstop
                    if (agent.velocity.magnitude < 0.1f)
                    {
                        npcController.stopTime += Time.deltaTime;
                        if (npcController.stopTime >= stopTimeLimit)
                        {
                            npcController.stopTime = 0;
                            SetRandomDestinationGlobal(agent);
                        }
                    }
                    else
                    {
                        npcController.stopTime = 0;
                    }

                    // Animation
                    if (animator != null)
                    {
                        float speed = agent.velocity.magnitude;
                        animator.SetBool("isWalking", speed > 0.1f);
                    }
                }
                else if (npcManager.npcState == NPCManager.NPCState.Resting)
                {
                    agent.ResetPath();
                    if (animator != null)
                    {
                        animator.SetBool("isWalking", false);
                        animator.SetTrigger("Resting");
                    }
                }
                else if (npcManager.npcState == NPCManager.NPCState.Harvesting)
                {
                    if (!crops.Contains(npcController.harvestTargetPosition))
                    {
                        npcManager.npcState = NPCManager.NPCState.Normal;
                        SetRandomDestinationGlobal(agent);
                    }
                    else if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        StartCoroutine(HarvestCrop(npcManager, npcController));
                    }
                }
            }
        }
    }
    public void MoveToTarget(Vector3 target)
    {
        foreach (GameObject npc in npcs)
        {
            if (npc == null) continue;
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.SetDestination(target);
            }
        }
    }
    public Vector3? FindNearestHarvestableCrop(Vector3 npcPosition)
    {
        if (crops.Count == 0)
            return null;

        Vector3 closestCrop = Vector3.zero;
        float minDistance = float.MaxValue;

        foreach (Vector3 crop in crops)
        {
            float distance = Vector3.Distance(npcPosition, crop);
            if (distance < minDistance)
            {
                closestCrop = crop;
                minDistance = distance;
            }
        }

        return minDistance < float.MaxValue ? closestCrop : null;
    }
    IEnumerator HarvestCrop(NPCManager npcManager, NPCController npcController)
    {
        Animator animator = npcController.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("Harvest");
        }

        yield return new WaitForSeconds(3f);

        crops.Remove(npcController.harvestTargetPosition); // 移除已收获作物
        npcManager.npcState = NPCManager.NPCState.Normal;
        SetRandomDestinationGlobal(npcController.GetComponent<NavMeshAgent>());
    }

    public IEnumerator SpawnNPCs(int count)
    {
        foreach (GameObject npc in npcs)
        {
            if (npc != null)
            {
                Destroy(npc);  
            }
        }
        npcs.Clear();
        count = Mathf.Min(count, npcPrefab.Length);

        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < npcPrefab.Length; i++)
        {
            availableIndexes.Add(i);
        }

        ShuffleList(availableIndexes);

        for (int i = 0; i < count; i++)
        {
            int index = availableIndexes[i]; 
            GameObject npcPrefabItem = npcPrefab[index];

            if (npcPrefabItem == null) continue;

            Vector3 spawnPosition;
            if (FindValidSpawnPosition(out spawnPosition))
            {
                GameObject npc = Instantiate(npcPrefabItem, spawnPosition, Quaternion.identity);
                npcs.Add(npc);
                npc.AddComponent<NPCController>();
                NPCController npcController = npc.GetComponent<NPCController>();

                npcController.speed = Random.Range(3f, 6f);  
                npcController.acceleration = Random.Range(4f, 10f); 

                NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    StartCoroutine(Wander(agent, npc.GetComponent<Animator>()));
                }
            }
        }
        yield return null;
    }

    private void ShuffleList(List<int> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    bool FindValidSpawnPosition(out Vector3 spawnPosition)
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        if (navMeshData.vertices.Length == 0)
        {
            spawnPosition = Vector3.zero;
            return false;
        }

        for (int i = 0; i < 10; i++)
        {
            int triangleIndex = Random.Range(0, navMeshData.indices.Length / 3);
            Vector3 p1 = navMeshData.vertices[navMeshData.indices[triangleIndex * 3]];
            Vector3 p2 = navMeshData.vertices[navMeshData.indices[triangleIndex * 3 + 1]];
            Vector3 p3 = navMeshData.vertices[navMeshData.indices[triangleIndex * 3 + 2]];

            Vector3 randomPoint = GetRandomPointInTriangle(p1, p2, p3);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                if (!IsInsideMountain(hit.position))
                {
                    Collider[] colliders = Physics.OverlapSphere(hit.position, 1f, obstacleLayer);
                    if (colliders.Length == 0)
                    {
                        spawnPosition = hit.position;
                        return true;
                    }
                }
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }
    bool IsInsideMountain(Vector3 position)
    {
        Vector3 rayOrigin = position + Vector3.up * 50f; 
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, obstacleLayer))
        {
            return true;
        }
        return false;
    }
    Vector3 GetRandomPointInTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;
        return (1 - r1) * p1 + (r1 * (1 - r2)) * p2 + (r1 * r2) * p3;
    }
    IEnumerator Wander(NavMeshAgent agent, Animator animator)
    {
        while (true)
        {
            float moveInterval = Random.Range(minMoveInterval, maxMoveInterval);
            yield return new WaitForSeconds(moveInterval); 
            SetRandomDestinationGlobal(agent);
        }
    }

    void SetRandomDestinationGlobal(NavMeshAgent agent)
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        if (navMeshData.vertices.Length == 0)
        {
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            int randomIndex = Random.Range(0, navMeshData.vertices.Length);
            Vector3 randomPoint = navMeshData.vertices[randomIndex];

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                if (agent == null)
                    return;
                if (!IsPathBlocked(agent.transform.position, hit.position))
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        Debug.LogWarning("no NavMesh position");
    }
    bool IsPathBlocked(Vector3 startPosition, Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(startPosition, targetPosition, NavMesh.AllAreas, path))
        {
            return path.status != NavMeshPathStatus.PathComplete;
        }
        return true;
    }
}

public class NPCController : MonoBehaviour
{
    public float stopTime = 0;
    public float speed;
    public float acceleration;
    public Vector3 harvestTargetPosition; // 目标作物的位置
    public bool isHarvesting = false; // 是否正在收获
}