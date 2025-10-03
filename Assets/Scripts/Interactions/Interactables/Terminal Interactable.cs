using Unity.Cinemachine;
using UnityEngine;

public class TerminalInteractable : MonoBehaviour, IInteractable
{
    CinemachineCamera m_previousCamera;
    [SerializeField] CinemachineCamera m_interactionCamera;
    bool m_isInteracting = false;
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        m_previousCamera = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera as CinemachineCamera;
        if(m_interactionCamera is null) return;
        EventBus<PlayerCameraBlendHandler.CameraBlendData>.Raise(new PlayerCameraBlendHandler.CameraBlendData(m_interactionCamera));
        EventBus<PlayerMovement.UpdatePlayerInputState>.Raise(new PlayerMovement.UpdatePlayerInputState(false));
        m_isInteracting = true;
    }

    void Update()
    {
        if (!m_isInteracting || !Input.GetKeyDown(KeyCode.Escape)){ return; }
        OnExitedInteraction();
    }
    public void OnExitedInteraction()
    {
        EventBus<PlayerCameraBlendHandler.CameraBlendData>.Raise(new PlayerCameraBlendHandler.CameraBlendData(m_previousCamera));
        EventBus<PlayerMovement.UpdatePlayerInputState>.Raise(new PlayerMovement.UpdatePlayerInputState(true));
        m_isInteracting = false;
    }
    
}
