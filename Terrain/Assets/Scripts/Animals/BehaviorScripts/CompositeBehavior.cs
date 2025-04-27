using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behavior/Composite")]
public class CompositeBehavior : FlockBehavior
{
    public FlockBehavior[] Behaviors;
    public float[] Weights;
    public override Vector3 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        if (Weights.Length != Behaviors.Length)
        {
            Debug.LogError("Data mismathc in" + name, this);
            return Vector3.zero;
        }
        Vector3 move = Vector3.zero;
        for (int i = 0; i < Behaviors.Length; i++)
        {
            Vector3 partialMove = Behaviors[i].CalculateMove(agent, context, flock) * Weights[i];
            if (partialMove.sqrMagnitude > Weights[i] * Weights[i])
            {
                partialMove.Normalize();
                partialMove *= Weights[i];
            }
            move += partialMove;
        }
        return move;
    }
}
