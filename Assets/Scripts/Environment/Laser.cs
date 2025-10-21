using System;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.VFX;

public class Laser : MonoBehaviour
{
    [SerializeField] uint m_damage = 10;
    [SerializeField] SoundData m_impactSound;
    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Rigidbody rb)) return;
        Vector3 direction = rb.transform.position - transform.position;
        Vector3 force = Vector3.Dot(direction, transform.forward) > 0 ? transform.forward : -transform.forward;
        rb.AddForce(force * 10, ForceMode.Impulse);
        if(!other.TryGetComponent(out IDamageable damageable)) return;
        damageable.Damage(m_damage);
        SoundManager.Instance.CreateSound().WithSoundData(m_impactSound).WithPosition(transform.position).WithRandomPitch().Play();
    }
    void DisableLaser()
    {
        gameObject.SetActive(false);
    }
}
