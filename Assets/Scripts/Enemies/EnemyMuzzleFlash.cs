using System.Collections;
using UnityEngine;

public class EnemyMuzzleFlash : MonoBehaviour
{
    [SerializeField] float m_muzzleFlashDuration = 0.1f;
    [SerializeField] GameObject m_muzzleFlash;
    public void OnMuzzleFlash()
    {
        StartCoroutine(MuzzleFlash());
    }

    IEnumerator MuzzleFlash()
    {
        m_muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(m_muzzleFlashDuration);
        m_muzzleFlash.SetActive(false);
    }
}
