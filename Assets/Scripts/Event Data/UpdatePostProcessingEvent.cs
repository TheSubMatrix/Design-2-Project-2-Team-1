using UnityEngine;
using UnityEngine.Rendering;

public struct UpdatePostProcessingEvent : IEvent
{
    public readonly VolumeProfile VolumeProfile;
    public readonly float FadeTime;
    public UpdatePostProcessingEvent(VolumeProfile volumeProfile, float fadeTime)
    {
        VolumeProfile = volumeProfile;
        FadeTime = fadeTime;
    }
}
