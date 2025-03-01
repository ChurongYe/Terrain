using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public bool IsMature;
    public bool Harvested;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DisappearBeforeHarvest();
        ControlMature();
        SwitchState();
    }
    void SwitchState()//to mature state
    {
        if(IsMature)
        transform.localScale = Vector3.one * 2;
    }
    void ControlMature()
    {
        //if ()
        //    IsMature = true;
    }
    void DisappearBeforeHarvest()
    {
        if (Harvested)
            Destroy(gameObject);
    }
}
