using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraHolder : MonoBehaviour
{
    // Start is called before the first frame update
    [FormerlySerializedAs("cameraPosition")] public Transform m_cameraPosition;
    // Update is called once per frame
    void Update()
    {
        transform.position = m_cameraPosition.position;
    }
}
