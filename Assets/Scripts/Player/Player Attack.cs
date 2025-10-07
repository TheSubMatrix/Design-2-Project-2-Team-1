using System.Collections;
using AudioSystem;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerAttack : MonoBehaviour
{
    [Header("Attacks")]
    [SerializeReference, SubclassList(typeof(BaseCombatAction))]
    BaseCombatAction m_attackCombatAction;
    [SerializeReference, SubclassList(typeof(BaseCombatAction))]
    BaseCombatAction m_heavyAttackCombatAction;
    [SerializeField] InputActionReference m_lightAttack;
    [SerializeField] InputActionReference m_heavyAttack;
    [Header("Berserk")]
    [SerializeReference, SubclassList(typeof(BaseCombatAction))]
    BaseCombatAction m_attackCombatActionBerserk;
    [SerializeReference, SubclassList(typeof(BaseCombatAction))]
    BaseCombatAction m_heavyAttackCombatActionBerserk;    
    [SerializeField] SoundData m_berserkSound;
    [SerializeField] SoundData m_berserkStopSound;
    [SerializeField] uint m_sanityLostPerSecond;
    [SerializeField] InputActionReference m_toggleBerserkAction;
    
    EventBinding<SanityUpdateEvent> m_sanityUpdateEvent;
    Coroutine m_berserkCoroutine;
    bool m_berserk;
    uint m_trackedSanity;
    uint m_trackedMaxSanity;
    
    
    bool IsAttacking => (m_attackCombatAction is not null && m_attackCombatAction.IsExecuting) ||
                        (m_heavyAttackCombatAction is not null && m_heavyAttackCombatAction.IsExecuting) ||
                        (m_attackCombatActionBerserk is not null && m_attackCombatActionBerserk.IsExecuting) ||
                        (m_heavyAttackCombatActionBerserk is not null && m_heavyAttackCombatActionBerserk.IsExecuting);
    void OnEnable()
    {
        m_lightAttack.action.Enable();
        m_heavyAttack.action.Enable();
        m_toggleBerserkAction.action.Enable();
        m_lightAttack.action.started += LightAttack;
        m_heavyAttack.action.started += HeavyAttack;
        m_toggleBerserkAction.action.started += OnToggleBerserk;
        m_sanityUpdateEvent = new EventBinding<SanityUpdateEvent>(OnSanityUpdate);
        EventBus<SanityUpdateEvent>.Register(m_sanityUpdateEvent);
    }
    
    void OnDisable()
    {
        m_lightAttack.action.Disable();
        m_heavyAttack.action.Disable();
        m_toggleBerserkAction.action.Disable();
        m_lightAttack.action.started -= LightAttack;
        m_heavyAttack.action.started -= HeavyAttack;
        m_toggleBerserkAction.action.started -= OnToggleBerserk;
        EventBus<SanityUpdateEvent>.Deregister(m_sanityUpdateEvent);
    }

    void Awake()
    {
        m_attackCombatAction?.InitializeCombatAction();
        m_heavyAttackCombatAction?.InitializeCombatAction();
        m_attackCombatActionBerserk?.InitializeCombatAction();
        m_heavyAttackCombatActionBerserk?.InitializeCombatAction();
    }
    
    void OnSanityUpdate(SanityUpdateEvent e)
    {
        m_trackedSanity = e.Sanity;
        m_trackedMaxSanity = e.SanityMax;
    }
    
    void LightAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        if (m_berserk)
        {
            m_attackCombatActionBerserk?.StartCombatAction(this);
        }
        else
        {
            m_attackCombatAction?.StartCombatAction(this);
        }
    }
    
    void HeavyAttack(InputAction.CallbackContext context)
    {
        if (IsAttacking) return;
        if (m_berserk)
        {
            m_heavyAttackCombatActionBerserk?.StartCombatAction(this);
        }
        else
        {
            m_heavyAttackCombatAction?.StartCombatAction(this);
        }
    }

    void OnToggleBerserk(InputAction.CallbackContext context)
    {
        if (m_berserkCoroutine is not null)
        {
            StopCoroutine(m_berserkCoroutine);
            SoundManager.Instance.CreateSound().WithSoundData(m_berserkStopSound).WithPosition(transform.position).WithRandomPitch().Play();
        }
        else
        {
            m_berserkCoroutine = StartCoroutine(BerserkAsync());
        }
    }

    IEnumerator BerserkAsync()
    {
        if(m_trackedSanity <= 0) yield break;
        m_berserk = true;
        SoundManager.Instance.CreateSound().WithSoundData(m_berserkSound).WithPosition(transform.position).WithRandomPitch().Play();
        while (m_trackedSanity > 0)
        {
            EventBus<RequestSanityChange>.Raise(new RequestSanityChange(-1));
            yield return new WaitForSeconds(1.0f / m_sanityLostPerSecond);
        }
        m_berserk = false;
        SoundManager.Instance.CreateSound().WithSoundData(m_berserkStopSound).WithPosition(transform.position).WithRandomPitch().Play();
    }
}
