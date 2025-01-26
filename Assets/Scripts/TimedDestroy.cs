using UnityEngine;

namespace DefaultNamespace
{
    public class TimedDestroy : MonoBehaviour
    {
        public float duration = 5f;
        void Start()
        {
            Destroy(gameObject, duration);
        }
    }
}