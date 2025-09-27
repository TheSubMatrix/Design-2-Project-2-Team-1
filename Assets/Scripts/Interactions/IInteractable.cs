using UnityEngine;

public interface IInteractable
{
    void OnStartedInteraction(MonoBehaviour interactor);
    void OnExitedInteraction();
}
