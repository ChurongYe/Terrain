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
    public GameObject ExclamationMark;
    private bool IsProducing ;
    public bool CanProduce ;
    public bool CanHarvest ;
    public bool Harvested = true;
    //private int ProduceCount = 0;
    [Header("Animation")]
    public Animator Ani;

    protected NavMeshAgent Agent;
    protected AnimalState CurrentState = AnimalState.Idle;

    void Start()
    {
        InitialiseAnimal();
    }
    private void Update()
    {
        ChangeFigure();
        SwitchAnimation();
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
        for (int i = 0; i < 5; i++)
        {
            Vector3 MoveDir = Random.insideUnitSphere * Radius;
            MoveDir += StartPoint;
            NavMeshHit Hit;
            if (NavMesh.SamplePosition(MoveDir, out Hit, Radius, NavMesh.AllAreas))
                return Hit.position;
        }
        return StartPoint;
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
        while (Agent.pathPending||Agent.remainingDistance > Agent.stoppingDistance&&Agent.isActiveAndEnabled )
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
        float GrowTime = 10f;   //10s from size(young) to size(grown-up)
        //float GrowSpeed = 0.1f;
        //transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, GrowSpeed);
        if(transform.localScale.magnitude < 2 * Vector3.one.magnitude)
        transform.localScale += Vector3.one / GrowTime * Time.deltaTime;

        if (transform.localScale.magnitude >= 2 * Vector3.one.magnitude)      //mature(can produce product)
        {
            if (!IsProducing)
            {
                IsProducing = true;
                InvokeRepeating(nameof(ControlCanProduce),0f, 5f);
            }
            HandleExclamationMark(); //this appears above animal(show it is mature)
            //InvokeRepeating(nameof(ProduceProduct),0f, 5f);     //invoke:only call once

            ControlHarvest();

        }
    }
 
    private void HandleExclamationMark()
    {
        if(CanProduce)
            ExclamationMark.SetActive(true);
        if (Harvested)
            ExclamationMark.SetActive(false);
    }
    private void ControlCanProduce()
    {
        //if (Random.Range(0,5)==2)               //can produce randomly
        //{
        //    CanProduce=true;                                                                    //npc needs this signal
        //}
        CanProduce = true;
        Harvested = false;
    }
    protected virtual void ProduceProduct()
    {
        float ProductDistance = 2f;
        Vector3 ProductPoint = new Vector3(transform.position.x,transform.position.y+ProductDistance, transform.position.z);
        GameObject Product=Instantiate(ProductPrefab,ProductPoint, Quaternion.identity);
        //ProduceCount++;
        //HarvestTimesBeforeDeath();        //wriiten in specific animals
        StartCoroutine(RemoveProduct(Product));
        //if (ProduceCount >= 3)          
        //    DestroyAnimal();
    }
    //private void HarvestTimesBeforeDeath(int Times)
    //{
    //    if (ProduceCount >= Times)      //npc harvests products ()times from the animal and then it will die/disappear
    //        DestroyAnimal();
    //}
    private IEnumerator RemoveProduct(GameObject Product)
    {
        yield return new WaitForSeconds(1);
        Destroy(Product);
    }
    private void ControlHarvest()
    {
        if (CanHarvest&&!Harvested)         //NPC gets close->can harvest->products pop up
        {
            ProduceProduct();
            CanHarvest = false;
            Harvested = true;
        }

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
            CanHarvest = true;
    }
    //private void DestroyAnimal()
    //{
    //    StopAllCoroutines();
    //    Destroy(gameObject);
    //}
    void SwitchAnimation()
    {
        Ani.SetFloat("Speed", Agent.velocity.sqrMagnitude);
    }
}
