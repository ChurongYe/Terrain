using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behavior/Stay In Radius")]
public class StayInRadiusBehavior : FlockBehavior
{
    public Vector3 Center;
    public float Radius = 15;
    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        Vector3 centerOffset = Center - agent.transform.position;
        float t = centerOffset.magnitude / Radius;
        if (t < 0.8)
            return Vector3.zero;
        else
            return centerOffset * t * t;
    }
}
