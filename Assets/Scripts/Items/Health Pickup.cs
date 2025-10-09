using UnityEngine;

public class HealthPickup : MonoBehaviour, IInteractable
{
    [SerializeField] uint m_healAmount = 10;
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        IHealable healable = interactor.transform.root.GetComponentInChildren<IHealable>();
        healable?.Heal(m_healAmount);
    }
    public void OnExitedInteraction()
    {
        
    }
}
