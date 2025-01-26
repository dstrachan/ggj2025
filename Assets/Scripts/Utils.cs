using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class Utils : MonoBehaviour
    {

        public static RaycastHit? ProjectPointToGround(Vector3 point)
        {
            var didHit = Physics.Raycast(
                point,
                Vector3.down,
                out var hit,
                0.01f,
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