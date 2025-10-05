using System.Collections;
using UnityEngine;

public class FadeCanvasGroupFromEvent : MonoBehaviour
{
    EventBinding<FadeCanvasGroup> m_fadeEvent;
    [SerializeField] CanvasGroup m_canvasGroupToFade;
    [SerializeField] string m_canvasGroupName;
    void OnEnable()
    {
        m_fadeEvent = new EventBinding<FadeCanvasGroup>(OnFadeCanvasGroup);
        EventBus<FadeCanvasGroup>.Register(m_fadeEvent);
    }
    void OnDisable()
    {
        EventBus<FadeCanvasGroup>.Deregister(m_fadeEvent);
    }

    void OnFadeCanvasGroup(FadeCanvasGroup e)
    {
        if(e.CanvasGroupName != m_canvasGroupName) return;
        StartCoroutine(FadeCanvasGroupAsync(e));
    }

    IEnumerator FadeCanvasGroupAsync(FadeCanvasGroup e)
    {
        float elapsedTime = 0;
        while (elapsedTime <= e.FadeTime)
        {
            m_canvasGroupToFade.alpha = Mathf.Lerp(m_canvasGroupToFade.alpha, e.FadePercent, elapsedTime / e.FadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
