using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detect No Targets", story: "[Agent] detects no [Target] with [Tag] tag", category: "Action/Sensor", id: "4d432581b398c17df86ab2ebc5fece75")]
public partial class DetectNoTargetsAction : Action
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
        Target.Value = target?.gameObject;
        return target is null ? Status.Success : Status.Running;
    }
}



