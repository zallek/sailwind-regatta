using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime as a child of a Port GameObject.
    // Builds its own visual hierarchy and handles player proximity + click interaction.
    internal class RaceMasterNPC : MonoBehaviour
    {
        private Race _race;
        private RaceMasterButton _button;
        private bool _playerNearby;

        internal void Init(Race race, RaceMasterConfig config)
        {
            _race = race;
            transform.localPosition = config.Position;
            transform.localEulerAngles = config.EulerAngles;

            // Proximity trigger — large sphere so the HUD text appears before the player
            // is right on top of the NPC. Lives on this GameObject so OnTriggerEnter/Exit fire here.
            var trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3f;

            // Capsule body: provides MeshRenderer (required by GoPointerButton) and
            // CapsuleCollider (for GoPointer raycast hit detection).
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(transform, worldPositionStays: false);
            capsule.transform.localPosition = Vector3.zero;
            capsule.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);

            _button = capsule.AddComponent<RaceMasterButton>();
            _button.npc = this;
            _button.description = "Race Master";

            // Name label floating above the NPC's head.
            var label = new GameObject("Label");
            label.transform.SetParent(capsule.transform, worldPositionStays: false);
            label.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var tm = label.AddComponent<TextMesh>();
            tm.text = "Race Master";
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.characterSize = 0.1f;
            tm.fontSize = 50;

            RefreshLookText();

            Plugin.Log.LogInfo($"RaceMasterNPC built for race: {race.DisplayName}");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerNearby = true;
            RefreshLookText();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerNearby = false;
        }

        private void Update()
        {
            // Refresh every frame while the player is nearby so the text stays
            // correct if race state changes while they stand next to the NPC.
            if (_playerNearby)
                RefreshLookText();
        }

        private void RefreshLookText()
        {
            if (_button == null) return;
            _button.lookText = RaceManager.Instance?.ActiveRun == null
                ? "start the race"
                : "abort the race";
        }

        internal void Activate()
        {
            RefreshLookText();
            if (RaceManager.Instance.ActiveRun == null)
                RaceManager.Instance.StartRace(_race);
            else
                RaceManager.Instance.AbortRace("Aborted by Race Master.");
        }
    }
}
