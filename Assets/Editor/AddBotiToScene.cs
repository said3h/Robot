using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AddBotiToScene
{
    [MenuItem("Boti/Add 3D Character to Scene")]
    public static void AddBoti()
    {
        string modelPath = "Assets/Models/Meshy_AI_BOTI_Character_Sheet_biped_Character_output.fbx";

        if (!AssetDatabase.LoadAssetAtPath<GameObject>(modelPath))
        {
            EditorUtility.DisplayDialog("Boti", "Model not found at: " + modelPath, "OK");
            return;
        }

        GameObject botiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        GameObject botiInstance = (GameObject)PrefabUtility.InstantiatePrefab(botiPrefab);

        botiInstance.name = "Boti";
        botiInstance.transform.position = new Vector3(0, 0, 0);
        botiInstance.transform.rotation = Quaternion.Euler(0, 180, 0);

        float autoScale = EstimateGoodScale(botiPrefab);
        botiInstance.transform.localScale = new Vector3(autoScale, autoScale, autoScale);

        Renderer renderer = botiInstance.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            FixMaterials(renderer);
        }

        Selection.activeGameObject = botiInstance;
        EditorGUIUtility.PingObject(botiInstance);

        if (!EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene()))
        {
            Debug.LogWarning("Could not save scene. Make sure the scene is saved manually.");
        }

        Debug.Log("Boti 3D character added to scene at (0,0,0) with scale " + autoScale);
    }

    private static float EstimateGoodScale(GameObject prefab)
    {
        Bounds bounds = new Bounds();
        bool init = false;

        MeshFilter[] meshes = prefab.GetComponentsInChildren<MeshFilter>();
        foreach (var mf in meshes)
        {
            if (mf.sharedMesh == null)
                continue;
            if (!init)
            {
                bounds = mf.sharedMesh.bounds;
                init = true;
            }
            else
            {
                bounds.Encapsulate(mf.sharedMesh.bounds);
            }
        }

        if (!init)
            return 1f;

        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        if (maxDim < 0.01f)
            maxDim = 1f;

        float targetHeight = 1.2f;
        return targetHeight / maxDim;
    }

    private static void FixMaterials(Renderer renderer)
    {
        Material[] mats = renderer.sharedMaterials;

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] == null)
                continue;

            if (mats[i].shader == null || mats[i].shader.name.Contains("Standard") || mats[i].shader.name == "Built-in")
            {
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    mats[i] = new Material(urpLit);
                    mats[i].color = renderer.sharedMaterials[i]?.color ?? Color.white;
                }
                else
                {
                    Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
                    if (urpUnlit != null)
                    {
                        mats[i] = new Material(urpUnlit);
                        mats[i].color = renderer.sharedMaterials[i]?.color ?? Color.white;
                    }
                }
            }
        }

        renderer.sharedMaterials = mats;
    }
}