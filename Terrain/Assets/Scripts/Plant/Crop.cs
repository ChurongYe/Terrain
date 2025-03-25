using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : Plant
{
    protected override void SwitchState()
    {
        PositionMultipler = 0.1f;
        base.SwitchState();
    }
    protected override void ControlMature()
    {
        {
            //if ()
            //    IsMature = true;
        }
    }
}
