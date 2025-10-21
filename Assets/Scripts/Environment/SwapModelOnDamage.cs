using AudioSystem;
using UnityEngine;
using UnityEngine.Events;

public class SwapModelOnDamage : MonoBehaviour, IDamageable
{
    [SerializeField] GameObject m_modelToSwapFrom;
    [SerializeField] GameObject m_modelToSwapTo;
    [SerializeField] SoundData m_swapSound;
    [SerializeField] UnityEvent m_onSwapComplete;
    bool m_hasSwapped = false;
    public void Damage(uint damageToApply)
    {
        if (m_hasSwapped) return;
        m_modelToSwapFrom.SetActive(false);
        m_modelToSwapTo.SetActive(true);
        SoundManager.Instance.CreateSound().WithSoundData(m_swapSound).WithPosition(transform.position).WithRandomPitch().Play();
        m_onSwapComplete.Invoke();
        m_hasSwapped = true;
    }
}