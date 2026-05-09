using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script para ajustar la cámara automáticamente.
/// Para usar: GameObject → Adjust Camera View
/// </summary>
public class AdjustCamera : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Adjust Camera View")]
    public static void Adjust()
    {
        // Buscar o crear una cámara
        Camera cam = Camera.main;
        
        if (cam == null)
        {
            // Crear cámara si no existe
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
        }
        
        // Configurar posición y rotación para vista top-down inclinada
        cam.transform.position = new Vector3(0, 15, -10);
        cam.transform.rotation = Quaternion.Euler(45, 0, 0);
        cam.transform.LookAt(new Vector3(0, 0, 4)); // Mirar hacia el centro de la escena
        
        // Configurar propiedades de la cámara
        cam.fieldOfView = 50;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
        
        Debug.Log("✓ Cámara ajustada para ver la escena Boti");
        Debug.Log("  Posición: (0, 15, -10)");
        Debug.Log("  Rotación: (45, 0, 0)");
        Debug.Log("  FOV: 50");
    }
#endif
}