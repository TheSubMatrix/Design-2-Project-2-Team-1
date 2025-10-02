using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyVisionSensor : MonoBehaviour
{
    public float m_detectionRadius = 10f;
    public List<string> m_targetTags = new();
    readonly List<Transform> m_detectedTargets = new(10);
    Transform m_cachedTarget;
    int m_frameCountForLastCache = -1;
    SphereCollider m_sightCollider;

    void Start()
    {
        
        m_sightCollider = GetComponent<SphereCollider>();
        m_sightCollider.isTrigger = true;
        m_sightCollider.radius = m_detectionRadius;
        // ReSharper disable once Unity.PreferNonAllocApi
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_detectionRadius);
        foreach (Collider c in colliders)
        {
            ProcessTrigger(c, item => m_detectedTargets.Add(item));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(other, item => m_detectedTargets.Add(item));
    }
    void OnTriggerExit(Collider other)
    {
        ProcessTrigger(other, item => m_detectedTargets.Remove(item));
    }

    void ProcessTrigger(Collider other, Action<Transform> action)
    {
        if(other.CompareTag("Untagged")) return;
        foreach (string _ in m_targetTags.TakeWhile(other.CompareTag))
        {
            action(other.transform);
        }
    }
    
    public Transform GetClosestTarget(string tagToConsider)
    {
        if (m_detectedTargets.Count == 0) return null;
        if (m_frameCountForLastCache == Time.frameCount) return m_cachedTarget;
        Transform closestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Transform potentialTarget in m_detectedTargets)
        {
            if (!potentialTarget.CompareTag(tagToConsider)) continue;
            Vector3 directionToTarget = potentialTarget.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (!(dSqrToTarget < closestDistanceSqr)) continue;
            closestDistanceSqr = dSqrToTarget;
            closestTarget = potentialTarget;
        }
        m_cachedTarget = closestTarget;
        m_frameCountForLastCache = Time.frameCount;
        return closestTarget;
    }
}
