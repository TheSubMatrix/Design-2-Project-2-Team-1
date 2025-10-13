using UnityEngine;

public class HealthPickup : FloatingPickup
{
    [SerializeField] uint m_healAmount = 10;
    protected override void OnPickup(MonoBehaviour interactor)
    {
        IHealable healable = interactor.transform.root.GetComponentInChildren<IHealable>();
        healable?.Heal(m_healAmount);
        Destroy(gameObject);
    }

}
