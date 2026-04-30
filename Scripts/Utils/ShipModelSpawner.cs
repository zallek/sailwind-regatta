using UnityEngine;

namespace SailwindRegatta
{
    internal static class ShipModelSpawner
    {
        internal static void SpawnForPlayer()
        {
            if (GameState.currentBoat == null)
            {
                Debug.LogWarning("[ShipModelSpawner] No current boat.");
                return;
            }
            var boatRefs = GameState.currentBoat.GetComponentInParent<BoatRefs>();
            if (boatRefs == null)
            {
                Debug.LogWarning("[ShipModelSpawner] Could not find BoatRefs on current boat.");
                return;
            }

            var go = new GameObject("ShipModelItem");
            go.transform.position = GameState.currentBoat.position + Vector3.up * 1.5f;

            go.AddComponent<Rigidbody>();
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(0.3f, 0.08f, 0.15f);

            go.AddComponent<MeshRenderer>();

            var item = go.AddComponent<ShipModelItem>();
            item.BuildVisual(boatRefs);

            var pointer = Object.FindObjectOfType<GoPointer>();
            if (pointer != null)
                pointer.PickUpItem(item);
        }
    }
}
