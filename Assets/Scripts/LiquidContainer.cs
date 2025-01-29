using System;
using System.Linq;
using DefaultNamespace;
using SplineMesh;
using UnityEngine;
using UnityEngine.VFX;

public class LiquidContainer : MonoBehaviour
{
    [SerializeField] private MeshRenderer liquidRenderer;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private Collider bottleneckCollider;
    [SerializeField] private Collider liquidLevelCollider;
    [SerializeField] public float mlPerSecond = 50;
    [SerializeField] internal Spline spline;
    [SerializeField] private VisualEffect splashEffect;
    [SerializeField] private VisualEffect foamingEffect;
    [SerializeField] private float force;
    [SerializeField] private int extraPourLength = 4;
    private int splineNodes = 4;
    [SerializeField] private bool pours;
    [SerializeField] private bool prefill;
    [SerializeField] private bool neckMoves;

    [SerializeField] private float sphereSize;
    [SerializeField] private float maxRayLength;
    [SerializeField] private int pointsForSamplingLength;


    public AudioSource fillAudio;
    public AudioSource filledAudio;

    private Vector3[] _pointsForSample;

    private bool _updateSpline;


    internal float _totalVolume;
    [SerializeField] internal float _startVolume;
    [SerializeField] internal float volumeMultiplier;
    [SerializeField] internal float _emptyAtVolume;
    [SerializeField] private float _filledVolume;

    private float _lastFilledAmount;
    private float startedFillingTime;
    private bool containerFull;

    public float FilledVolume
    {
        get => _filledVolume;
        set
        {
            if (!_fillAudioNull)
            {
                var audioSource = fillAudio;

                print($"Filling not null {_lastFilledAmount}, {_filledVolume}, {containerFull}, {containerFull}, {audioSource.isPlaying}");

                if (!containerFull && !audioSource.isPlaying && _lastFilledAmount < _filledVolume)
                {
                    print("Filling");
                    startedFillingTime = Time.time;

                    audioSource.Play();

                }

            }

            _filledVolume = Mathf.Clamp(value, _emptyAtVolume, _totalVolume);
            if (_filledVolume >= _totalVolume)
            {
                if (!_fillAudioNull)
                {
                    fillAudio.Stop();
                }

                if (!_filledAudioNull)
                {
                    filledAudio.Play();
                }

                containerFull = true;
                OnContainerFull?.Invoke();
            }
            else if (_filledVolume <= _emptyAtVolume)
            {
                print("Container is empty");
                OnContainerEmpty?.Invoke();
            }


            startedFillingTime = Time.time;

        }
    }

    public event Action OnContainerFull;
    public event Action OnContainerEmpty;

    private static readonly int GravityDirectionId = Shader.PropertyToID("_GravityDirection");
    private static readonly int SurfaceLevelId = Shader.PropertyToID("_SurfaceLevel");
    private bool _fillAudioNull;
    private bool _filledAudioNull;
    private bool _isfoamingEffectNotNull;

    private void Start()
    {
        _isfoamingEffectNotNull = foamingEffect != null;
    }

    private Vector3 ArcPosition(float t)
    {
        Vector3 pos = bottleneckCollider.transform.position + (bottleneckCollider.transform.up * (force * t)) +
                      Physics.gravity * (0.5f * (t * t));

        return pos;
    }

    private void UpdateSpline()
    {
        // adjust the number of nodes in the spline.
        while (spline.nodes.Count < splineNodes)
        {
            spline.AddNode(new SplineNode(Vector3.zero, Vector3.zero));
        }

        while (spline.nodes.Count > splineNodes && spline.nodes.Count > 2)
        {
            spline.RemoveNode(spline.nodes.Last());
        }
    }

