using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Random Point In Range", story: "Find random [point] around [location] with [range] ", category: "Action/Navigation", id: "2e923b5bf5f7fe7742704636e6f41c02")]
public partial class FindRandomPointInRangeAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Location;
    [SerializeReference] public BlackboardVariable<Vector3> Point;
    [SerializeReference] public BlackboardVariable<float> Range;

    protected override Status OnUpdate()
    {
        Point.Value = RandomNavSphere(Location, Range.Value, ~0);
        return Status.Success;
    }
 
    public Vector3 RandomNavSphere(Vector3 origin, float distance, int layerMask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, distance, layerMask);
        return navHit.position;
    }
}

