using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class Utils : MonoBehaviour
    {

        public static RaycastHit? ProjectPointToGround(Vector3 point, Vector3 from, Vector3 to, float sphereSize, float maxRayLength)
        {

            var didHit = Physics.Raycast(
                new Ray(point, (to - from).normalized),
                out var hit,
                maxRayLength,
                LayerMask.GetMask("PourCanHit"),
                QueryTriggerInteraction.Collide);

            if (didHit)
            {
                return hit;
            }
            else
            {
                return null;
            }
        }


    }
}