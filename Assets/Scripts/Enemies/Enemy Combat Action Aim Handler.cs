using System;
using UnityEngine;

public class EnemyCombatActionAimHandler : MonoBehaviour
{
    [SerializeField] Transform m_aimPoint;
    [SerializeField] EnemyVisionSensor m_enemyVisionSensor;
    [SerializeField] float m_aimingSpeed = 100f;
    [SerializeField] float m_aimPointSpeed = 150f;
    [SerializeField] float m_maxVerticalAngle = 60f; 
    [SerializeField] float m_aimThreshold = 5f; 
    
    private bool m_isAimedAtTarget = false;
    
    void LateUpdate()
    {
        Transform target = m_enemyVisionSensor.GetClosestTarget("Player");
        if (target is null)
        {
            m_isAimedAtTarget = false;
            return;
        }
        
        // === Body Rotation (Y-axis only) ===
        Vector3 horizontalDirection = target.position - transform.position;
        horizontalDirection.y = 0;
        
        if (horizontalDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetBodyRotation = Quaternion.LookRotation(horizontalDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetBodyRotation, m_aimingSpeed * Time.deltaTime);
        }
        
        // === Aim Point Rotation (vertical aiming) ===
        if (m_aimPoint)
        {
            Vector3 aimDirection = target.position - m_aimPoint.position;
            
            // Calculate the angle to the target
            float distance = new Vector3(aimDirection.x, 0, aimDirection.z).magnitude;
            float angle = Mathf.Atan2(aimDirection.y, distance) * Mathf.Rad2Deg;
            
            // Clamp the angle
            angle = Mathf.Clamp(angle, -m_maxVerticalAngle, m_maxVerticalAngle);
            
            // Create target rotation for aim point (rotation around X-axis for pitch)
            Quaternion targetAimRotation = Quaternion.Euler(-angle, 0, 0);
            
            // Smoothly rotate the aim point
            m_aimPoint.localRotation = Quaternion.RotateTowards(m_aimPoint.localRotation, targetAimRotation, m_aimPointSpeed * Time.deltaTime);
        }
        
        // Check if aimed at target
        m_isAimedAtTarget = IsAimedAtTarget();
    }
    
    /// <summary>
    /// Checks if the enemy is currently aimed at the target from the vision sensor
    /// </summary>
    public bool IsAimedAtTarget()
    {
        Transform target = m_enemyVisionSensor.GetClosestTarget("Player");
        if (!target) return false;
        
        Transform aimTransform = m_aimPoint ? m_aimPoint : transform;
        
        // Calculate direction to target
        Vector3 directionToTarget = (target.position - aimTransform.position).normalized;
        
        // Get the forward direction of the aim point
        Vector3 aimForward = aimTransform.forward;
        
        // Calculate the angle between aim direction and target direction
        float angle = Vector3.Angle(aimForward, directionToTarget);
        
        // Return true if within threshold
        return angle <= m_aimThreshold;
    }
}