using UnityEngine;

namespace SailwindRegatta
{
    internal static class BoatTypeUtils
    {
        internal static int? TryGetBoatTypeIdFromRudder(Rudder rudder)
        {
            if (rudder == null || rudder.shipRigidbody == null)
                return null;

            var save = rudder.shipRigidbody.gameObject.GetComponent<SaveableObject>();
            if (save == null)
                return null;

            return save.sceneIndex;
        }
    }
}
