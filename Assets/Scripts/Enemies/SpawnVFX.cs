using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public class SpawnVFX : MonoBehaviour
{
    [FormerlySerializedAs("asset")] [SerializeField]VisualEffectAsset m_asset;
    
    public void OnSpawnVFX()
    {
        VFXPooler.Instance.CreateVFX.WithAsset(m_asset).AtPosition(transform.position).Play();
    }
}
