using System;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if(!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100f)) return;
        IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
        interactable?.OnStartedInteraction(this);
    }
}
