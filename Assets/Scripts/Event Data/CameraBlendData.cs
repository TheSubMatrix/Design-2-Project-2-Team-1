using Unity.Cinemachine;

public struct CameraBlendData : IEvent
{
    public CameraBlendData(CinemachineCamera cameraToBlendTo)
    {
        CameraToBlendTo = cameraToBlendTo;
    }
    public CinemachineCamera CameraToBlendTo { get; private set; }
}