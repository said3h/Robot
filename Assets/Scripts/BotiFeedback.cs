using System.Collections;
using UnityEngine;

/// <summary>
/// Visual and audio feedback for Boti actions.
/// This does not change game rules; it only shows what is happening.
/// </summary>
public class BotiFeedback : MonoBehaviour
{
    private GameObject tileHighlight;
    private GameObject buildPreview;
    private AudioSource audioSource;
    private AudioClip collectClip;
    private AudioClip buildClip;
    private AudioClip errorClip;
    private Material validMaterial;
    private Material invalidMaterial;
    private Material validPreviewMaterial;
    private Material invalidPreviewMaterial;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        collectClip = CreateTone("Collect", 660f, 0.08f);
        buildClip = CreateTone("Build", 440f, 0.1f);
        errorClip = CreateTone("Error", 160f, 0.12f);
        validMaterial = CreateFeedbackMaterial(Color.green, 0.35f);
        invalidMaterial = CreateFeedbackMaterial(Color.red, 0.35f);
        validPreviewMaterial = CreateFeedbackMaterial(Color.green, 0.45f);
        invalidPreviewMaterial = CreateFeedbackMaterial(Color.red, 0.45f);

        tileHighlight = CreateFlatCube("TileHighlight");
        buildPreview = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buildPreview.name = "BuildPreview";
        Destroy(buildPreview.GetComponent<Collider>());
        buildPreview.SetActive(false);
    }

    public void ShowTileHighlight(Vector3 worldPosition, bool isValid)
    {
        tileHighlight.transform.position = worldPosition + new Vector3(0, 0.08f, 0);
        tileHighlight.GetComponent<Renderer>().sharedMaterial = isValid ? validMaterial : invalidMaterial;
        tileHighlight.SetActive(true);
    }

    public void ShowBuildPreview(Vector3 worldPosition, WorldStructureType structureType, bool isValid)
    {
        buildPreview.transform.position = worldPosition + new Vector3(0, 0.45f, 0);
        buildPreview.transform.localScale = structureType == WorldStructureType.Wall
            ? new Vector3(0.9f, 0.9f, 0.9f)
            : new Vector3(0.75f, 0.65f, 0.75f);
        buildPreview.GetComponent<Renderer>().sharedMaterial = isValid ? validPreviewMaterial : invalidPreviewMaterial;
        buildPreview.SetActive(true);
    }

    public void HideBuildPreview()
    {
        if (buildPreview != null)
            buildPreview.SetActive(false);
    }

    public void ShowFloatingText(string message, Vector3 worldPosition)
    {
        StartCoroutine(FloatingTextRoutine(message, worldPosition));
    }

    public void PlayCollect()
    {
        audioSource.PlayOneShot(collectClip);
    }

    public void PlayBuild()
    {
        audioSource.PlayOneShot(buildClip);
    }

    public void PlayError()
    {
        audioSource.PlayOneShot(errorClip);
    }

    private IEnumerator FloatingTextRoutine(string message, Vector3 worldPosition)
    {
        GameObject textObj = new GameObject("FloatingText");
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = message;
        textMesh.fontSize = 42;
        textMesh.characterSize = 0.08f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.white;

        Vector3 start = worldPosition + new Vector3(0, 1.2f, 0);
        float duration = 0.8f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            textObj.transform.position = start + Vector3.up * t;

            if (Camera.main != null)
                textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - Camera.main.transform.position);

            Color color = textMesh.color;
            color.a = 1f - t;
            textMesh.color = color;
            yield return null;
        }

        Destroy(textObj);
    }

    private GameObject CreateFlatCube(string objectName)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = objectName;
        obj.transform.localScale = new Vector3(0.95f, 0.03f, 0.95f);
        Destroy(obj.GetComponent<Collider>());
        obj.SetActive(false);
        return obj;
    }

    private Material CreateFeedbackMaterial(Color color, float alpha)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        color.a = alpha;
        mat.color = color;
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        return mat;
    }

    private AudioClip CreateTone(string clipName, float frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * 0.15f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
