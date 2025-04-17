using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Flock/Behavior/Avoidance")]
public class AvoidanceBehavior :FlockBehavior
{
    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        //if no neighbours,return no adjustment
        if (context.Count == 0)
            return Vector3.zero;

        //add all points together and average
        Vector3 AvoidanceMove = Vector3.zero;
        int Avoid = 0;
        foreach (Transform item in context)
        {
            if (Vector3.SqrMagnitude(item.position - agent.transform.position) < flock.SquareAvoidanceRadius)
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
