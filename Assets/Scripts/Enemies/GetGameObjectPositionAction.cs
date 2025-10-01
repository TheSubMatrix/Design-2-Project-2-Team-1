using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get GameObject Position", story: "Get [Position] of [Object]", category: "Action/GameObject", id: "9d81098ab7a6bc872494314994825483")]
public partial class GetGameObjectPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Position;
    [SerializeReference] public BlackboardVariable<GameObject> Object;

    protected override Status OnStart()
    {
        if (Object?.Value == null)
        {
            return Status.Failure;
        }
        Position.Value = Object.Value.transform.position;
        return Status.Running;
    }
}

