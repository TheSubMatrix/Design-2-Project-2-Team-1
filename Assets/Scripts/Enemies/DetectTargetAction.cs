using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detect Target", story: "[Agent] detects [Target] with [Tag] tag", category: "Action/Sensor", id: "182fe09b443c80ef00967c3991f7ab1b")]
public partial class DetectTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<string> Tag;

    NavMeshAgent m_agent;
    EnemyVisionSensor m_sensor;
    
    
    protected override Status OnStart()
    {
        m_agent = Agent.Value.GetComponent<NavMeshAgent>();
        m_sensor = Agent.Value.GetComponentInChildren<EnemyVisionSensor>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Transform target = m_sensor.GetClosestTarget(Tag.Value);
        if (target is null) return Status.Running;
        Target.Value = target.gameObject;
        return Status.Success;
    }
}

