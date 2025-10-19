using UnityEngine;
using UnityEngine.Serialization;

public class Alarm : MonoBehaviour
{
    [FormerlySerializedAs("speed")] [SerializeField] float m_speed = 10f;
    void Update()
    {
        transform.Rotate(Vector3.right, m_speed * Time.deltaTime);       
    }
}
