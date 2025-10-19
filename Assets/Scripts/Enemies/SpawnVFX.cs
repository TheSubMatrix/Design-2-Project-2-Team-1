using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public class SpawnVFX : MonoBehaviour
{
    [FormerlySerializedAs("asset")] [SerializeField]VisualEffectAsset m_asset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VFXPooler.Instance.CreateVFX.WithAsset(m_asset).AtPosition(transform.position).Play();
    }

}
