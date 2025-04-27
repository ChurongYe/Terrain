using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public CharacterController Controller;
    [Header("Move")]
    public float Horizontal;
    public float Vertical;
    float YMove;
    public float MoveSpeed=5;
    [Header("Ray")]
    Ray Ray;
    RaycastHit HitInfo;
    public EmitLight Light;
    public GameObject robotPrefab;
    public int maxRobots = 3;
    private List<GameObject> spawnedRobots = new List<GameObject>();
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMove();
        EmitRay();
    }
    void HandleMove()
    {
        Horizontal = Input.GetAxis("Horizontal");
        Vertical = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Q))
            YMove = 1;
        else if (Input.GetKey(KeyCode.E))
            YMove = -1;
        else
            YMove = 0;
        Controller.Move(new Vector3(Horizontal, YMove, Vertical).normalized * Time.deltaTime * MoveSpeed);
    }
    void EmitRay()
    {
        Ray = new Ray(transform.position, -transform.up);
        if (Input.GetKeyDown(KeyCode.J))
        {
            Light.SetLightVisibility();
            if(Light.ifrobot)
            SpawnRobot();
        }
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    Light.SetLightColor();
        //}
        //if(Physics.Raycast(Ray, out HitInfo))
        //Debug.DrawLine(transform.position, HitInfo.point, Color.green);
    }
    void SpawnRobot()
    {
        if (spawnedRobots.Count >= maxRobots)
        {
            Debug.Log("Maximum robots reached!");
            return;
        }

        if (robotPrefab != null )
        {
            Vector3 rayOrigin = transform .position;
            Ray ray = new Ray(rayOrigin, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point + Vector3.up * 3f;
                GameObject newRobot = Instantiate(robotPrefab, spawnPosition, Quaternion.identity);
                spawnedRobots.Add(newRobot);
            }
            else
            {
                Debug.LogWarning("No ground found below player to spawn robot!");
            }
        }
    }

    public IEnumerator ClearRobots()
    {
        foreach (GameObject robot in spawnedRobots)
        {
            if (robot != null)
            {
                Destroy(robot);
            }
        }
        spawnedRobots.Clear();
        yield return null;
    }
}

    //void ChangeRayColor()
    //{
    //    if (Input.GetKeyDown(KeyCode.K))
    //    {
          
    //    }

    //}


