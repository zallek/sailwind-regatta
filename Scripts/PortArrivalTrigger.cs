using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime onto port GameObjects for trigger-based checkpoint detection.
    // A SphereCollider (isTrigger=true) is added alongside this component.
    internal class PortArrivalTrigger : MonoBehaviour
    {
        public Port Port { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) {
                RaceManager.Instance?.OnPlayerEnteredPort(Port);
            }
        }
    }
}
