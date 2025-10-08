using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public delegate void OnProjectileShotCompleted(Projectile projectile, Collision collision);
    [SerializeField] float m_lifetime = 5f;
    OnProjectileShotCompleted m_onProjectileShotCompleted;
    Coroutine m_cleanupAfterTime;
    public Rigidbody Rigidbody { get; private set; }
    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
    public void Fire(OnProjectileShotCompleted onProjectileShotCompleted, Vector3 velocity)
    {
        m_onProjectileShotCompleted = onProjectileShotCompleted;
        Rigidbody.AddForce(velocity, ForceMode.VelocityChange);
       m_cleanupAfterTime = StartCoroutine(CleanupAfterTime());
    }

    void OnCollisionEnter(Collision other)
    {
        if (m_cleanupAfterTime is not null)
        {
            StopCoroutine(m_cleanupAfterTime);
            m_cleanupAfterTime = null;
        }
        m_onProjectileShotCompleted.Invoke(this, other);
    }

    IEnumerator CleanupAfterTime()
    {
        yield return new WaitForSeconds(m_lifetime);
        m_onProjectileShotCompleted.Invoke(this, null);
    }
}
