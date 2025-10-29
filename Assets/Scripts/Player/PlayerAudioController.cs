using System;
using UnityEngine;

[Serializable]
internal class PlayerAudioController
{
    [SerializeField, RequiredField] AudioSource m_walkSoundSource;
    [SerializeField, Range(0.1f, 2f)] float m_minPitch = 0.8f;
    [SerializeField, Range(0.1f, 2f)] float m_maxPitch = 1.3f;
    [SerializeField] float m_fadeInSpeed = 5f;
    [SerializeField] float m_fadeOutSpeed = 3f;
    [SerializeField] float m_movementThreshold = 0.1f;

    float m_sprintMoveSpeed;

    public void Initialize(float sprintMoveSpeed)
    {
        m_sprintMoveSpeed = sprintMoveSpeed;
    }

    public void UpdateWalkingSound(Vector3 velocity, bool isGrounded)
    {
        if (m_walkSoundSource is null) return;

        Vector3 horizontalVelocity = new(velocity.x, 0, velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        bool isMoving = currentSpeed > m_movementThreshold && isGrounded;

        if (isMoving)
        {
            if (!m_walkSoundSource.isPlaying)
                m_walkSoundSource.Play();
    
            float normalizedSpeed = Mathf.InverseLerp(0, m_sprintMoveSpeed, currentSpeed);
            m_walkSoundSource.pitch = Mathf.Lerp(m_minPitch, m_maxPitch, normalizedSpeed);
            m_walkSoundSource.volume = Mathf.MoveTowards(m_walkSoundSource.volume, 1f, Time.deltaTime * m_fadeInSpeed);
        }
        else if (m_walkSoundSource.isPlaying)
        {
            m_walkSoundSource.volume = Mathf.MoveTowards(m_walkSoundSource.volume, 0f, Time.deltaTime * m_fadeOutSpeed);

            if (!(m_walkSoundSource.volume <= 0.01f)) return;
            m_walkSoundSource.Stop();
            m_walkSoundSource.volume = 1f;
        }
    }
}