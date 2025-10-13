using System.Collections.Generic;
using UnityEngine;

public class SpawnRandomPickup : MonoBehaviour
{
    [SerializeField] List<GameObject> m_pickups;
    [SerializeField] float m_pickupChance = 0.5f;
    public void OnDropPickup()
    {
        if (Random.Range(0f, 1f) > m_pickupChance) return;
        int selectedIndex = Random.Range(0, m_pickups.Count);
        Instantiate(m_pickups[selectedIndex], transform.position, Quaternion.identity);
    }
}