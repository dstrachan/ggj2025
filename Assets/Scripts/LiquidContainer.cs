using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class LiquidContainer : MonoBehaviour
{
    public float Volume { get; private set; }

    public float FillAmount
    {
        get => fillAmount;
        set => fillAmount = Mathf.Clamp01(value);
    }

    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] [Range(0, 1)] private float fillAmount = 0.5f;

    private static readonly int GravityDirectionId = Shader.PropertyToID("_GravityDirection");
    private static readonly int SurfaceLevelId = Shader.PropertyToID("_SurfaceLevel");

    private void Awake()
    {
        Debug.Assert(liquidRenderer is not null);
        Debug.Assert(liquidMaterial is not null);

        var meshFilter = liquidRenderer.GetComponent<MeshFilter>();
        var boundsSize = meshFilter.sharedMesh.bounds.size;
        var scale = transform.lossyScale;
        Volume = boundsSize.x * boundsSize.y * boundsSize.z * scale.x * scale.y * scale.y * 1000;

        liquidRenderer.sharedMaterial = liquidMaterial;

        liquidMaterial.SetVector(GravityDirectionId, Vector3.down);
    }

    private void Update()
    {
        UpdateSurface();
    }

    private void UpdateSurface()
    {
        if (fillAmount > 0)
        {
            var bounds = liquidRenderer.bounds;
            var min = bounds.min.y;
            var max = bounds.max.y;

            var surfaceLevel = bounds.center;
            surfaceLevel.y = fillAmount * (max - min) + min;
            liquidRenderer.material.SetVector(SurfaceLevelId, surfaceLevel);

            liquidRenderer.enabled = true;
        }
        else
        {
            liquidRenderer.enabled = false;
        }
    }
}