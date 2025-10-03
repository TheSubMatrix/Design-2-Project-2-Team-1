using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable, IHealable
{
    [field:SerializeField] public uint CurrentHealth { get; private set; }
    [field:SerializeField] public uint MaxHealth { get; private set; }
    public UnityEvent<uint, uint> OnHealthInitializedEvent = new();
    [Serializable]
    public class HealthEvent : UnityEvent<uint, uint> { }
    public HealthEvent OnDamagedEvent = new();
    public HealthEvent OnHealedEvent = new();
    public UnityEvent OnDeathEvent = new();
    public UnityEvent OnReviveEvent = new();
    public UnityEvent OnBecameInvulnerableEvent = new();
    public UnityEvent OnBecameVulnerableEvent = new();
    Coroutine m_invulnerabilityCoroutine;
    [SerializeField] bool m_invulnerabilityAfterDamage;
    [SerializeField] float m_invulnerabilityTime = 1;
    
    public bool IsAlive => CurrentHealth > 0;
    public bool IsInvulnerable { get; private set; }
    public bool IsHealable => CurrentHealth < MaxHealth;
    public bool IsDamageable => CurrentHealth > 0 && !IsInvulnerable;

    public void Awake()
    {
        CurrentHealth = MaxHealth;
        OnHealthInitializedEvent.Invoke(CurrentHealth, MaxHealth);
    }

    public void Damage(uint damage)
    {
        if (IsInvulnerable)
            return;
        uint oldHealth = CurrentHealth;
        bool currentAliveState = IsAlive;
        CurrentHealth -= CurrentHealth > damage ? damage : CurrentHealth;
        OnDamagedEvent.Invoke(oldHealth, CurrentHealth);
        if (currentAliveState != IsAlive)
        {
            OnDeathEvent.Invoke();
            return;
        }
        if (!m_invulnerabilityAfterDamage) return;
        StartCoroutine(InvulnerabilityForTimeAsync());
    }
    
    public void Heal(uint healAmount)
    {
        uint oldHealth = CurrentHealth;
        bool currentAliveState = IsAlive;
        CurrentHealth += CurrentHealth < MaxHealth ? healAmount : MaxHealth;
        OnHealedEvent.Invoke(oldHealth, CurrentHealth);
        if (currentAliveState != IsAlive)
        {
            OnReviveEvent.Invoke();
        }
    }

    public void MakeInvulnerable()
    {
        if (m_invulnerabilityCoroutine is not null)
        {
            StopCoroutine(m_invulnerabilityCoroutine);
        }
        IsInvulnerable = true;
        OnBecameInvulnerableEvent.Invoke();
    }

    public void MakeVulnerable()
    {
        if (m_invulnerabilityCoroutine is not null)
        {
            StopCoroutine(m_invulnerabilityCoroutine);
        }
        IsInvulnerable = false;
        OnBecameVulnerableEvent.Invoke();
    }

    IEnumerator InvulnerabilityForTimeAsync()
    {
        IsInvulnerable = true;
        OnBecameInvulnerableEvent.Invoke();
        yield return new WaitForSeconds(m_invulnerabilityTime);
        IsInvulnerable = false;
        OnBecameVulnerableEvent.Invoke();
    }
}
