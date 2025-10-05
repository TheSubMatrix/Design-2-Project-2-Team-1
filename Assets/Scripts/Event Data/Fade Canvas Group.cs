using UnityEngine;

public class FadeCanvasGroup : IEvent
{
    public readonly string CanvasGroupName;
    public readonly float FadeTime;
    public readonly float FadePercent;

    public FadeCanvasGroup(string canvasGroupName, float fadeTime, float fadePercent)
    {
        CanvasGroupName = canvasGroupName;
        FadeTime = fadeTime;
        FadePercent = fadePercent;
    }
}
