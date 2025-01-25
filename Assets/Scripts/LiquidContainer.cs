using System.Linq;
using UnityEngine;

public class LiquidContainer : MonoBehaviour
{
    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private Collider bottleneckCollider;
    [SerializeField] private bool infiniteContainer;
    [SerializeField] private float mlPerSecond = 50;
    [SerializeField] private float splashRadius = 0.025f;

    private float _totalVolume;
    private float _filledVolume;

    private static readonly int GravityDirectionId = Shader.PropertyToID("_GravityDirection");
    private static readonly int SurfaceLevelId = Shader.PropertyToID("_SurfaceLevel");

    private void Awake()
    {
        Debug.Assert(liquidRenderer is not null);
        Debug.Assert(liquidMaterial is not null);

        var boundsSize = liquidRenderer.bounds.size;
        var scale = transform.lossyScale;
        _totalVolume = boundsSize.x * boundsSize.y * boundsSize.z * scale.x * scale.y * scale.y *
                       178_296.7f; // Make sure the big bottle is 750ml
        _filledVolume = _totalVolume;

        liquidRenderer.sharedMaterial = liquidMaterial;

        liquidMaterial.SetVector(GravityDirectionId, Vector3.down);
    }

    private void Update()
    {
        if (infiniteContainer && _filledVolume < _totalVolume * 0.5f)
        {
            _filledVolume += mlPerSecond * Time.deltaTime * 2;
        }

        if (_filledVolume > 0)
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
        var filledPercentage = _filledVolume / _totalVolume;
        surfaceLevel.y = filledPercentage * (max - min) + min;
        return surfaceLevel;
    }

    private bool ShouldPour(float surfaceLevel) => surfaceLevel > bottleneckCollider.bounds.min.y ||
                                                   transform.rotation.eulerAngles.z is > 100 and < 260;

    private void PourLiquid()
    {
        var volumeToRemove = Mathf.Min(_filledVolume, mlPerSecond * Time.deltaTime);
        _filledVolume -= volumeToRemove;

        // TODO: Where is the point that we hit with the liquid? For now we just assume the cup is below the bottleneck.
        if (TryGetContainer(bottleneckCollider.bounds.min, out var container))
        {
            var volumeToAdd = Mathf.Min(container._totalVolume - container._filledVolume, volumeToRemove);
            container._filledVolume += volumeToAdd;
        }

        // TODO: Particles
    }

    private bool TryGetContainer(Vector3 origin, out LiquidContainer liquidContainer)
    {
        var ray = new Ray(origin, Vector3.down);

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