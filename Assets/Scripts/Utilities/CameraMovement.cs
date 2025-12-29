using System;
using UnityEngine;
using NewtonianGravity.Utilities;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] 
    [Tooltip("Target to follow. Assign a GameObject with SystemCenterProxy to follow the system's center of mass.")]
    Transform targetTransform;
    
    [SerializeField]
    [Tooltip("Offset from the target position.")]
    Vector3 offset = new Vector3(0f, 0f, -10f);
    
    void Update()
    {
        if (targetTransform == null) return;
        
        transform.position = new Vector3(
            targetTransform.position.x + offset.x,
            transform.position.y + offset.y,
            targetTransform.position.z + offset.z
        );
    }
}
