using UnityEngine;

/// <summary>
/// Simple camera follow script for smooth tracking of the Boti robot.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 15, 0);
    public bool lookAtTarget = true;
    
    void Start()
    {
        // Find robot if not assigned
        if (target == null)
        {
            GameObject boti = GameObject.Find("Boti");
            if (boti != null)
            {
                target = boti.transform;
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Calculate desired camera position
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Look at target if enabled
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
    
    /// <summary>
    /// Set the target to follow
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Snap camera to target immediately
    /// </summary>
    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
