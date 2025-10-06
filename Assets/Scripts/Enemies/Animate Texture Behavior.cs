using System.Collections.Generic;
using UnityEngine;

public class AnimateTextureBehavior : StateMachineBehaviour
{
    MeshRenderer m_renderer;
    int m_currentFrame;
    [SerializeField] List<Texture2D> m_textures;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        m_renderer = animator.GetComponent<MeshRenderer>();
        m_currentFrame = -1; // Reset frame tracking
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_textures == null || m_textures.Count == 0) return;
        
        // normalizedTime is already a 0-1 value (or higher if looping)
        // Use modulo to handle looping animations
        float normalizedTime = stateInfo.normalizedTime % 1f;
        
        int frame = Mathf.FloorToInt(normalizedTime * m_textures.Count);
        
        // Clamp to prevent index out of bounds
        frame = Mathf.Clamp(frame, 0, m_textures.Count - 1);
        
        // Only update if frame changed (optimization)
        if (frame == m_currentFrame) return;
        m_currentFrame = frame;
        m_renderer.material.mainTexture = m_textures[frame];
    }
}