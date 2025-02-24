using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum AnimalState
{
    Idle,
    Moving,
}
public class Animal : MonoBehaviour
{
    [Header("Move")]
    public float WanderDistance;
    public float MoveSpeed;
    public float MoveTime;
    [Header("Idle")]
    public float IdleTime;
    [Header("Produce")]
    public GameObject ProductPrefab;
    private bool IsProducing = false;

    protected NavMeshAgent Agent;
    protected AnimalState CurrentState = AnimalState.Idle;
    // Start is called before the first frame update
    void Start()
    {
        InitialiseAnimal();
    }
    private void Update()
    {
        ChangeFigure();
    }

    protected virtual void InitialiseAnimal()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = MoveSpeed;
        CurrentState = AnimalState.Idle;
        UpdateState();
    
    }
    protected virtual void UpdateState()
    {
        switch (CurrentState)
        {
            case AnimalState.Idle:
                HandleIdle();
                break;
            case AnimalState.Moving:
                HandleMoving();
                break;  
        }
    }
    protected Vector3 ToRandomPosition(Vector3 StartPoint, float Radius)
    {
        Vector3 MoveDir = Random.insideUnitSphere * Radius;
        MoveDir += StartPoint;
        NavMeshHit Hit;
        if (NavMesh.SamplePosition(MoveDir, out Hit, Radius, NavMesh.AllAreas))
            return Hit.position;
        else
            return ToRandomPosition(StartPoint, Radius);
    }
    protected virtual void HandleIdle()
    {
        StartCoroutine(WaitToMove());
    }
    private IEnumerator WaitToMove()
    {
        float WaitTime = Random.Range(IdleTime / 2, IdleTime * 2);
        yield return new WaitForSeconds(WaitTime);
        Vector3 Destination = ToRandomPosition(transform.position, WanderDistance);
        Agent.SetDestination(Destination);
        SetState(AnimalState.Moving);
    }

    protected void SetState(AnimalState NewState)
    {
        if (CurrentState == NewState)
            return;
        CurrentState = NewState;
        OnStateChanged(NewState);
    }

    protected virtual void OnStateChanged(AnimalState newState)
    {
        UpdateState();
    }

    protected virtual void HandleMoving()
    {
        StartCoroutine(WaitToDestination());
    }

    private IEnumerator WaitToDestination()
    {
        float StartTime = Time.deltaTime;
        while (Agent.remainingDistance > Agent.stoppingDistance)
        {
            if (Time.deltaTime - StartTime >= MoveTime)
            {
                Agent.ResetPath();
                SetState(AnimalState.Idle);
                yield break;
            }
            yield return null;
        }
        SetState(AnimalState.Idle);
    }
    private void ChangeFigure()
    {
        float GrowTime = 10f;   //10s from size1 to size2
        //float GrowSpeed = 0.1f;
        //transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, GrowSpeed);
        transform.localScale += Vector3.one / GrowTime * Time.deltaTime;

        if (transform.localScale.magnitude >= 2 * Vector3.one.magnitude&&!IsProducing) //mature(can produce product)
        {
            IsProducing = true;
            InvokeRepeating(nameof(ProduceProduct),0f, 5f);//invoke:only call once
        }
    }
    private void ProduceProduct()
    {
        float ProductDistance = 0.5f;
        Vector3 ProductPoint = new Vector3(transform.position.x+ProductDistance, 0, transform.position.z);
        GameObject Product=Instantiate(ProductPrefab,ProductPoint, Quaternion.identity);
        StartCoroutine(RemoveProduct(Product));
    }

    private IEnumerator RemoveProduct(GameObject Product)
    {
        yield return new WaitForSeconds(5);
        Destroy(Product);
    }
}
