using UnityEngine;

public class HealthBarEventDriver : MonoBehaviour
{
    [SerializeField] string m_barName = "Health";
    uint m_maxHealth;
    public void OnHealthInitialized(uint currentHealth, uint maxHealth)
    {
        m_maxHealth = maxHealth;
        EventBus<BarUpdateEvent>.Raise(new BarUpdateEvent(m_barName, currentHealth / (float)m_maxHealth));
    }
    public void OnHealthUpdated(uint oldHealth, uint newHealth)
    {
        EventBus<BarUpdateEvent>.Raise(new BarUpdateEvent(m_barName, newHealth / (float)m_maxHealth));
    }
}
