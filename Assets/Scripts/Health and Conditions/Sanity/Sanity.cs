using UnityEngine;

public class Sanity : MonoBehaviour
{
    [SerializeField] uint m_currentSanity;
    [SerializeField] uint m_maxSanity;

    EventBinding<SanityChange> m_healthEvent;
    void OnEnable()
    {
        m_healthEvent = new EventBinding<SanityChange>(UpdateSanity);
        EventBus<SanityChange>.Register(m_healthEvent);
    }
    void OnDisable()
    {
        EventBus<SanityChange>.Deregister(m_healthEvent);
    }
    void Start()
    {
        m_currentSanity = m_maxSanity;
    }
    void UpdateSanity(SanityChange change)
    {
        if (change.SanityDifference > 0)
        {
            m_currentSanity = m_currentSanity + change.SanityDifference > m_maxSanity ? m_currentSanity = m_maxSanity : m_currentSanity + (uint)change.SanityDifference;
        }
        else
        {
            m_currentSanity = (int)m_currentSanity - change.SanityDifference < 0 ? m_currentSanity = 0 : m_currentSanity - (uint)(-change.SanityDifference);
        }
        EventBus<BarUpdateEvent>.Raise(new BarUpdateEvent("Sanity", m_currentSanity / (float)m_maxSanity));
    }
}
