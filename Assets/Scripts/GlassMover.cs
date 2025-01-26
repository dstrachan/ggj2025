using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class GlassMover : MonoBehaviour
    {
        public Vector3 target;
        public Vector3 startPos;
        public float glassSpeed;

        private float _timeSinceStarted;

        public bool started;
        public AnimationCurve moveCurve;

        private void Update()
        {
            if (started)
            {

                _timeSinceStarted += Time.deltaTime;
                float t1 = moveCurve.Evaluate(_timeSinceStarted * glassSpeed);
                transform.localPosition = Vector3.Lerp(startPos, target, t1);

                // If the object has arrived, stop the coroutine
                if (Vector3.Distance(transform.position, target) < 0.01f)
                {
                    started = false;
                }
            }

        }
    }
}