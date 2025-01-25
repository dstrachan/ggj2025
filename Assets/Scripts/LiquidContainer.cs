using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class LiquidContainer : MonoBehaviour
{
    public float Volume { get; private set; }

    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] [Range(0, 1)] private float fillAmount = 0.5f;
    [SerializeField] private float bottleneckRadius = 0.1f;
    [SerializeField] private float pourSpeed = 1;

    private Mesh liquidMesh;

    private static readonly int GravityDirectionId = Shader.PropertyToID("_GravityDirection");
    private static readonly int SurfaceLevelId = Shader.PropertyToID("_SurfaceLevel");

    private void Awake()
    {
        Debug.Assert(liquidRenderer is not null);
        Debug.Assert(liquidMaterial is not null);

        liquidMesh = liquidRenderer.GetComponent<MeshFilter>().sharedMesh;
        var boundsSize = liquidMesh.bounds.size;
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

            var bottleneckPlane = new Plane(transform.up, liquidMesh.bounds.max.y * transform.lossyScale.y);
            var bottleneckPos = bottleneckPlane.normal * bottleneckPlane.distance + transform.position;
            var surfacePlane = new Plane(Vector3.up, surfaceLevel.y - transform.position.y);
            var bottleneckRadiusWorld = bottleneckRadius * transform.lossyScale.magnitude;

            var overflows = GetPlaneIntersection(out var overflowPoint, bottleneckPlane, surfacePlane);
            overflowPoint += transform.position;

            if (overflows)
            {
                // Check if the overflow point is inside bottleneck radius
                var insideBottleneck = Vector3.Distance(overflowPoint, bottleneckPos) < bottleneckRadiusWorld;
                if (insideBottleneck)
                {
                    PourFromMinPoint(bottleneckPlane, bottleneckPos, bottleneckRadiusWorld);
                    return;
                }
            }

            if (bottleneckPos.y < overflowPoint.y)
            {
                // Container is upside down
                var dot = Vector3.Dot(bottleneckPlane.normal, surfacePlane.normal);
                if (dot < 0)
                {
                    Pour(bottleneckPos, bottleneckRadiusWorld);
                }
                else
                {
                    var dist = liquidRenderer.bounds.SqrDistance(overflowPoint);
                    var inBounding = dist < 0.0001f;
                    if (inBounding)
                    {
                        PourFromMinPoint(bottleneckPlane, bottleneckPos, bottleneckRadiusWorld);
                    }
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

    private void PourFromMinPoint(Plane bottleneckPlane, Vector3 bottleneckPos, float bottleneckRadiusWorld)
    {
        var bottleneckSlope = Vector3.Cross(Vector3.Cross(Vector3.up, bottleneckPlane.normal), bottleneckPlane.normal)
            .normalized;
        var minPoint = bottleneckPos + bottleneckSlope * bottleneckRadiusWorld;
        Pour(minPoint, bottleneckRadiusWorld);
    }

    private void Pour(Vector3 point, float bottleneckRadiusWorld)
    {
        var howLow = Vector3.Dot(Vector3.up, transform.up);
        var flowScale = 1 - (howLow + 1) * 0.5f + 0.2f;

        var liquidStep = bottleneckRadiusWorld * pourSpeed * Time.deltaTime * flowScale;
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

    private static bool GetPlaneIntersection(out Vector3 point, Plane plane1, Plane plane2)
    {
        var lineVec = Vector3.Cross(plane1.normal, plane2.normal);
        var dir = Vector3.Cross(plane2.normal, lineVec);
        var numerator = Vector3.Dot(plane1.normal, dir);

        if (Mathf.Abs(numerator) > 0.000001f)
        {
            var pos1 = plane1.normal * plane1.distance;
            var pos2 = plane2.normal * plane2.distance;

            var plane1ToPlane2 = pos1 - pos2;
            var t = Vector3.Dot(plane1.normal, plane1ToPlane2) / numerator;
            point = pos2 + t * dir;
            return true;
        }

        point = Vector3.zero;
        return false;
    }
}