using System.Collections;
using AudioSystem;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class FireSlowProjectileCombatAction : BaseCombatAction
{
    IObjectPool<Projectile> m_projectilePool;
    [SerializeField] Projectile m_projectilePrefab;
    [SerializeField] Transform m_projectileSpawnPoint;
    [SerializeField] SoundData m_projectileShotSound;
    [SerializeField] SoundData m_projectileHitSound;
    [SerializeField] float m_slowPercentage = 0.5f;
    [SerializeField] float m_slowDuration = 5f;
    public override void InitializeCombatAction()
    {
        m_projectilePool = new ObjectPool<Projectile>(CreateProjectile, OnTakeFromPool, OnReturnedToPool, OnDestroyPooledObject, true, 10, 100);
    }

    Projectile CreateProjectile()
    {
        Projectile newProjectile = Object.Instantiate(m_projectilePrefab, SceneManager.GetActiveScene()) as Projectile;
        newProjectile?.gameObject.SetActive(false);
        return newProjectile;
    }
    void OnTakeFromPool(Projectile projectile)
    {
        projectile.transform.position = m_projectileSpawnPoint.position;
        projectile.gameObject.SetActive(true);
    }
    void OnReturnedToPool(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    void OnDestroyPooledObject(Projectile projectile)
    {
        Object.Destroy(projectile.gameObject);
    }

    void ProjectileShotCompleted(Projectile projectile, Collision other)
    {
        projectile.Rigidbody.linearVelocity = Vector3.zero;
        m_projectilePool.Release(projectile);
        
        if (other?.contacts[0] is null) return;
        if (other.gameObject.TryGetComponent<ISlowable>(out ISlowable slowable))
        {
            slowable.Slow(m_slowPercentage, m_slowDuration);
        }
        SoundManager.Instance.CreateSound().WithSoundData(m_projectileHitSound).WithPosition(other.contacts[0].point).WithRandomPitch().Play();
    }
    
    protected override IEnumerator ExecuteCombatActionAsyncImplementation()
    {
        Projectile projectile = m_projectilePool.Get();
        SoundManager.Instance.CreateSound().WithSoundData(m_projectileShotSound).WithPosition(m_projectileSpawnPoint.position).WithRandomPitch().Play();
        projectile.Fire(ProjectileShotCompleted, m_projectileSpawnPoint.forward * 15);
        yield return null;
    }
}
