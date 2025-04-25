using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Seasonchange : MonoBehaviour
{
    public GameObject[] objectsToEdit; // 拖入你要修改的 GameObjects
    public Material newMaterial;       // 替换用的新材质（可选）

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            objectsToEdit =
     GameObject.FindGameObjectsWithTag("Prefab")
     .Concat(GameObject.FindGameObjectsWithTag("tile"))
     .ToArray();

            foreach (GameObject obj in objectsToEdit)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer == null) continue;

                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.Contains("leafsGreen") || materials[i].name.Contains("leafsDark") || materials[i].name.Contains("grass"))
                    {
                        //替换材质（可选）
                        if (newMaterial != null)
                            materials[i] = newMaterial;
                    }
                }

                renderer.materials = materials; // 应用更改
            }
        }
    }
}