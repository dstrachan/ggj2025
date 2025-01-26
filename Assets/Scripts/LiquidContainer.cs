using System.Linq;
using SplineMesh;
using UnityEngine;
using UnityEngine.VFX;

public class LiquidContainer : MonoBehaviour
{
    public float Volume { get; private set; }

    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private Collider bottleneckCollider;
    [SerializeField] [Range(0, 1)] private float fillAmount = 0.5f;
    [SerializeField] private float pourSpeed = 1;
    [SerializeField] private float splashRadius = 0.025f;
    [SerializeField] private Spline spline;
    [SerializeField] private VisualEffect splashEffect;
    [SerializeField] private float force;


    private static readonly int GravityDirectionId = Shader.PropertyToID("_GravityDirection");
    private static readonly int SurfaceLevelId = Shader.PropertyToID("_SurfaceLevel");

    private Vector3 ArcPosition(float t)
    {
        Vector3 pos = bottleneckCollider.transform.position + (bottleneckCollider.transform.up * force * t) +
                      0.5f * Physics.gravity * (t*t);

        return pos;
    }

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

        spline.nodes[0].Position = bottleneckCollider.transform.position;
        // spline.nodes[0].Up = Vector3.zero;
        // spline.nodes[0].Direction = Vector3.zero;

        if (fillAmount > 0)
        {
            liquidRenderer.enabled = true;

            var surfaceLevel = GetSurfaceLevel();
            liquidRenderer.material.SetVector(SurfaceLevelId, surfaceLevel);

            if (ShouldPour(surfaceLevel.y))
            {
                if (splashEffect.aliveParticleCount <= 0)
                {
                    splashEffect.Play();
                }

                PourLiquid();
            }
            else
            {
                if (splashEffect.aliveParticleCount > 0)
                {
                    splashEffect.Stop();
                }
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

        for (int i = 0; i <= 10; i++)
        {
            ArcPosition(i * 0.1f);
        }

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

        for (int i = 1; i < 101; i++)
        {
             var x = ArcPosition(i*0.01f);
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