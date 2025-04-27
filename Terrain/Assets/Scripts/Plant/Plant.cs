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
    public Transform HarvestPoint;
    public float SuperVegProbability;
    protected Vector3 StartPosition;
    private Vector3 OriginalScale;
    private float Timer;//
    private float MatureTime;//
    private bool HasSwitched = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        StartPosition= transform.position;
        MatureTime = Random.Range(3f, 5f);//
        Timer = 0f;//
        OriginalScale = transform.localScale;
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
            if (HasSwitched)
                return; // 如果已经切换过，就什么也不做
            if (!IsMature)
                return;   // 如果还没成熟，也不做
            HasSwitched = true;
            transform.position = StartPosition + Vector3.up * PositionMultipler;
            Collider col = GetComponent<Collider>();
            col.enabled = false;

            if (SuperVegProbability > Random.Range(0f, 1f))
            {

                transform.localScale = OriginalScale * SizeMultipler * SizeMultipler;

            }
            else
                transform.localScale = OriginalScale * SizeMultipler;
            StartCoroutine(ReenableCollider());

    }
    IEnumerator ReenableCollider()
    {
        yield return new WaitForSeconds(0.2f);
        Collider col = GetComponent<Collider>();
        col.enabled = true;
    }

    protected virtual void DisappearBeforeHarvest() 
    {
        if (Harvested)
            Destroy(gameObject,1f);
    }
}
