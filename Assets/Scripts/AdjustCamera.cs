using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor helper para ajustar la MainCamera al estilo RTS fijo de Boti.
/// Para usar: GameObject -> Adjust Camera View
/// </summary>
public class AdjustCamera : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Adjust Camera View")]
    public static void Adjust()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            Debug.Log("AdjustCamera created MainCamera with AudioListener.");
        }
        else if (cam.GetComponent<AudioListener>() == null)
        {
            cam.gameObject.AddComponent<AudioListener>();
            Debug.Log("AdjustCamera added missing AudioListener to MainCamera.");
        }

        cam.gameObject.name = "MainCamera";
        cam.tag = "MainCamera";
        cam.transform.SetParent(null);
        cam.transform.position = new Vector3(0, 26, -18);
        cam.transform.rotation = Quaternion.Euler(60, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 15;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = cam.gameObject.AddComponent<CameraFollow>();
            Debug.Log("AdjustCamera added CameraFollow fixed camera guard.");
        }

        follow.fixedPosition = new Vector3(0, 26, -18);
        follow.fixedRotation = new Vector3(60, 0, 0);
        follow.orthographicSize = 15;
        follow.ApplyFixedCamera();

        Debug.Log("AdjustCamera configured fixed RTS MainCamera.");
        Debug.Log("MainCamera position: " + cam.transform.position);
        Debug.Log("MainCamera rotation: " + cam.transform.eulerAngles);
        Debug.Log("MainCamera orthographic size: " + cam.orthographicSize);
    }
#endif
}
