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
            if (other.gameObject.layer == LayerMask.NameToLayer("FillGlass"))
            {
                var thingToPourInto = other.GetComponentInParent<LiquidContainer>();
                if (thingToPourInto is not null)
                {
                    thingToPourInto.FilledVolume += bottleContainer.mlPerSecond * Time.deltaTime;
                }
            }
        }
    }
}