using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

[Serializable]
internal class VFXPool
{
    VFXPool()
    {
    }

    public static VFXPool Instance { get; } = new();
    public IObjectPool<VisualEffect> Pool;
    public bool CollectionCheck = true;
    public int MaxPoolSize = 100;
    public int InitialPoolSize = 10;
        
    public VisualEffect CreateVFXObjectForPool(VisualEffectAsset asset)
    {
        VisualEffect newVisualEffect = new GameObject().AddComponent<VisualEffect>();
        newVisualEffect.visualEffectAsset = asset;
        return newVisualEffect;
    }

    public void InitializePool(VisualEffectAsset asset)
    {
        Pool = new ObjectPool<VisualEffect>(
            () => CreateVFXObjectForPool(asset), 
            OnVFXGet, 
            OnVFXReturn, 
            OnVFXDestroy, 
            CollectionCheck, 
            InitialPoolSize, 
            MaxPoolSize);
    }
        
    void OnVFXGet(VisualEffect vfx)
    {
        vfx.Stop();
        vfx.gameObject.SetActive(true);
    }
        
    void OnVFXReturn(VisualEffect vfx)
    {
        vfx.gameObject.SetActive(false);
        vfx.Play();
    }
        
    void OnVFXDestroy(VisualEffect vfx)
    {
        Object.Destroy(vfx.gameObject);
    }
}