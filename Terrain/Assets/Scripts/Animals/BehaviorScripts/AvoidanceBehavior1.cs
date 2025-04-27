using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Flock/Behavior/AvoidanceObstacle")]
public class AvoidanceBehaviorForObstacle :FilterFlockBehavior
{
    public float AvoidObstacleRadius;
    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        //if no neighbours,return no adjustment
        if (context.Count == 0)
            return Vector3.zero;

        //add all points together and average
        Vector3 AvoidanceMove = Vector3.zero;
        int Avoid = 0;
        List<Transform> filteredContext = (filter == null) ? context : filter.Filter(agent, context);
        foreach (Transform item in filteredContext)
        {
            if (Vector3.SqrMagnitude(item.position - agent.transform.position) < AvoidObstacleRadius)
            {
                Avoid++;
                AvoidanceMove += agent.transform.position - item.position;
            }
        }
        if (Avoid > 0)
            AvoidanceMove /= Avoid;
        return AvoidanceMove;
    }
}
