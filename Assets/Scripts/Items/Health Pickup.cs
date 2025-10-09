using UnityEngine;

public class HealthPickup : MonoBehaviour, IInteractable
{
    [SerializeField] uint m_healAmount = 10;
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        if (interactor.transform.root.TryGetComponent(out IHealable healable))
        {
            healable.Heal(m_healAmount);
        }
    }
    public void OnExitedInteraction()
    {
        
    }
}
