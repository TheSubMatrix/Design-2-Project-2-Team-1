using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIShockHandler : MonoBehaviour
{
    [SerializeField] float m_shockDuration = 0.1f;
    [SerializeField] float m_minTimeBetweenShock = 0.1f;
    [SerializeField] float m_maxTimeBetweenShock = 0.1f;
    [SerializeField] Image m_uiImage;
    [SerializeField] Sprite m_shockSprite;
    Sprite m_originalSprite;
    bool m_isShocking = false;
    void Awake()
    {
        m_originalSprite = m_uiImage.sprite;
        StartCoroutine(Shock());
    }
    void StopShock()
    {
        m_isShocking = false;
    }
    IEnumerator Shock()
    {
        m_isShocking = true;
        while (m_isShocking)
        {
            float waitTime = Random.Range(m_minTimeBetweenShock, m_maxTimeBetweenShock);
            yield return new WaitForSeconds(waitTime);
            m_uiImage.sprite = m_shockSprite;
            yield return new WaitForSeconds(m_shockDuration);
            m_uiImage.sprite = m_originalSprite;
        }
    }
}
