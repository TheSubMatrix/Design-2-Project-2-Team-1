using System.Collections;
using AudioSystem;
using UnityEngine;
using UnityEngine.Events;

public class RaycastShotFireAction : BaseCombatAction
{
    [SerializeField] Transform m_raycastOrigin;
    [SerializeField] float m_raycastDistance = 10f;
    [SerializeField] LayerMask m_raycastLayerMask;
    [SerializeField] SoundData m_shotSound;
    [SerializeField] Vector2 m_spread = new Vector2(0f, 0f);
    [SerializeField] UnityEvent m_onShotFired = new();
    [SerializeField] uint m_damage = 10;
    public override void InitializeCombatAction()
    {
        
    }

    protected override IEnumerator ExecuteCombatActionAsyncImplementation()
    {
        SoundManager.Instance.CreateSound().WithSoundData(m_shotSound).WithPosition(m_raycastOrigin.position).WithRandomPitch().Play();
        if (!Physics.Raycast(m_raycastOrigin.position, Quaternion.AngleAxis(Random.Range(-m_spread.x, m_spread.x), m_raycastOrigin.up) * Quaternion.AngleAxis(Random.Range(-m_spread.y, m_spread.y), m_raycastOrigin.right) * m_raycastOrigin.forward, out RaycastHit hit, m_raycastDistance, m_raycastLayerMask)) yield break;
        m_onShotFired?.Invoke();
        if (!hit.collider.TryGetComponent(out IDamageable damageable)) yield break;
        damageable.Damage(m_damage);
        yield return null;
    }
}
