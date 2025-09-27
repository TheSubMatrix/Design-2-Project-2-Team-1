using Unity.Cinemachine;
using UnityEngine;

public class TerminalInteractable : MonoBehaviour, IInteractable
{
    CinemachineCamera m_previousCamera;
    [SerializeField] CinemachineCamera m_interactionCamera;
    public void OnStartedInteraction(MonoBehaviour interactor)
    {
        m_previousCamera = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera as CinemachineCamera;
        if(m_interactionCamera is null) return;
        EventBus<PlayerCameraBlendHandler.CameraBlendData>.Raise(new PlayerCameraBlendHandler.CameraBlendData(m_interactionCamera));
    }

    public void OnExitedInteraction()
    {
        if(m_previousCamera is null) return;
        EventBus<PlayerCameraBlendHandler.CameraBlendData>.Raise(new PlayerCameraBlendHandler.CameraBlendData(m_previousCamera));
    }
    
}
