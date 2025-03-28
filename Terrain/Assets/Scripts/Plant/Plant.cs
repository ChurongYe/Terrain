using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Plant : MonoBehaviour
{
    public bool IsMature;
    public bool Harvested;
    public float SizeMultipler=2;
    public float PositionMultipler = 0.1f;
    protected Vector3 StartPosition;
    public Transform HarvestPoint;
    private float Timer;//
    private float MatureTime;//
    // Start is called before the first frame update
    protected virtual void Start()
    {
        StartPosition= transform.position;
        MatureTime = Random.Range(3f, 5f);//
        Timer = 0f;//
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        DisappearBeforeHarvest();
        ControlMature();
        SwitchState();
        Timer += Time.deltaTime;//
        if (Timer >= MatureTime)//
            IsMature = true;//

    }
    protected abstract void ControlMature();

    protected virtual void SwitchState()//to mature state
    {
        if (IsMature)
        {
            transform.localScale = Vector3.one * SizeMultipler;
            transform.position = StartPosition + Vector3.up * PositionMultipler;
        }

    }

    protected virtual void DisappearBeforeHarvest() 
    {
        if (Harvested)
            Destroy(gameObject,2f);
    }
}
