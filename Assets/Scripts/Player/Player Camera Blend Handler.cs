
using Unity.Cinemachine;
using UnityEngine;

public partial class PlayerCameraBlendHandler : MonoBehaviour
{
    [SerializeField] CinemachineBrain m_brain;

    EventBinding<CameraBlendData> m_blendToTargetEvent;
    void OnEnable()
    {
        m_blendToTargetEvent = new EventBinding<CameraBlendData>(HandleIncomingBlendEvent);
        EventBus<CameraBlendData>.Register(m_blendToTargetEvent);
    }
    
    void OnDisable()
    {
        EventBus<CameraBlendData>.Deregister(m_blendToTargetEvent);
    }

    void Start()
    {
        m_brain = CinemachineBrain.GetActiveBrain(0);
    }

    void HandleIncomingBlendEvent(CameraBlendData data)
    {
        CinemachineVirtualCameraBase virtualCamera = m_brain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
        if (virtualCamera != null) virtualCamera.Priority.Value = 0;
        if(data.CameraToBlendTo is null) return;
        data.CameraToBlendTo.Priority.Value = 10;
    }
}
