using System.Collections;
using AudioSystem;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class FireDamageProjectileCombatAction : BaseCombatAction
{
    IObjectPool<Projectile> m_projectilePool;
    [SerializeField] Projectile m_projectilePrefab;
    [SerializeField] Transform m_projectileSpawnPoint;
    [SerializeField] SoundData m_projectileShotSound;
    [SerializeField] SoundData m_projectileHitSound;
    
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
        try
        {
            projectile.Rigidbody.linearVelocity = Vector3.zero;
            m_projectilePool.Release(projectile);
        }
        catch
        {
            //No-Op
            //Catches any problems releasing the projectile from the pool
        }

        if (other == null) return;
        if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.Damage(10);
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
