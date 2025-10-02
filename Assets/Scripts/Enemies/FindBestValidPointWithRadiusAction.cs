using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Best Valid Point With Radius",
    story: "Find nearest valid [Point] around [Position] with [Radius]", category: "Action/Navigation",
    id: "199d47df04afa89246b381cccdd827d6")]
public partial class FindBestValidPointWithRadiusAction : Action
{
    const uint MaxIterations = 16;
    [SerializeReference] public BlackboardVariable<Vector3> Point;
    [SerializeReference] public BlackboardVariable<Vector3> Position;
    [SerializeReference] public BlackboardVariable<float> Radius;
    NavMeshAgent m_navMeshAgent;
    List<Vector3> m_searchPositions = new();

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!NavMesh.SamplePosition(Position.Value, out NavMeshHit hit, 5, NavMesh.AllAreas)) return Status.Failure;
        Vector3 searchCenter = hit.position;
        m_searchPositions.Clear();
        for (uint i = 0; i < MaxIterations; i++)
        {
            m_searchPositions.Add(new Vector3(
                searchCenter.x + (Radius.Value * MathF.Cos(i * (2 * MathF.PI / MaxIterations))),
                searchCenter.y,
                searchCenter.z + (Radius.Value * MathF.Sin(i * (2 * MathF.PI / MaxIterations)))));
        }
        
        Vector3 closestPoint = Vector3.positiveInfinity;
        foreach (Vector3 position in m_searchPositions.ToList())
        {
            if (NavMesh.Raycast(searchCenter, position, out NavMeshHit meshHit, NavMesh.AllAreas))
            {
                m_searchPositions.Remove(position);
            }
        }
        Point.Value = m_searchPositions.OrderBy(p => Vector3.Distance(p, Position.Value)).First();
        return Status.Success;
    }
}

