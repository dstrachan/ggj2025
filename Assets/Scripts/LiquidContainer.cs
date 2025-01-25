using System.Linq;
using UnityEngine;

public class LiquidContainer : MonoBehaviour
{
    public float Volume { get; private set; }

    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private Collider bottleneckCollider;
    [SerializeField] [Range(0, 1)] private float fillAmount = 0.5f;
    [SerializeField] private float pourSpeed = 1;
    [SerializeField] private float splashRadius = 0.025f;

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

    private bool ShouldPour(float surfaceLevel) => surfaceLevel > bottleneckCollider.bounds.min.y ||
                                                   transform.rotation.eulerAngles.z is > 100 and < 260;

    private void PourLiquid()
    {
        var percentageToRemove = pourSpeed * Time.deltaTime * 0.25f;
        fillAmount = Mathf.Max(0, fillAmount - percentageToRemove);

        // TODO: Where is the point that we hit with the liquid? For now we just assume the cup is below the bottleneck.
        if (TryGetContainer(bottleneckCollider.bounds.min, out var container))
        {
            Debug.Log($"{gameObject.name} filling {container.gameObject.name}");
            // TODO: Did we actually hit the bottleneck?
            var liquidAmount = Volume * percentageToRemove;
            // TODO: FIXME
            container.fillAmount = Mathf.Clamp01(container.fillAmount + liquidAmount);
        }

        // TODO: Particles
    }

    private bool TryGetContainer(Vector3 origin, out LiquidContainer liquidContainer)
    {
        var ray = new Ray(origin, Vector3.down);

        // var raycastHits = new RaycastHit[10]; // TODO: how many hits do we want?
        var raycastHits = Physics.SphereCastAll(ray, splashRadius);
        foreach (var hit in raycastHits.OrderBy(x => x.distance))
        {
            if (hit.collider is null || hit.collider == bottleneckCollider) continue;

            var container = hit.collider.GetComponentInParent<LiquidContainer>();
            if (container is not null)
            {
                liquidContainer = container;
                return true;
            }
        }

        liquidContainer = null;
        return false;
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

        if (TryGetContainer(bottleneckCollider.bounds.min, out var container))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(container.bottleneckCollider.bounds.center, 0.01f);
        }
    }
}