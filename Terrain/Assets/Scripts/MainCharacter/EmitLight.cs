using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitLight : MonoBehaviour
{
    private Renderer Renderer;
    private Color OriginalColor;
    private int ColorCount=0;
    void Awake()
    {
        Renderer = GetComponent<Renderer>();
        OriginalColor = Renderer.material.color;
    }
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetLightVisibility()
    {
          gameObject.SetActive(!gameObject.activeSelf); 
    }
    public void SetLightColor()
    {
        ColorCount = (ColorCount + 1) % 3;
        switch (ColorCount)
        {
            case 0:
                Renderer.material.color = OriginalColor;
                break;
            case 1:
                Renderer.material.color = Color.red;
                break;
            case 2:
                Renderer.material.color = Color.yellow;
                break;
        }
    }
}
