using UnityEngine;

public class SanityPickup : MonoBehaviour, IInteractable
{
    [SerializeField] uint m_healAmount = 10;
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        Sanity healable = interactor.transform.root.GetComponentInChildren<Sanity>();
        healable?.Heal(m_healAmount);
        Destroy(gameObject);
    }
}
