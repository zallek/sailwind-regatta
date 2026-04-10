using System.Reflection;
using UnityEngine;

namespace SailwindRegatta
{
    // Injected as a child of a Port GameObject by RaceManager.TryInjectRaceScroll.
    // Defers the actual spawn to Update() so that ShipItemScroll prefabs have loaded.
    internal class RaceScrollSpawner : MonoBehaviour
    {
        private Race             _race;
        private RaceScrollConfig _config;

        internal void Init(Race race, RaceScrollConfig config)
        {
            _race   = race;
            _config = config;
        }

        private void Update()
        {
            var prefab = FindScrollPrefab();
            if (prefab == null) return;

            Spawn(prefab);
            enabled = false;   // stop updating once spawned
        }

        private void Spawn(ShipItemScroll prefab)
        {
            var go = Object.Instantiate(prefab.gameObject);
            go.name = "RaceScroll";
            go.transform.position    = transform.parent.TransformPoint(_config.Position);
            go.transform.eulerAngles = _config.EulerAngles;
            go.transform.SetParent(null, worldPositionStays: true);  // detach from port

            // Give the clone a fresh SaveablePrefab identity so it doesn't clash with the template.
            var saveable = go.GetComponent<SaveablePrefab>();
            if (saveable != null)
            {
                var idField = typeof(SaveablePrefab).GetField(
                    "instanceId", BindingFlags.Public | BindingFlags.Instance);
                idField?.SetValue(saveable, 0);
            }

            // Pre-sold: freely pickable without any shop interaction.
            var shipItem = go.GetComponent<ShipItem>();
            if (shipItem != null)
                shipItem.sold = true;

            go.AddComponent<RaceScroll>().Init(_race);
            Plugin.Log.LogInfo($"RaceScroll spawned at port: {_config.PortName.ToPortString()}");
        }

        // Cached on first successful call — the prefab reference never changes at runtime.
        private static ShipItemScroll _scrollPrefab;

        private static ShipItemScroll FindScrollPrefab()
        {
            if (_scrollPrefab != null) return _scrollPrefab;

            var all = Resources.FindObjectsOfTypeAll<ShipItemScroll>();
            _scrollPrefab = all.Length > 0 ? all[0] : null;
            return _scrollPrefab;
        }
    }
}
