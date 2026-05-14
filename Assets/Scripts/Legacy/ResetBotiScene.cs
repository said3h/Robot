using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script para reposicionar todos los objetos y la cámara.
/// Para usar: GameObject → Reset Boti Scene
/// </summary>
public class ResetBotiScene : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Reset Boti Scene")]
    public static void ResetScene()
    {
        // Mover Floor al centro
        GameObject floor = GameObject.Find("Floor");
        if (floor != null)
        {
            floor.transform.position = new Vector3(0, 0, 0);
            floor.transform.localScale = new Vector3(3, 1, 3);
            Debug.Log("✓ Floor posicionado");
        }
        else
        {
            Debug.LogWarning("Floor no encontrado");
        }
        
        // Mover Boti al frente de la cámara
        GameObject boti = GameObject.Find("Boti");
        if (boti != null)
        {
            boti.transform.position = new Vector3(0, 0.5f, -5);
            boti.transform.rotation = Quaternion.identity;
            Debug.Log("✓ Boti posicionado");
        }
        else
        {
            Debug.LogWarning("Boti no encontrado");
        }
        
        // Mover Goal
        GameObject goal = GameObject.Find("Goal");
        if (goal != null)
        {
            goal.transform.position = new Vector3(0, 0.5f, 5);
            goal.transform.localScale = new Vector3(1, 1, 1);
            Debug.Log("✓ Goal posicionado");
        }
        else
        {
            Debug.LogWarning("Goal no encontrado");
        }
        
        // Mover Obstacle
        GameObject obstacle = GameObject.Find("Obstacle");
        if (obstacle != null)
        {
            obstacle.transform.position = new Vector3(0, 0.5f, 0);
            obstacle.transform.localScale = new Vector3(1, 1, 1);
            Debug.Log("✓ Obstacle posicionado");
        }
        else
        {
            Debug.LogWarning("Obstacle no encontrado");
        }
        
        // Ajustar cámara para ver todo
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 10, -15);
            cam.transform.rotation = Quaternion.Euler(30, 0, 0);
            cam.fieldOfView = 60;
            Debug.Log("✓ Cámara ajustada");
        }
        else
        {
            Debug.LogWarning("Main Camera no encontrada");
        }
        
        Debug.Log("═══════════════════════════════");
        Debug.Log("¡Escena reiniciada!");
        Debug.Log("Ahora deberías ver todos los objetos.");
        Debug.Log("═══════════════════════════════");
    }
#endif
}