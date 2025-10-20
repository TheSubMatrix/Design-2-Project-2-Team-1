using UnityEngine;

public class EnemyCombatActionHandler : MonoBehaviour
{
    [SerializeReference, ClassSelector]
    BaseCombatAction m_attackCombatAction;
    [SerializeReference, ClassSelector]
    BaseCombatAction m_secondaryAttackCombatAction;

    public bool IsAttacking => (m_attackCombatAction is not null && m_attackCombatAction.IsExecuting) ||
                               (m_secondaryAttackCombatAction is not null && m_secondaryAttackCombatAction.IsExecuting);

    public void Awake()
    {
        m_attackCombatAction?.InitializeCombatAction();
        m_secondaryAttackCombatAction?.InitializeCombatAction();
    }
    public void ExecutePrimaryAttack()
    {
        m_attackCombatAction?.StartCombatAction(this);
    }
    public void ExecuteSecondaryAttack()
    {
        m_secondaryAttackCombatAction?.StartCombatAction(this);
    }
}
