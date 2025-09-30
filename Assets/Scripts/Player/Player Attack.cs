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

    void Start()
    {
        m_attackCombatAction.InitializeCombatAction();
        m_heavyAttackCombatAction.InitializeCombatAction();
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
