using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class TerminalInteractable : MonoBehaviour, IInteractable
{
    CinemachineCamera m_previousCamera;
    [SerializeField] CinemachineCamera m_interactionCamera;
    [SerializeField] InputSystemUIInputModule m_inputModule;
    [SerializeField] InputActionReference m_exitAction;

    void OnEnable()
    {
        m_exitAction.action.Enable();
        m_exitAction.action.performed += ExitInteractionViaInput;
    }
    void OnDisable()
    {
        m_exitAction.action.Disable();
        m_exitAction.action.performed -= ExitInteractionViaInput;
    }

    void ExitInteractionViaInput(InputAction.CallbackContext context)
    {
        OnExitedInteraction();
    }
    
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        m_previousCamera = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera as CinemachineCamera;
        if(m_interactionCamera is null) return;
        EventBus<CameraBlendData>.Raise(new CameraBlendData(m_interactionCamera));
        EventBus<UpdatePlayerInputState>.Raise(new UpdatePlayerInputState(false));
        m_inputModule.enabled = true;
    }
    public void OnExitedInteraction()
    {
        EventBus<CameraBlendData>.Raise(new CameraBlendData(m_previousCamera));
        EventBus<UpdatePlayerInputState>.Raise(new UpdatePlayerInputState(true));
        m_inputModule.enabled = false;
    }
    
}
