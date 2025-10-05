using UnityEngine;
using UnityEngine.UI;

public class BarHandler : MonoBehaviour
{
    [SerializeField] Image m_barFill;
    EventBinding<BarUpdateEvent> m_barUpdateEvent;
    [field:SerializeField] public string BarName {get; private set;}

    void OnEnable()
    {
        m_barUpdateEvent = new EventBinding<BarUpdateEvent>(OnBarUpdate);
        EventBus<BarUpdateEvent>.Register(m_barUpdateEvent);
    }
    void OnDisable()
    {
        EventBus<BarUpdateEvent>.Deregister(m_barUpdateEvent);
    }

    void OnBarUpdate(BarUpdateEvent e)
    {
        if (e.BarName != BarName || m_barFill is null) return;
        m_barFill.fillAmount = e.BarPercentage;
    }
}
