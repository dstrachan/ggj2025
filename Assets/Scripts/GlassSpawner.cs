using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class GlassSpawner : MonoBehaviour
    {
        public GameObject glass;
        public Transform spawnPos;

        public Transform startPoint;
        public Transform endPoint;
        public AnimationCurve moveCurve;

        public float howOftenAGlassAppearsMin = 10f;
        public float howOftenAGlassAppearsMax = 2f;
        public float howOftenAGlassAppears = 2f;
        public float glassSpeed = 1.4f;
        public float _lastGlass;

        public bool activeGlass = false;

        private void Start()
        {
            howOftenAGlassAppears = Random.Range(howOftenAGlassAppearsMin, howOftenAGlassAppearsMax);
        }

        private void Update()
        {
            if (!activeGlass && _lastGlass + howOftenAGlassAppears < Time.time)
            {
                var spawnedGlass = Instantiate(glass, transform);
                spawnedGlass.transform.localPosition = spawnPos.localPosition;
                _lastGlass = Time.time;

                var target = new Vector3(Random.Range(startPoint.localPosition.x, endPoint.localPosition.x),0,0);

                var glassMover = spawnedGlass.AddComponent<GlassMover>();
                glassMover.target = target;
                glassMover.glassSpeed = glassSpeed;
                glassMover.moveCurve = moveCurve;
                glassMover.startPos = spawnedGlass.transform.localPosition;
                glassMover.started = true;

                howOftenAGlassAppears = Random.Range(howOftenAGlassAppearsMin, howOftenAGlassAppearsMax);
                activeGlass = true;
            }

        }
    }
}