    private void Awake()
    {
        if (fillAudio == null) _fillAudioNull = true;
        if (filledAudio == null) _filledAudioNull = true;

        _updateSpline = pours;
        _pointsForSample = new Vector3[pointsForSamplingLength];

        Debug.Assert(liquidRenderer is not null);
        Debug.Assert(liquidMaterial is not null);


        var boundsSize = liquidRenderer.localBounds.size;
        var scale = transform.lossyScale;

        _totalVolume = boundsSize.x * boundsSize.y * boundsSize.z * scale.x * scale.y * scale.y *
                       volumeMultiplier; // Make sure the big bottle is 750ml

        if (prefill)
        {
            _filledVolume = _startVolume;
        }

        liquidRenderer.sharedMaterial = liquidMaterial;

        liquidMaterial.SetVector(GravityDirectionId, Vector3.down);
    }

    private void Update()
    {
        if (pours && _updateSpline)
        {
            UpdateSpline();
            _updateSpline = false;
        }

        if (!_fillAudioNull)
        {
            if (Time.time - startedFillingTime > 0.05f)
            {
                _lastFilledAmount = _filledVolume;
                print("Stopped Filling");

                fillAudio.Pause();
            }
        }

        if (_filledVolume > _emptyAtVolume)
        {
            if (_isfoamingEffectNotNull && foamingEffect.aliveParticleCount <= 0)
            {
                foamingEffect.Play();
            }

            liquidRenderer.enabled = true;

            var surfaceLevel = GetSurfaceLevel();
            liquidRenderer.material.SetVector(SurfaceLevelId, surfaceLevel);

            if (neckMoves)
            {
                var bottleNeckPos = liquidLevelCollider.transform.position;
                bottleNeckPos.y = surfaceLevel.y;
                liquidLevelCollider.transform.position = bottleNeckPos;
            }

            if (pours)
            {
                if (ShouldPour(surfaceLevel.y))
                {
                    if (splashEffect.aliveParticleCount <= 0)
                    {
                        splashEffect.Play();
                    }


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
        }
        else
        {
            liquidRenderer.enabled = false;


            if (pours)
            {
                if (splashEffect.aliveParticleCount > 0)
                {
                    splashEffect.Stop();
                }

                spline.gameObject.SetActive(false);
            }
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
        FilledVolume -= mlPerSecond * Time.deltaTime;

        if (pours)
        {
            var lastPos = -1;
            var maxIter = pointsForSamplingLength;
            var i = 0;
            var firstHit = false;
            while (i < maxIter)
            {
                var x = ArcPosition(i * (1f / pointsForSamplingLength));
                var x2 = ArcPosition(i + 1 * (1f / pointsForSamplingLength));

                var toGround = Utils.ProjectPointToGround(x, x, x2, sphereSize, maxRayLength);

                _pointsForSample[i] = x;

                if (!firstHit && toGround != null)
                {
                    maxIter = i + extraPourLength;
                    firstHit = true;
                }

                lastPos = i;
                i++;
            }

            var splinePoints = lastPos / 4;

            spline.nodes[0].Position = _pointsForSample[0];
            spline.nodes[1].Position = _pointsForSample[splinePoints * 1];
            spline.nodes[2].Position = _pointsForSample[splinePoints * 3];
            spline.nodes[3].Position = _pointsForSample[lastPos - 1];

            splashEffect.transform.position = _pointsForSample[lastPos - 1];
        }

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

        if (pours)
        {
            var lastPos = -1;
            var maxIter = pointsForSamplingLength;
            var i = 0;
            var firstHit = false;
            while (i < maxIter)
            {
                var x = ArcPosition(i * (1f / pointsForSamplingLength));
                var x2 = ArcPosition(i + 1 * (1f / pointsForSamplingLength));

                var toGround = Utils.ProjectPointToGround(x, x, x2, sphereSize, maxRayLength);

                Gizmos.DrawSphere(x, 0.01f);

                if (!firstHit && toGround != null)
                {
                    maxIter = i + extraPourLength;
                    firstHit = true;
                }

                lastPos = i;
                i++;
            }
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