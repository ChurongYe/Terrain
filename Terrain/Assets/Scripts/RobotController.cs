using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class RobotController : MonoBehaviour
{
    public enum RobotState { Walking, Idle }

    public RobotState robotState = RobotState.Walking;

    public LayerMask groundLayer;
    public GameObject markerPrefab;
    public float idleTime = 5f;
    public Outline outline;
    private NavMeshAgent agent;
    private Animator animator;
    private GameObject currentMarker;
    private bool waitingForSecondClick = false;
    private float clickTimer = 0f;
    private Vector3 pendingClickPosition;

    void Start()
    {
        outline.OutlineColor = Color.green;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        robotState = RobotState.Walking;
        RandomWalk();
    }

    void Update()
    {
        HandleMouseClick();
        UpdateMovement();
        HandleClickTimer();
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (!waitingForSecondClick)
                {
                    // robot click
                    if (hit.collider.gameObject == this.gameObject)
                    {
                        outline.OutlineColor = Color.red;
                        clickTimer = 0f;
                        waitingForSecondClick = true;
                        robotState = RobotState.Idle;
                        agent.ResetPath();
                        UpdateAnimation();
                        Debug.Log("First click on robot, waiting for second click...");
                    }
                }
                else
                {
                    // ground click
                    if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
                    {
                        waitingForSecondClick = false;
                        agent.SetDestination(hit.point);
                        robotState = RobotState.Walking;
                        clickTimer = 0f;

                        // marker
                        if (currentMarker != null)
                            Destroy(currentMarker);

                        if (markerPrefab != null)
                            currentMarker = Instantiate(markerPrefab, hit.point, Quaternion.identity);

                        Debug.Log($"Second click on ground! Moving to {hit.point}");

                        UpdateAnimation();
                    }
                }
            }
        }
    }

    void HandleClickTimer()
    {
        if (waitingForSecondClick)
        {
            clickTimer += Time.deltaTime;
            if (clickTimer > 3f)
            {
                outline.OutlineColor = Color.green;
                Debug.Log("Second click timeout, returning to walking");
                waitingForSecondClick = false;
                robotState = RobotState.Walking;
                RandomWalk();
                UpdateAnimation();
            }
        }
    }

    void UpdateMovement()
    {
        if (robotState == RobotState.Walking)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // to target
                robotState = RobotState.Idle;
                outline.OutlineColor = Color.green ;
                UpdateAnimation();
                Debug.Log("Reached destination, now idle.");

                if (currentMarker != null)
                {
                    Destroy(currentMarker);
                    currentMarker = null;
                }

                // continue walk
                Invoke(nameof(RandomWalk), idleTime);
            }
        }
    }

    void RandomWalk()
    {
        if (robotState != RobotState.Idle) return;

        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
            robotState = RobotState.Walking;
            UpdateAnimation();
            Debug.Log($"Random walking to {navHit.position}");
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        if (robotState == RobotState.Walking)
            animator.SetBool("IsWalking", true);
        else
            animator.SetBool("IsWalking", false);
    }
}