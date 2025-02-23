using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float Horizontal;
    float Vertical;
    float UpDown;
    public float MoveSpeed=5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }
    void Movement()
    {
        //Horizontal = Input.GetAxis("Horizontal");
        //Vertical = Input.GetAxis("Vertical");
        //if (Input.GetKey(KeyCode.Q))
        //    UpDown = 1;
        //else if (Input.GetKey(KeyCode.E))
        //    UpDown = -1;
        //else
        //    UpDown = 0;
        //Vector3 Movedir = (new Vector3(Horizontal, 0, Vertical)+new Vector3(0,UpDown,0)).normalized ;
        //transform.position += Movedir * Time.deltaTime * MoveSpeed;

    }
}
