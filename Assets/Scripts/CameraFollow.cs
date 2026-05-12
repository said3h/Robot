using UnityEngine;

/// <summary>
/// Fixed top-down camera helper.
/// Keeps the camera still so player movement does not feel dizzy.
/// </summary>
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(AudioListener))]
public class CameraFollow : MonoBehaviour
{
    [Header("Fixed Camera Settings")]
    public Vector3 fixedPosition = new Vector3(0, 26, -18);
    public Vector3 fixedRotation = new Vector3(60, 0, 0);
    public float orthographicSize = 15;

    void Awake()
    {
        EnsureSingleMainCamera();
        EnsureAudioListener();
    }

    void Start()
    {
        ApplyFixedCamera();
    }

    void LateUpdate()
    {
        ApplyFixedCamera();
    }

    public void ApplyFixedCamera()
    {
        transform.SetParent(null);
        gameObject.name = "MainCamera";
        gameObject.tag = "MainCamera";
        transform.position = fixedPosition;
        transform.rotation = Quaternion.Euler(fixedRotation);

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
        }

        EnsureAudioListener();
    }

    private void EnsureSingleMainCamera()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            if (cam == null || cam.gameObject == gameObject)
                continue;

            Debug.LogWarning("Boti camera guard removed duplicate camera: " + cam.gameObject.name);
            DestroySafely(cam.gameObject);
        }
    }

    private void EnsureAudioListener()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        foreach (AudioListener listener in listeners)
        {
            if (listener == null || listener.gameObject == gameObject)
                continue;

            Debug.LogWarning("Boti camera guard removed duplicate AudioListener from: " + listener.gameObject.name);
            DestroySafely(listener);
        }

        if (GetComponent<AudioListener>() == null)
        {
            gameObject.AddComponent<AudioListener>();
            Debug.Log("Boti camera guard added AudioListener to MainCamera.");
        }
    }

    private void DestroySafely(Object obj)
    {
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }

    /// <summary>
    /// Kept for compatibility with older setup code.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        ApplyFixedCamera();
    }

    /// <summary>
    /// Kept for compatibility with older setup code.
    /// </summary>
    public void SnapToTarget()
    {
        ApplyFixedCamera();
    }
}
