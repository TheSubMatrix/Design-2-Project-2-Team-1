
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class SceneTransitionHandler : PersistentSingleton<SceneTransitionHandler>
{
    static readonly int Curvature = Shader.PropertyToID("_Curvature");
    public bool IsTransitioning { get; private set; } 
    [SerializeField] Material m_crtMaterial;
    [SerializeField] SoundData m_transitionStartSound;
    [SerializeField] SoundData m_transitionEndSound;
    [SerializeField] AudioMixerSnapshot m_transitionStartSnapshot;
    [SerializeField] AudioMixerSnapshot m_transitionEndSnapshot;

    public void TransitionScene(string sceneName, float transitionTime)
    {
        if(IsTransitioning) return;
        StartCoroutine(TransitionSceneWithEffectAsync(sceneName, transitionTime));
    }
    public void TransitionScene(string sceneName)
    {
        // ReSharper disable once IntroduceOptionalParameters.Global
        //Need to do it this way so that it can be called via an event
        TransitionScene(sceneName, 0.5f);
    }
    public void ReloadScene(float transitionTime = 0.5f)
    {
        TransitionScene(SceneManager.GetActiveScene().name, transitionTime);
    }

    IEnumerator TransitionSceneWithEffectAsync(string sceneName, float transitionTime = 0.5f)
    {
        IsTransitioning = true;
        float startingCurvature = m_crtMaterial.GetFloat(Curvature);

        SoundManager.Instance.CreateSound().WithSoundData(m_transitionStartSound).WithPosition(transform.position).WithRandomPitch().Play();
        m_transitionStartSnapshot.TransitionTo(transitionTime);
        yield return FadeCRTTransitionAndSoundAsync(transitionTime, 0.0f);
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        SoundManager.Instance.CreateSound().WithSoundData(m_transitionEndSound).WithPosition(transform.position).WithRandomPitch().Play();
        m_transitionEndSnapshot.TransitionTo(transitionTime);
        yield return FadeCRTTransitionAndSoundAsync(transitionTime, startingCurvature);
        IsTransitioning = false;
    }

    IEnumerator FadeCRTTransitionAndSoundAsync(float transitionTime, float destinationCurvature)
    {
        float elapsedTime = 0.0f;
        float startingCurvature = m_crtMaterial.GetFloat(Curvature);
        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            m_crtMaterial.SetFloat(Curvature, Mathf.SmoothStep(startingCurvature, destinationCurvature, elapsedTime / transitionTime));
            yield return null;
        }
    }
}
