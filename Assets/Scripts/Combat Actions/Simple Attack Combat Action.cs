using System;
using System.Collections;
using AudioSystem;
using UnityEngine;
using UnityEngine.Serialization;
[Serializable]
public class SimpleAttackCombatAction : BaseCombatAction
{
    [SerializeField] TriggerEventCallbacks m_triggerEventCallbacks;
    [FormerlySerializedAs("m_animator")]
    [SerializeField] Animator m_animatorRight;
    [SerializeField] bool m_useRightHand;
    [SerializeField] Animator m_animatorLeft;
    [SerializeField] bool m_useLeftHand;
    [SerializeField] string m_attackTriggerName = "Attack";
    [FormerlySerializedAs("m_attackAnimationOverride")]

    [SerializeField] uint m_damage;
    [SerializeField] SoundData m_attackSound;
    public override void InitializeCombatAction()
    {
        if (m_triggerEventCallbacks is null)
        {
            Debug.LogError("TriggerEventCallbacks is null on Simple Combat Action");
            return;
        }
        if (m_animatorRight is not null && m_animatorLeft is not null)
        {
            
        }
        m_triggerEventCallbacks.OnTriggerEnterEvent.m_shouldBeInvoked = true;
        m_triggerEventCallbacks.OnTriggerEnterEvent.AddListener(TriggerEnter);
    }

    protected override IEnumerator ExecuteCombatActionAsyncImplementation()
    {
        if (m_useRightHand)
        {
            if (m_animatorRight is null) yield break;
            //m_animatorRight.runtimeAnimatorController = m_attackAnimationOverrideRight;
            m_animatorRight.SetTrigger(m_attackTriggerName);
        }
        if (m_useLeftHand)
        {
            if(m_animatorLeft is null) yield break;
            //m_animatorLeft.runtimeAnimatorController = m_attackAnimationOverrideLeft;
            m_animatorLeft.SetTrigger(m_attackTriggerName);
        }
        SoundManager.Instance.CreateSound().WithSoundData(m_attackSound).WithPosition(m_triggerEventCallbacks.ReferencedCollider.transform.position).WithRandomPitch().Play();
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
