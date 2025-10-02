using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeReference, SubclassList(typeof(BaseCombatAction))]
    BaseCombatAction m_attackCombatAction;
    [SerializeReference, SubclassList(typeof(BaseCombatAction))]
    BaseCombatAction m_heavyAttackCombatAction;
    [SerializeReference, SubclassList(typeof(BaseCombatAction))]
    BaseCombatAction m_attackCombatActionBerserk;
    [SerializeReference, SubclassList(typeof(BaseCombatAction))]
    BaseCombatAction m_heavyAttackCombatActionBerserk;

    void Start()
    {
        m_attackCombatAction.InitializeCombatAction();
        m_heavyAttackCombatAction.InitializeCombatAction();
        m_attackCombatActionBerserk.InitializeCombatAction();
        m_heavyAttackCombatActionBerserk.InitializeCombatAction();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            m_attackCombatAction.StartCombatAction(this);
        }

        if (!Input.GetMouseButtonDown(1))
        {
            m_heavyAttackCombatAction.StartCombatAction(this);
        }
    }
}
