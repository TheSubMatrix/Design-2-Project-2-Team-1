using UnityEngine;

public class SanityPickup : MonoBehaviour, IInteractable
{
    [SerializeField] int m_sanityChange = 10;
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        EventBus<RequestSanityChange>.Raise(new RequestSanityChange(m_sanityChange));
    }
}
