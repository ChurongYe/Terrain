using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


public class Seasonchange : MonoBehaviour
{
    private List<GameObject> objectsToEdit; // 
    public Gradient autumnGradient; //Fall
    public Material fallmaterial;
    public Gradient Springmaterial;
    public Material springmaterial;
    public Gradient Summermaterial;
    public Material summermaterial;
    //public Gradient Winterterial;
    public Material wintermaterial;
    public Material wintermaterial2;
    public float transitionDuration = 3f; // color change time
    public float seasonDuration = 3f;// season change time;
    public bool Ifrefresh;
    public Shader shader;
    private Coroutine changeColorCoroutine;
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
        if (changeColorCoroutine != null)
        {
            StopCoroutine(changeColorCoroutine);
        }
        currentSeason = Season.Spring;
        Ifrefresh = true;
        //refresh
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
                        if (materials[i].shader != Shader.Find("Universal Render Pipeline/Lit"))
                        {
                            materials[i].shader = Shader.Find("Universal Render Pipeline/Lit");
                            Color targetColor = Springmaterial.Evaluate(Random.Range(0f, 1f));
                            materials[i].color = targetColor;
                        }
                        else
                        {
                            Color targetColor = Springmaterial.Evaluate(Random.Range(0f, 1f));
                            materials[i].color = targetColor;
                        }
                    }
                }
            }
        }
        Ifrefresh = false;
        changeColorCoroutine = StartCoroutine(ChangeColor());
    }
    private IEnumerator ChangeColor()
    {
        while (true)
        {
            if (Ifrefresh)
            {
                yield return null; 
                continue;
            }
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
                    yield return new WaitForSeconds(transitionDuration);
                    currentSeason = Season.Spring;
                    break;
            }

            yield return new WaitForSeconds(transitionDuration + seasonDuration);
        }
    }
    void StartSpringTransition(List<GameObject> objectsToEdit)
    {
        foreach (GameObject obj in objectsToEdit)
        {
            if (obj == null) continue;
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
                        materials[i].shader = Shader.Find("Universal Render Pipeline/Lit");
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
            if (obj == null) continue;
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
            if (obj == null) continue;
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
            if (obj == null) continue;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains("leafsGreen") || materials[i].name.Contains("leafsDark")
                    || materials[i].name.Contains("grass"))
                {
                    if (materials[i].name.Contains("grass"))
                    {
                        if (wintermaterial != null)
                        {
                            Color targetColor = wintermaterial.color;
                            StartCoroutine(LerpColor(materials[i], materials[i].color, targetColor, transitionDuration + 2f));
                        }
                    }
                    else
                    {
                        if (wintermaterial2 != null)
                        {
                            Color targetColor = wintermaterial2.color;
                            StartCoroutine(LerpThenSnow(materials[i], materials[i].color, targetColor, transitionDuration));
                        }
                    }
                }
            }
        }

    }
    private IEnumerator LerpThenSnow(Material material, Color startColor, Color targetColor, float duration)
    {
        yield return StartCoroutine(LerpColor(material, startColor, targetColor, duration));
        yield return StartCoroutine(TreeSnow(material));
    }
    IEnumerator TreeSnow(Material material)
    {
        material.shader = shader;
        //string[] propertyNames = ShaderUtil.GetPropertyNames(shader);

        if (material.HasProperty("threshold"))
        {

            StartCoroutine(LerpSnowThreshold(material, 1f, 0.1f, transitionDuration));
        }
        yield return null;
    }
    private IEnumerator LerpSnowThreshold(Material material, float startValue, float endValue, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float value = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            material.SetFloat("threshold", value);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        material.SetFloat("threshold", endValue);
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