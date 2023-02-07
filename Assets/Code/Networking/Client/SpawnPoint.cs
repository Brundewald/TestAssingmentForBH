using UnityEngine;

namespace Code.Networking
{
    public class SpawnPoint: MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 1f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
        }
    }
}