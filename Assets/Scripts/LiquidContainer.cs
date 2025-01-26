using System.Linq;
using SplineMesh;
using UnityEngine;
using UnityEngine.VFX;

public class LiquidContainer : MonoBehaviour
{
    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private Collider bottleneckCollider;
    [SerializeField] private bool infiniteContainer;
    [SerializeField] private float mlPerSecond = 50;
    [SerializeField] private float splashRadius = 0.025f;
    [SerializeField] private Spline spline;
    [SerializeField] private VisualEffect splashEffect;
    [SerializeField] private float force;
    [SerializeField] private float splineNodes;

    private bool _toUpdate = true;
    private float _totalVolume;
    [SerializeField] private float _filledVolume;

    private static readonly int GravityDirectionId = Shader.PropertyToID("_GravityDirection");
    private static readonly int SurfaceLevelId = Shader.PropertyToID("_SurfaceLevel");

    private Vector3 ArcPosition(float t)
    {
        Vector3 pos = bottleneckCollider.transform.position + (bottleneckCollider.transform.up * (force * t)) +
                      Physics.gravity * (0.5f * (t*t));

        return pos;
    }

    private void UpdateSpline() {

        // adjust the number of nodes in the spline.
        while (spline.nodes.Count < splineNodes) {
            spline.AddNode(new SplineNode(Vector3.zero, Vector3.zero));
        }
        while (spline.nodes.Count > splineNodes && spline.nodes.Count > 2) {
            spline.RemoveNode(spline.nodes.Last());
        }

        print($"{spline.nodes.Count} Here is the count");
    }

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
        if (_toUpdate)
        {
            UpdateSpline();
            _toUpdate = false;
        }

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
                // if (splashEffect.aliveParticleCount <= 0)
                // {
                //     splashEffect.Play();
                // }

                spline.gameObject.SetActive(true);
                PourLiquid();
            }
            else
            {
                spline.gameObject.SetActive(false);

                if (splashEffect.aliveParticleCount > 0)
                {
                    splashEffect.Stop();
                }
            }


        }
        else
        {
            liquidRenderer.enabled = false;
            spline.gameObject.SetActive(false);
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

        for (int i = 0; i < splineNodes; i++)
        {
            spline.nodes[i].Position = ArcPosition(i * (1f/splineNodes));
        }

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

        bool firstSplashHit = false;
        foreach (var hit in raycastHits.OrderBy(x => x.distance))
        {
            if (hit.collider is null || hit.collider == bottleneckCollider) continue;

            if (!firstSplashHit)
            {
                splashEffect.transform.position = hit.point;
                firstSplashHit = true;
            }

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
        var shouldPour = ShouldPour(surfaceLevel.y);

        Gizmos.color = shouldPour
            ? Color.green
            : Color.red;
        var center = liquidRenderer.bounds.center;
        center.y = surfaceLevel.y;
        Gizmos.DrawSphere(center, 0.01f);

        Gizmos.color = Color.magenta;

        for (int i = 1; i < 50; i++)
        {
             var x = ArcPosition(i* (1f/50f));
            Gizmos.DrawSphere(x, 0.01f);
        }



        // if (shouldPour)
        // {
        //     if (TryGetContainer(bottleneckCollider.bounds.min, out var container))
        //     {
        //         Gizmos.color = Color.blue;
        //         Gizmos.DrawSphere(container.bottleneckCollider.bounds.center, 0.01f);
        //     }
        // }
    }
}