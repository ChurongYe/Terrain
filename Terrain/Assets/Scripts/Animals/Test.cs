using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    [SerializeField]
    protected List<AnimalSettings> Animals = new List<AnimalSettings>();
    //[SerializeField]
    //private List<Transform> SpawnPoints = new List<Transform>();
    //[SerializeField]
    //private List<BoxCollider> SpawnAreas;
    public float SpawnRadius = 15f;
    public int SpawnPointAmount = 5;//
    public float SpawnBoxX = 5f;
    public float SpawnBoxZ = 4f;
    public LayerMask CreatureMask;
    //public int LandMask;
    public LayerMask LandMask;
    public float RayDistance = 10f;
    protected List<Transform> Points = new List<Transform>();
    public float Upset = 1f;
}
