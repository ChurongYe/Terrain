using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Seasonchange : MonoBehaviour
{
    private List<GameObject> objectsToEdit; // 拖入你要修改的 GameObjects
    public Gradient autumnGradient; //Fall
    public Material fallmaterial;
    public Gradient Springmaterial;
    public Material springmaterial;
    public Gradient Summermaterial;
    public Material summermaterial;
    public Gradient Winterterial;
    public Material wintermaterial;
    public float transitionDuration = 3f; // 颜色过渡时长（秒）
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
    private Season currentSeason = Season.Spring;
    public IEnumerator StartColor()
    {
        objectsToEdit = GameObject.FindGameObjectsWithTag("Prefab")
          .Concat(GameObject.FindGameObjectsWithTag("tile"))
          .ToList();
        if (objectsToEdit.Count == 0)
        {
            Debug.LogWarning("No objects found with tags 'Prefab' or 'tile'.");
            yield break;
        }
        foreach (GameObject obj in objectsToEdit)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains("leafsGreen") || materials[i].name.Contains("leafsDark") || materials[i].name.Contains("grass"))
                {
                    if (materials[i].name.Contains("grass"))
                    {
                        if (springmaterial != null)
                        {
                            Color targetColor = springmaterial.color;
                            materials[i].color = targetColor;
                        }
                    }
                    else
                    {
                        Color targetColor = Springmaterial.Evaluate(Random.Range(0f, 1f));
                        materials[i].color = targetColor;
                    }
                }
            }
        }
        yield return StartCoroutine(ChangeColor());
    }
    private IEnumerator ChangeColor()
    {
        while (true)
        {
            switch (currentSeason)
            {
                case Season.Spring:
                    StartSpringTransition(objectsToEdit);
                    currentSeason = Season.Summer;
                    break;

                case Season.Summer:
                    StartSummerTransition(objectsToEdit);
                    currentSeason = Season.Autumn;
                    break;

                case Season.Autumn:
                    StartAutumnTransition(objectsToEdit);
                    currentSeason = Season.Winter;
                    break;
                case Season.Winter:
                    StartWinterTransition(objectsToEdit);
                    currentSeason = Season.Spring;
                    break;
            }

            yield return new WaitForSeconds(transitionDuration + 2f); // 留出时间观察过渡
        }
    }
    void StartSpringTransition(List<GameObject> objectsToEdit)
    {
        foreach (GameObject obj in objectsToEdit)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains("leafsGreen") || materials[i].name.Contains("leafsDark") || materials[i].name.Contains("grass"))
                {
                    if (materials[i].name.Contains("grass"))
                    {
                        if (springmaterial != null)
                        {
                            Color targetColor = springmaterial.color;
                            StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration));
                        }
                    }
                    else
                    {
                        Color targetColor = Springmaterial.Evaluate(Random.Range(0f, 1f));
                        StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration));
                    }
                }
            }
        }

    }
    void StartSummerTransition(List<GameObject> objectsToEdit)
    {
        foreach (GameObject obj in objectsToEdit)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains("leafsGreen") || materials[i].name.Contains("leafsDark") || materials[i].name.Contains("grass"))
                {
                    if (materials[i].name.Contains("grass"))
                    {
                        if (summermaterial != null)
                        {
                            Color targetColor = summermaterial.color;
                            StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration));
                        }
                    }
                    else
                    {
                        Color targetColor = Summermaterial.Evaluate(Random.Range(0f, 1f));
                        StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration));
                    }
                }
            }
        }
    }
    void StartAutumnTransition(List<GameObject> objectsToEdit)
    {
        foreach (GameObject obj in objectsToEdit)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains("leafsGreen") || materials[i].name.Contains("leafsDark") || materials[i].name.Contains("grass"))
                {
                    if (materials[i].name.Contains("grass"))
                    {
                        if (fallmaterial != null)
                        {
                            Color targetColor = fallmaterial.color;
                            StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration));
                        }
                    }
                    else
                    {
                        Color targetColor = autumnGradient.Evaluate(Random.Range(0f, 1f));
                        StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration));
                    }
                }
            }
        }
    }
    void StartWinterTransition(List<GameObject> objectsToEdit)
    {
        foreach (GameObject obj in objectsToEdit)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains("leafsGreen") || materials[i].name.Contains("leafsDark") || materials[i].name.Contains("grass"))
                {
                    if (materials[i].name.Contains("grass"))
                    {
                        if (wintermaterial != null)
                        {
                            Color targetColor = wintermaterial.color;
                            StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration));
                        }
                    }
                    else
                    {
                        Color targetColor = Winterterial.Evaluate(Random.Range(0f, 1f));
                        StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration));
                    }
                }
            }
        }

    }
    private IEnumerator LerpColor(Material mat, Color startColor, Color endColor, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            Color lerpedColor = Color.Lerp(startColor, endColor, t);
            mat.color = lerpedColor;
            time += Time.deltaTime;
            yield return null;
        }
        mat.color = endColor;
    }

}