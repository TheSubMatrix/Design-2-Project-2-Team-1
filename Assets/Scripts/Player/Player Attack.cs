using UnityEngine;
using UnityEngine.InputSystem;


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

    [SerializeField] InputActionReference m_lightAttack;
    [SerializeField] InputActionReference m_heavyAttack;
    bool m_berserk;
    bool IsAttacking => (m_attackCombatAction is not null && m_attackCombatAction.IsExecuting) ||
                        (m_heavyAttackCombatAction is not null && m_heavyAttackCombatAction.IsExecuting) ||
                        (m_attackCombatActionBerserk is not null && m_attackCombatActionBerserk.IsExecuting) ||
                        (m_heavyAttackCombatActionBerserk is not null && m_heavyAttackCombatActionBerserk.IsExecuting);
    void OnEnable()
    {
        m_lightAttack.action.Enable();
        m_heavyAttack.action.Enable();
        m_lightAttack.action.performed += LightAttack;
        m_heavyAttack.action.performed += HeavyAttack;
    }

    void OnDisable()
    {
        m_lightAttack.action.Disable();
        m_heavyAttack.action.Disable();
        m_lightAttack.action.performed -= LightAttack;
        m_heavyAttack.action.performed -= HeavyAttack;
    }

    void Awake()
    {
        m_attackCombatAction?.InitializeCombatAction();
        m_heavyAttackCombatAction?.InitializeCombatAction();
        m_attackCombatActionBerserk?.InitializeCombatAction();
        m_heavyAttackCombatActionBerserk?.InitializeCombatAction();
    }

    void LightAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        if (m_berserk)
        {
            m_attackCombatActionBerserk.StartCombatAction(this);
            return;
        }
        m_attackCombatAction.StartCombatAction(this);
    }
    void HeavyAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        if (m_berserk)
        {
            m_heavyAttackCombatActionBerserk.StartCombatAction(this);
            return;
        }
        m_heavyAttackCombatAction.StartCombatAction(this);
    }
}
