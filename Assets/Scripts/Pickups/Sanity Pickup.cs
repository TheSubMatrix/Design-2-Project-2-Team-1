using UnityEngine;

public class SanityPickup : FloatingPickup
{
    [SerializeField] int m_sanityChange = 10;
    protected override void OnPickup(MonoBehaviour interactor)
    {
        EventBus<RequestSanityChange>.Raise(new RequestSanityChange(m_sanityChange));
        Destroy(gameObject);
    }
}
