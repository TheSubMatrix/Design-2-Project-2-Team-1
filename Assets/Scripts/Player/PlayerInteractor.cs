using System;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] LayerMask m_interactableLayer;
    bool m_shouldReadInput = true;

    EventBinding<PlayerMovement.UpdatePlayerInputState> m_updateInputStateEvent;
    void OnEnable()
    {
        m_updateInputStateEvent = new EventBinding<PlayerMovement.UpdatePlayerInputState>(HandleInputReadChange);
        EventBus<PlayerMovement.UpdatePlayerInputState>.Register(m_updateInputStateEvent);
    }
    
    void OnDisable()
    {
        EventBus<PlayerMovement.UpdatePlayerInputState>.Deregister(m_updateInputStateEvent);
    }
    void HandleInputReadChange(PlayerMovement.UpdatePlayerInputState state)
    {
        m_shouldReadInput = state.DesiredInputState;
    }
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E) || !m_shouldReadInput) return;
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100f, m_interactableLayer)) return;
        IInteractable interactable = hit.collider?.gameObject.GetComponent<IInteractable>();
        interactable?.OnStartedInteraction(this);
    }
}
