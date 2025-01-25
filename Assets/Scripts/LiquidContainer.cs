using UnityEngine;

public class LiquidContainer : MonoBehaviour
{
    public float Volume { get; private set; }

    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private MeshRenderer bottleneck;
    [SerializeField] [Range(0, 1)] private float fillAmount = 0.5f;
    [SerializeField] private float pourSpeed = 1;

    private float bottleneckRadius;

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

        bottleneckRadius = bottleneck.localBounds.extents.x;
    }

    private void Update()
    {
        if (fillAmount > 0)
        {
            liquidRenderer.enabled = true;

            var surfaceLevel = GetSurfaceLevel();
            liquidRenderer.material.SetVector(SurfaceLevelId, surfaceLevel);

            // TODO: Check extents when inverted.
            if (surfaceLevel.y > bottleneck.bounds.min.y)
            {
                var distance = surfaceLevel.y - bottleneck.bounds.min.y;
                var rate = Mathf.Clamp01(distance / bottleneckRadius);
                // TODO: This isn't quite right.
                PourLiquid(rate);
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

    private void PourLiquid(float rate)
    {
        Debug.Log(rate);

        var howLow = Vector3.Dot(Vector3.up, transform.up);
        var flowScale = 1 - (howLow + 1) * 0.5f + 0.2f;

        var liquidStep = bottleneck.bounds.extents.x * pourSpeed * Time.deltaTime * flowScale * rate;
        var newLiquidAmount = fillAmount - liquidStep;
        if (newLiquidAmount < 0)
        {
            liquidStep = fillAmount;
            newLiquidAmount = 0;
        }

        fillAmount = newLiquidAmount;

        // TODO: Transfer liquid

        // TODO: Particles
    }

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (fillAmount > 0)
        {
            var radius = bottleneck.bounds.size.x;
            var bottleneckPos = bottleneck.bounds.min;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(bottleneckPos, radius);

            var surfaceLevel = GetSurfaceLevel();
            Gizmos.color = surfaceLevel.y > bottleneck.bounds.min.y ? Color.green : Color.red;
            Gizmos.DrawWireSphere(surfaceLevel, radius);
        }
    }

    #endregion
}