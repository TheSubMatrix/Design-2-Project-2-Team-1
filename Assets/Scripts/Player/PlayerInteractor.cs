using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] float m_interactionDistance = 10f;
    [SerializeField] InputActionReference m_interactAction;
    [SerializeField] LayerMask m_interactableLayer;
    bool m_shouldReadInput = true;

    EventBinding<UpdatePlayerInputState> m_updateInputStateEvent;
    void OnEnable()
    {
        m_updateInputStateEvent = new EventBinding<UpdatePlayerInputState>(HandleInputReadChange);
        EventBus<UpdatePlayerInputState>.Register(m_updateInputStateEvent);
        m_interactAction.action.Enable();
        m_interactAction.action.started += OnInteract;
    }
    
    void OnDisable()
    {
        EventBus<UpdatePlayerInputState>.Deregister(m_updateInputStateEvent);
        m_interactAction.action.Disable();
        m_interactAction.action.started -= OnInteract;
    }
    void HandleInputReadChange(UpdatePlayerInputState state)
    {
        m_shouldReadInput = state.DesiredInputState;
    }
    
    void OnInteract(InputAction.CallbackContext context)
    {
        if (!m_shouldReadInput || !Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, m_interactionDistance, m_interactableLayer)) return;
        IInteractable interactable = hit.collider?.gameObject.GetComponent<IInteractable>();
        interactable?.OnStartedInteraction(this);
    }
}
