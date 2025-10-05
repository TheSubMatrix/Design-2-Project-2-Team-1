using System;
using UnityEngine;

public class EnemyCombatActionAimHandler : MonoBehaviour
{
    [SerializeField] EnemyVisionSensor m_enemyVisionSensor;
    [SerializeField] float m_aimingSpeed = 100f;
    void LateUpdate()
    {
        Transform target = m_enemyVisionSensor.GetClosestTarget("Player");
        Vector3 lookDirection = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(lookDirection);
        rotation.x = 0;
        rotation.z = 0;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, m_aimingSpeed * Time.deltaTime);
    }
}
