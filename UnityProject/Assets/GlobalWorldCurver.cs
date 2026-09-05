using UnityEngine;

public class GlobalWorldCurver : MonoBehaviour
{
    [Header("Curve Settings")]
    public float curveStrength = 0.0008f;
    public float curveStartDistance = 15f;

    private Shader curvedShader;

    void Awake()
    {
        // Cache the custom shader early so it is immediately ready for spawned objects
        curvedShader = Shader.Find("Custom/CurvedStandard");
        if (curvedShader == null)
        {
            Debug.LogError("Could not find Custom/CurvedStandard shader!");
        }
    }

    void Start()
    {
        if (curvedShader == null) return;

        // 1. Process standard static scene meshes (like roads/buildings)
        MeshRenderer[] staticRenderers = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude);
        foreach (MeshRenderer renderer in staticRenderers)
        {
            if (renderer.CompareTag("Player")) continue;
            ApplyCurveToMeshRenderer(renderer);
        }

        // 2. Process animated skeletal scene meshes (if any exist at start)
        SkinnedMeshRenderer[] skinnedRenderers = FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Exclude);
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            if (renderer.CompareTag("Player")) continue;
            ApplyCurveToSkinnedRenderer(renderer);
        }
    }

    // Public method called by your CharacterSpawner script right when a prefab is instantiated
    public void ApplyCurveToTarget(GameObject target)
    {
        if (curvedShader == null || target == null) return;

        // Fix for Cars (Static meshes)
        MeshRenderer[] staticRenderers = target.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in staticRenderers)
        {
            ApplyCurveToMeshRenderer(renderer);
        }

        // Fix for Zombies/Humans (Animated skeletal meshes)
        SkinnedMeshRenderer[] skinnedRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            ApplyCurveToSkinnedRenderer(renderer);
        }
    }

    private void ApplyCurveToMeshRenderer(MeshRenderer renderer)
    {
        Material[] materials = renderer.materials;
        UpdateMaterials(materials);
        renderer.materials = materials;
    }

    private void ApplyCurveToSkinnedRenderer(SkinnedMeshRenderer renderer)
    {
        Material[] materials = renderer.materials;
        UpdateMaterials(materials);
        renderer.materials = materials;
    }

    private void UpdateMaterials(Material[] materials)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            Texture originalTexture = materials[i].mainTexture;
            Color originalColor = materials[i].color;

            materials[i].shader = curvedShader;

            if (originalTexture != null)
            {
                materials[i].SetTexture("_MainTex", originalTexture);
            }
            materials[i].SetColor("_BaseColor", originalColor);
            materials[i].SetFloat("_CurveStrength", curveStrength);
            materials[i].SetFloat("_CurveStartDistance", curveStartDistance);
        }
    }
}
