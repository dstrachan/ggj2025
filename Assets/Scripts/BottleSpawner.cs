using System;
using System.Collections;
using SplineMesh;
using UnityEngine;

namespace DefaultNamespace
{
    public class BottleSpawner : MonoBehaviour
    {
        public BottleController bottleController;

        public GameObject bottlePrefab;
        public GameObject activeBottle;
        public GameObject oldBottle;
        public Vector3 oldBottlePosition;
        public Spline spline;
        public AnimationCurve moveCurve;

        public float timeToThrowAway = 5f;

        private bool _activeSwitch = false;
        private void Start()
        {
            BottlePrefabOnOnContainerEmpty();


        }

        private void BottlePrefabOnOnContainerEmpty()
        {
            if (!_activeSwitch)
            {
                if (activeBottle != null)
                {
                    print("Doing switch not null");
                    _activeSwitch = true;
                    oldBottle = activeBottle;
                    oldBottle.tag = "Untagged";
                    oldBottlePosition = oldBottle.transform.position;
                    oldBottle.GetComponentInChildren<LiquidContainer>().OnContainerEmpty -= BottlePrefabOnOnContainerEmpty;
                    var coroutine = Move(oldBottle, oldBottle.transform.position, transform.position, timeToThrowAway, true, 0.5f);
                    StartCoroutine(coroutine);
                }


                activeBottle = Instantiate(bottlePrefab, transform.position, transform.rotation);
                var liquid =  activeBottle.GetComponentInChildren<LiquidContainer>();
                liquid.spline = spline;
                liquid.OnContainerEmpty += BottlePrefabOnOnContainerEmpty;

                var coroutineNew = Move(activeBottle, activeBottle.transform.position, oldBottlePosition, timeToThrowAway, false, 1f);
                StartCoroutine(coroutineNew);

                //bottleController.SetBottleRail(0, 0.1f);
            }

        }

        private IEnumerator Move(GameObject bottleToMove, Vector3 start, Vector3 end, float time, bool destroy, float delay)
        {

            yield return new WaitForSecondsRealtime(delay);

            var elapsedTime = 0f;
            while (elapsedTime < time)
            {
                print("doing move");
                float t1 = moveCurve.Evaluate(elapsedTime / time);
                bottleToMove.transform.position = Vector3.Lerp(start, end, t1);
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            bottleToMove.transform.position = end;

            if (destroy)
            {
                Destroy(bottleToMove);
            }
            else
            {
                bottleToMove.tag = "activeBottle";
                bottleController.SetActiveBottle(activeBottle.transform);


            }

            _activeSwitch = false;
            yield return null;
        }

    }
}