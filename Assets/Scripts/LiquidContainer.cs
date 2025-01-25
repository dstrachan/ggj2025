using UnityEngine;

public class LiquidContainer : MonoBehaviour
{
    public float Volume { get; private set; }

    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private MeshRenderer bottleneck;
    [SerializeField] [Range(0, 1)] private float fillAmount = 0.5f;
    [SerializeField] private float pourSpeed = 1;

    private static readonly int GravityDirectionId = Shader.PropertyToID("_GravityDirection");
    private static readonly int SurfaceLevelId = Shader.PropertyToID("_SurfaceLevel");

    private void Awake()
    {
        Debug.Assert(liquidRenderer is not null);
        Debug.Assert(liquidMaterial is not null);

        var boundsSize = liquidRenderer.bounds.size;
        var scale = transform.lossyScale;
        Volume = boundsSize.x * boundsSize.y * boundsSize.z * scale.x * scale.y * scale.y * 1000;

        liquidRenderer.sharedMaterial = liquidMaterial;

        liquidMaterial.SetVector(GravityDirectionId, Vector3.down);
    }

    private void Update()
    {
        if (fillAmount > 0)
        {
            liquidRenderer.enabled = true;

            var surfaceLevel = GetSurfaceLevel();
            liquidRenderer.material.SetVector(SurfaceLevelId, surfaceLevel);

            if (ShouldPour(surfaceLevel.y))
            {
                PourLiquid();
            }
        }
        else
        {
            liquidRenderer.enabled = false;
        }
    }

    private Vector3 GetSurfaceLevel()
    {
        var bounds = liquidRenderer.bounds;
        var min = bounds.min.y;
        var max = bounds.max.y;

        var surfaceLevel = bounds.center;
        surfaceLevel.y = fillAmount * (max - min) + min;
        return surfaceLevel;
    }

    private bool ShouldPour(float surfaceLevel) => surfaceLevel > bottleneck.bounds.min.y ||
                                                   transform.rotation.eulerAngles.z is > 100 and < 260;

    private void PourLiquid()
    {
        var liquidToRemove = pourSpeed * Time.deltaTime * 0.25f;
        fillAmount = Mathf.Max(0, fillAmount - liquidToRemove);

        // TODO: Transfer liquid

        // TODO: Particles
    }

    private void OnDrawGizmos()
    {
        var surfaceLevel = GetSurfaceLevel();
        Gizmos.color = ShouldPour(surfaceLevel.y)
            ? Color.green
            : Color.red;
        var center = liquidRenderer.bounds.center;
        center.y = surfaceLevel.y;
        Gizmos.DrawSphere(center, 0.01f);
    }
}