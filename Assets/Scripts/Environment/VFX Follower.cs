using UnityEngine;

public class VFXFollower : MonoBehaviour
{
    Transform m_target;
    Vector3 m_positionOffset;
    Quaternion m_rotationOffset;

    void Update()
    {
        if (!m_target) return;
        transform.position = m_target.position + m_positionOffset;
        transform.rotation = m_target.rotation * m_rotationOffset;
    }

    public void SetTarget(Transform target)
    {
        m_target = target;
        if (target == null) return;
        m_positionOffset = transform.position - target.position;
        m_rotationOffset = Quaternion.Inverse(target.rotation) * transform.rotation;
    }

    public void ClearTarget()
    {
        m_target = null;
    }

    public bool HasTarget()
    {
        return m_target != null;
    }
}