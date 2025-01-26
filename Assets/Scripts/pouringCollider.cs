using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class PouringCollider : MonoBehaviour
    {
        public LiquidContainer bottleContainer;

        private void OnTriggerEnter(Collider other)
        {
            bottleContainer = GameObject.FindWithTag("activeBottle").GetComponent<LiquidContainer>();

        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Bar"))
            {
                var thingToPourInto = other.GetComponentInParent<LiquidContainer>();
                if (thingToPourInto is not null)
                {

                    var volumeToAdd = Mathf.Min(thingToPourInto._totalVolume - thingToPourInto._filledVolume, bottleContainer.mlPerSecond * Time.deltaTime);
                    thingToPourInto._filledVolume += volumeToAdd;

                }
            }

        }

        // private void OnTriggerExit(Collider other)
        // {
        //     print("Exit trigger");
        //     if (other.gameObject.layer == LayerMask.NameToLayer("Bar"))
        //     {
        //         print("Exit trigger Bar");
        //         var container = other.GetComponentInParent<LiquidContainer>();
        //         if (container == liquidContainer)
        //         {
        //             liquidContainer = null;
        //             print("Left liquid container");
        //         }
        //
        //     }
        // }

    }
}