using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// Script maestro para crear la escena Boti completa.
/// Uso: GameObject → Create Complete Boti Scene
/// </summary>
public class CreateBotiScene : MonoBehaviour
{
    [MenuItem("GameObject/Create Complete Boti Scene")]
    public static void CreateCompleteScene()
    {
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("CREANDO ESCENA BOTI COMPLETA");
        Debug.Log("═══════════════════════════════════════════");

        // 1. Crear Floor (Plane)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(3, 1, 3);

        // Material gris para el piso
        Material floorMat = new Material(Shader.Find("Standard"));
        floorMat.color = new Color(0.3f, 0.3f, 0.3f);
        floor.GetComponent<Renderer>().material = floorMat;
        Debug.Log("✓ Floor creado en (0,0,0)");

        // 2. Crear Boti (cubo azul con script de movimiento)
        GameObject boti = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boti.name = "Boti";
        boti.transform.position = new Vector3(0, 0.5f, 0);

        // Material azul para Boti
        Material botiMat = new Material(Shader.Find("Standard"));
        botiMat.color = Color.blue;
        boti.GetComponent<Renderer>().material = botiMat;

        // Agregar script de movimiento BotiMovement
        boti.AddComponent<BotiMovement>();
        Debug.Log("✓ Boti creado con script BotiMovement");

        // 3. Crear Goal (cubo verde)
        GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goal.name = "Goal";
        goal.transform.position = new Vector3(0, 0.5f, 8);
        goal.transform.localScale = new Vector3(1, 1, 1);

        // Material verde para la meta
        Material goalMat = new Material(Shader.Find("Standard"));
        goalMat.color = Color.green;
        goal.GetComponent<Renderer>().material = goalMat;

        // Agregar tag Goal
        if (goal.tag != "Goal")
        {
            Debug.LogWarning("Por favor asigna el tag 'Goal' al objeto Goal en el Inspector");
        }
        Debug.Log("✓ Goal creado en (0, 0.5, 8)");

        // 4. Crear Obstacle (cubo rojo más grande)
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "Obstacle";
        obstacle.transform.position = new Vector3(0, 0.5f, 4);
        obstacle.transform.localScale = new Vector3(2, 2, 2);

        // Material rojo para el obstáculo
        Material obstacleMat = new Material(Shader.Find("Standard"));
        obstacleMat.color = Color.red;
        obstacle.GetComponent<Renderer>().material = obstacleMat;
        Debug.Log("✓ Obstacle creado en (0, 0.5, 4) con escala (2,2,2)");

        // 5. Configurar Main Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj == null)
            {
                camObj = new GameObject("MainCamera");
                cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                cam.tag = "MainCamera";
                Debug.Log("CreateBotiScene created MainCamera with AudioListener.");
            }
            else
            {
                cam = camObj.GetComponent<Camera>();
            }
        }

        // Posicionar cámara RTS fija para ver la escena completa
        cam.transform.SetParent(null);
        cam.transform.position = new Vector3(0, 26, -18);
        cam.transform.rotation = Quaternion.Euler(60, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 15;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;
        cam.gameObject.name = "MainCamera";
        cam.tag = "MainCamera";
        if (cam.GetComponent<AudioListener>() == null)
        {
            cam.gameObject.AddComponent<AudioListener>();
            Debug.Log("CreateBotiScene added missing AudioListener to MainCamera.");
        }

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = cam.gameObject.AddComponent<CameraFollow>();
            Debug.Log("CreateBotiScene added CameraFollow fixed camera guard.");
        }

        follow.fixedPosition = new Vector3(0, 26, -18);
        follow.fixedRotation = new Vector3(60, 0, 0);
        follow.orthographicSize = 15;
        follow.ApplyFixedCamera();
        Debug.Log("✓ Main Camera configurada");

        // 6. Crear luz direccional
        Light[] lights = FindObjectsOfType<Light>();
        bool hasLight = false;
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                hasLight = true;
                break;
            }
        }

        if (!hasLight)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            dirLight.intensity = 1f;
            Debug.Log("✓ Luz Directional creada");
        }

        // 7. Guardar escena
        string scenePath = "Assets/Scenes/Level_01.unity";

        // Crear directorio si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
            Debug.Log("✓ Carpeta Scenes creada");
        }

        // Guardar escena
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        Debug.Log("CreateBotiScene saved scene to: " + scenePath);

        // Seleccionar Boti en la Hierarchy
        Selection.activeGameObject = boti;
        EditorGUIUtility.PingObject(boti);

        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("¡ESCENA BOTI CREADA EXITOSAMENTE!");
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("Objetos creados:");
        Debug.Log("  • Floor (piso gris)");
        Debug.Log("  • Boti (cubo azul con movimiento)");
        Debug.Log("  • Goal (cubo verde)");
        Debug.Log("  • Obstacle (cubo rojo)");
        Debug.Log("  • Main Camera (vista isométrica)");
        Debug.Log("  • Directional Light");
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("CONTROLES:");
        Debug.Log("  W = Mover hacia adelante");
        Debug.Log("  A = Girar a la izquierda");
        Debug.Log("  D = Girar a la derecha");
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("Presiona PLAY (▶) para probar el juego!");
        Debug.Log("═══════════════════════════════════════════");
    }
}
