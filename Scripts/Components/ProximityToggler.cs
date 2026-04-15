using UnityEngine;

namespace SailwindRegatta
{
    // Generic proximity toggler. Shows/hides a target GameObject when the player
    // enters or exits the trigger zone. The Collider (radius, shape) is configured
    // by whoever creates this GO — this component only reacts to the trigger events.
    [RequireComponent(typeof(Collider))]
    internal class ProximityToggler : MonoBehaviour
    {
        [SerializeField]
        public GameObject target;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                target.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                target.SetActive(false);
        }
    }
}
