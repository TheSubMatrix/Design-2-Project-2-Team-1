using UnityEngine;

public class Sanity : MonoBehaviour
{
    [SerializeField] uint m_currentSanity;
    [SerializeField] uint m_maxSanity;
    
    EventBinding<RequestSanityChange> m_healthEvent;
    void OnEnable()
    {
        m_healthEvent = new EventBinding<RequestSanityChange>(UpdateSanity);
        EventBus<RequestSanityChange>.Register(m_healthEvent);
    }
    void OnDisable()
    {
        EventBus<RequestSanityChange>.Deregister(m_healthEvent);
    }
    void Start()
    {
        m_currentSanity = 0;
        EventBus<BarUpdateEvent>.Raise(new BarUpdateEvent("Sanity", m_currentSanity / (float)m_maxSanity));
        EventBus<SanityUpdateEvent>.Raise(new SanityUpdateEvent(m_currentSanity, m_maxSanity));
    }
    void UpdateSanity(RequestSanityChange change)
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
        EventBus<SanityUpdateEvent>.Raise(new SanityUpdateEvent(m_currentSanity, m_maxSanity));
    }
}
