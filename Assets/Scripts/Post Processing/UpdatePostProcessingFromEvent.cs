using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class UpdatePostProcessingFromEvent : MonoBehaviour
{
    EventBinding<UpdatePostProcessingEvent> m_postProcessingEvent;
    [SerializeField] List<Volume> m_postProcessingVolumes = new List<Volume>();
    Coroutine currentFade;

    void Awake()
    {
        foreach (Volume volume in GetComponentsInChildren<Volume>())
        {
            if (!m_postProcessingVolumes.Contains(volume))
            {
                m_postProcessingVolumes.Add(volume);
            }
            volume.weight = 0;
        }

        if (m_postProcessingVolumes.Count <= 0) return;
        Volume processingVolume = m_postProcessingVolumes[0];
        processingVolume.weight = 1.0f;
    }

    void OnEnable()
    {
        m_postProcessingEvent = new EventBinding<UpdatePostProcessingEvent>(OnUpdatePostProcessing);
        EventBus<UpdatePostProcessingEvent>.Register(m_postProcessingEvent);
    }

    void OnDisable()
    {
        EventBus<UpdatePostProcessingEvent>.Deregister(m_postProcessingEvent);
    }

    void OnUpdatePostProcessing(UpdatePostProcessingEvent e)
    {
        // Clean up any null references first
        m_postProcessingVolumes.RemoveAll(v => v == null);
        
        // Find volume by profile name instead of reference comparison
        Volume volumeToFadeTo = null;
        if (e.VolumeProfile != null)
        {
            string targetProfileName = e.VolumeProfile.name;
            foreach (Volume volume in m_postProcessingVolumes.Where(volume => volume != null && volume.profile != null && volume.profile.name == targetProfileName))
            {
                volumeToFadeTo = volume;
                break;
            }
        }

        //Create a new volume if we couldn't find an existing one
        if (volumeToFadeTo == null && e.VolumeProfile != null)
        {
            GameObject volumeObject = new GameObject($"{e.VolumeProfile.name} Volume");
            volumeObject.transform.SetParent(transform);
            volumeObject.transform.localPosition = Vector3.zero;
            volumeObject.transform.localRotation = Quaternion.identity;
            volumeObject.transform.localScale = Vector3.one;
            volumeToFadeTo = volumeObject.AddComponent<Volume>();
            volumeToFadeTo.isGlobal = true;
            volumeToFadeTo.priority = 0;
            volumeToFadeTo.weight = 0;
            volumeToFadeTo.profile = e.VolumeProfile;
            m_postProcessingVolumes.Add(volumeToFadeTo);
        }

        // Stop any existing fade
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        
        if (volumeToFadeTo != null)
        {
            currentFade = StartCoroutine(FadeVolumeProfile(volumeToFadeTo, e.FadeTime));
        }
    }

    IEnumerator FadeVolumeProfile(Volume volumeToFadeIn, float duration)
    {
        // Store starting weights for ALL volumes
        Dictionary<Volume, float> startingWeights = new Dictionary<Volume, float>();
        foreach (Volume volume in m_postProcessingVolumes.Where(volume => volume is not null))
        {
            startingWeights[volume] = volume.weight;
        }

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            // Fade out all volumes except the target
            foreach (Volume volume in m_postProcessingVolumes.Where(volume => volume is not null))
            {
                volume.weight = Mathf.Lerp(startingWeights[volume], volume == volumeToFadeIn ? 1 : 0, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are set for ALL volumes
        foreach (Volume volume in m_postProcessingVolumes.Where(volume => volume is not null))
        {
            volume.weight = volume == volumeToFadeIn ? 1 : 0;
        }
        currentFade = null;
    }
}