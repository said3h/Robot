using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script para arreglar completamente la cámara.
/// Para usar: GameObject → Fix Camera Setup
/// </summary>
public class FixCameraSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Fix Camera Setup")]
    public static void FixCamera()
    {
        Debug.Log("═══════════════════════════════");
        Debug.Log("Fixing Camera Setup...");
        Debug.Log("═══════════════════════════════");
        
        // Eliminar cámaras antiguas
        Camera[] oldCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera oldCam in oldCameras)
        {
            if (oldCam.gameObject.name != "Main Camera")
            {
                Object.DestroyImmediate(oldCam.gameObject);
                Debug.Log("✓ Eliminada cámara antigua: " + oldCam.gameObject.name);
            }
        }
        
        // Crear o buscar Main Camera
        Camera cam = GameObject.Find("Main Camera")?.GetComponent<Camera>();
        
        if (cam == null)
        {
            GameObject camObj = GameObject.Find("Camera");
            if (camObj != null)
            {
                camObj.name = "Main Camera";
                cam = camObj.GetComponent<Camera>();
            }
        }
        
        if (cam == null)
        {
            GameObject newCamObj = new GameObject("Main Camera");
            cam = newCamObj.AddComponent<Camera>();
            Debug.Log("✓ Nueva Main Camera creada");
        }
        else
        {
            Debug.Log("✓ Main Camera encontrada");
        }
        
        // Configuración básica de cámara
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.2f); // Azul oscuro
        cam.cullingMask = -1; // Todo visible
        
        // Posición para ver toda la escena
        cam.transform.position = new Vector3(0, 12, -12);
        cam.transform.rotation = Quaternion.Euler(35, 0, 0);
        
        // Lente
        cam.fieldOfView = 50;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;
        
        // Asegurar que sea la cámara principal
        cam.tag = "MainCamera";
        
        Debug.Log("✓ Cámara configurada:");
        Debug.Log("  Posición: " + cam.transform.position);
        Debug.Log("  Rotación: " + cam.transform.eulerAngles);
        Debug.Log("  FOV: " + cam.fieldOfView);
        
        // Crear luz direccional si no existe
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        bool hasDirectionalLight = false;
        
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                hasDirectionalLight = true;
                break;
            }
        }
        
        if (!hasDirectionalLight)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            dirLight.intensity = 1f;
            Debug.Log("✓ Luz direccional creada");
        }
        
        Debug.Log("═══════════════════════════════");
        Debug.Log("¡Configuración de cámara completada!");
        Debug.Log("Ahora ve a Game View y你应该 ver la escena.");
        Debug.Log("═══════════════════════════════");
    }
#endif
}