using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor helper para reparar la camara principal sin crear duplicados.
/// Para usar: GameObject -> Fix Camera Setup
/// </summary>
public class FixCameraSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Fix Camera Setup")]
    public static void FixCamera()
    {
        Camera cam = GameObject.Find("MainCamera")?.GetComponent<Camera>();
        if (cam == null)
            cam = GameObject.Find("Main Camera")?.GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        if (cam == null)
        {
            GameObject legacyCamera = GameObject.Find("Camera");
            if (legacyCamera != null)
                cam = legacyCamera.GetComponent<Camera>();
        }

        if (cam == null)
        {
            GameObject newCamera = new GameObject("MainCamera");
            cam = newCamera.AddComponent<Camera>();
            newCamera.AddComponent<AudioListener>();
        }
        else
        {
        }

        RemoveDuplicateCameras(cam);
        ConfigureMainCamera(cam);
        EnsureDirectionalLight();
    }

    static void RemoveDuplicateCameras(Camera mainCamera)
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            if (camera != null && camera != mainCamera)
                Object.DestroyImmediate(camera.gameObject);
        }
    }

    static void ConfigureMainCamera(Camera cam)
    {
        cam.gameObject.name = "MainCamera";
        cam.tag = "MainCamera";
        cam.transform.SetParent(null);
        cam.transform.position = new Vector3(0, 26, -18);
        cam.transform.rotation = Quaternion.Euler(60, 0, 0);
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
        cam.cullingMask = -1;
        cam.orthographic = true;
        cam.orthographicSize = 15;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;

        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null)
            follow = cam.gameObject.AddComponent<CameraFollow>();

        follow.fixedPosition = new Vector3(0, 26, -18);
        follow.fixedRotation = new Vector3(60, 0, 0);
        follow.orthographicSize = 15;
        follow.ApplyFixedCamera();
    }

    static void EnsureDirectionalLight()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
                return;
        }

        GameObject lightObj = new GameObject("Directional Light");
        Light dirLight = lightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        dirLight.intensity = 1f;
    }
#endif
}
