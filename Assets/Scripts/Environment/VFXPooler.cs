using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXPooler : PersistentSingleton<VFXPooler>
{
    [SerializeField]internal SerializableDictionary<VisualEffectAsset, VFXPool> ParticlePools = new();
    public VFXBuilder CreateVFX => new VFXBuilder(this);
    protected override void InitializeSingleton()
    {
        base.InitializeSingleton();
        foreach (KeyValuePair<VisualEffectAsset, VFXPool> kvp in ParticlePools.Dictionary)
        {
            kvp.Value.InitializePool(kvp.Key);
        }
    }

    public void ReturnVFXAfterDelay(VisualEffect vfx, VisualEffectAsset asset, float delay)
    {
        StartCoroutine(ReturnVFXCoroutine(vfx, asset, delay));
    }

    System.Collections.IEnumerator ReturnVFXCoroutine(VisualEffect vfx, VisualEffectAsset asset, float delay)
    {
        yield return new WaitForSeconds(delay);
    
        if (!vfx || !vfx.gameObject.activeInHierarchy)
        {
            yield break;
        }

        // Stop the effect if it's still playing
        vfx.Stop();

        // Clear the follower target
        VFXFollower follower = vfx.GetComponent<VFXFollower>();
        if (follower)
        {
            follower.ClearTarget();
        }

        // Return to pool
        if (ParticlePools.Dictionary.TryGetValue(asset, out VFXPool pool))
        {
            pool.Pool.Release(vfx);
        }
    }
    
    public void StopLoopingVFX(VisualEffect vfx, VisualEffectAsset asset)
    {
        if (vfx == null) return;

        vfx.Stop();

        VFXFollower follower = vfx.GetComponent<VFXFollower>();
        if (follower != null)
        {
            follower.ClearTarget();
        }

        if (ParticlePools.Dictionary.TryGetValue(asset, out VFXPool pool))
        {
            pool.Pool.Release(vfx);
        }
    }
}