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
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Light.SetLightColor();
        }
        //if(Physics.Raycast(Ray, out HitInfo))
        //Debug.DrawLine(transform.position, HitInfo.point, Color.green);
    }
    void ChangeRayColor()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
          
        }

    }

}
