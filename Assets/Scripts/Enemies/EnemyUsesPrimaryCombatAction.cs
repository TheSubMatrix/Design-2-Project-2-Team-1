using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Use Primary Combat Action", story: "[Agent] uses primary combat action", category: "Action/Combat Actions", id: "124ba1cb3088c40d5ca8929b63fb385f")]
public partial class EnemyUsesPrimaryCombatAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    EnemyCombatActionHandler m_combatActionHandler;
    protected override Status OnStart()
    {
        m_combatActionHandler = Agent.Value.GetComponent<EnemyCombatActionHandler>();
        return (m_combatActionHandler is null) ? Status.Failure : Status.Running;
    }

    protected override Status OnUpdate()
    {
        if(m_combatActionHandler.IsAttacking) return Status.Running;
        m_combatActionHandler.ExecutePrimaryAttack();
        return Status.Success;
    }
}

