using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit :Plant
{
    public float RayDistance=20f;
    protected override void SwitchState()
    {
        PositionMultipler = -0.5f;
        if (IsMature)
        {
            transform.localScale = Vector3.one * SizeMultipler;
            transform.position = StartPosition + Vector3.up * PositionMultipler;
            CheckGround();
        }
    }
    protected override void ControlMature()
    {
        {
            //if ()
            //    IsMature = true;
        }
    }
    void CheckGround()
    {
        RaycastHit Info;
        //TODO:add layermask(ground)
        if (Physics.Raycast(transform.position, Vector3.down,out Info,RayDistance,11))//add layermask(ground):11
        {
            gameObject.transform.position = new Vector3(transform.position.x, Info.point.y, transform.position.z);
        }
    }

}
