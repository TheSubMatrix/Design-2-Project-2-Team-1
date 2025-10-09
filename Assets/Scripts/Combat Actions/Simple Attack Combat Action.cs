using System;
using System.Collections;
using AudioSystem;
using UnityEngine;
using UnityEngine.Serialization;
[Serializable]
public class SimpleAttackCombatAction : BaseCombatAction
{
    [Header("Right Hand")]
    [SerializeField] bool m_useRightHand;
    [FormerlySerializedAs("m_animator"),SerializeField] Animator m_animatorRight;
    [SerializeField] AnimatorOverrideController m_rightHandOverrides;
    [Header("Left Hand")]
    [SerializeField] bool m_useLeftHand;
    [SerializeField] Animator m_animatorLeft;
    [SerializeField] AnimatorOverrideController m_leftHandOverrides;
    [Header("Other")]
    [SerializeField] TriggerEventCallbacks m_triggerEventCallbacks;
    [SerializeField] string m_attackTriggerName = "Attack";
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
        if (m_useRightHand && m_animatorRight is not null)
        {
            if (m_rightHandOverrides is not null)
            {
                m_animatorRight.runtimeAnimatorController = m_rightHandOverrides;
            }
            m_animatorRight.SetTrigger(m_attackTriggerName);
        }
        if (m_useLeftHand && m_animatorLeft is not null)
        {
            if (m_leftHandOverrides is not null)
            {
                m_animatorLeft.runtimeAnimatorController = m_leftHandOverrides;
            }
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
