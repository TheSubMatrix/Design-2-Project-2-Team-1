using System;
using System.Collections;
using UnityEngine;
[Serializable]
public class SimpleAttackCombatAction : BaseCombatAction
{
    [SerializeField] TriggerEventCallbacks m_triggerEventCallbacks;
    [SerializeField] Animator m_animator;
    [SerializeField] string m_attackTriggerName = "Attack";
    [SerializeField] AnimatorOverrideController m_attackAnimationOverride;
    [SerializeField] uint m_damage;
    public override void InitializeCombatAction()
    {
        if (m_triggerEventCallbacks is null)
        {
            Debug.LogError("TriggerEventCallbacks is null on Simple Combat Action");
            return;
        }
        m_triggerEventCallbacks.OnTriggerEnterEvent.m_shouldBeInvoked = true;
        m_triggerEventCallbacks.OnTriggerEnterEvent.AddListener(TriggerEnter);
    }

    protected override IEnumerator ExecuteCombatActionAsyncImplementation()
    {
         m_animator?.SetTrigger(m_attackTriggerName);
         m_triggerEventCallbacks.ReferencedCollider.enabled = true;
         yield return new WaitForSeconds(Duration);
         m_triggerEventCallbacks.ReferencedCollider.enabled = false;
    }

    public override void CancelCombatActionImplementation()
    {
        m_triggerEventCallbacks.ReferencedCollider.enabled = false;
    }

    void TriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        damageable?.Damage(m_damage);
    }
}
