using System;
using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public class Pound : MonoBehaviour
    {
        public AudioSource clink;
        public Rigidbody rb;

        void Start()
        {
            StartCoroutine(Freeze(2));
        }

        private IEnumerator Freeze(float delay)
        {

            yield return new WaitForSecondsRealtime(delay);
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("tipJar"))
            {
                clink.PlayOneShot(clink.clip);
            }
        }
    }
}