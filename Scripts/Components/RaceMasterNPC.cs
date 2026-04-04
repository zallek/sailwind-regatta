using PsychoticLab;
using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime as a child of a Port GameObject.
    // Builds its own visual hierarchy and handles player proximity + click interaction.
    internal class RaceMasterNPC : MonoBehaviour
    {
        private Race _race;
        private RaceMasterUI _ui;
        private bool _playerNearby;

        internal void Init(Race race, RaceMaster raceMaster)
        {
            _race = race;
            transform.localPosition = raceMaster.Position;
            transform.localEulerAngles = raceMaster.EulerAngles;

            // Proximity trigger — fires OnTriggerEnter/Exit on this GameObject.
            var trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3f;

            var body = BuildCharacter(raceMaster);
            if (body == null)
            {
                Plugin.Log.LogWarning($"RaceMasterNPC: aborting init for race '{race.DisplayName}'. Check avatar index in RaceMasterRegistry.");
                return;
            }

            var button = body.AddComponent<RaceMasterButton>();
            button.npc = this;

            var uiGO = new GameObject("RaceMasterUI");
            uiGO.transform.SetParent(body.transform, worldPositionStays: false);
            _ui = uiGO.AddComponent<RaceMasterUI>();
            _ui.Init();

            Plugin.Log.LogInfo($"RaceMasterNPC built for race: {race.DisplayName}");
        }

        // Clones the CharacterCustomizer mesh from Port.ports[config.Avatar] and
        // sets up all components needed for GoPointer interaction.
        private GameObject BuildCharacter(RaceMaster raceMaster)
        {
            if (raceMaster.Avatar < 0 || raceMaster.Avatar >= Port.ports.Length)
            {
                Plugin.Log.LogWarning($"RaceMasterNPC: avatar index {raceMaster.Avatar} is out of range (Port.ports.Length = {Port.ports.Length}).");
                return null;
            }

            var dude = Port.ports[raceMaster.Avatar].GetDude();
            if (dude == null)
            {
                Plugin.Log.LogWarning($"RaceMasterNPC: GetDude() returned null for port index {raceMaster.Avatar}.");
                return null;
            }

            var customizer = dude.GetComponentInChildren<CharacterCustomizer>();
            if (customizer == null)
            {
                Plugin.Log.LogWarning($"RaceMasterNPC: no CharacterCustomizer found on dude at port index {raceMaster.Avatar}.");
                return null;
            }

            var go = Instantiate(customizer.gameObject, transform, false);
            go.name = "Character";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            // GoPointer's raycast uses layer mask -604165 (excludes layers 2, 11-13, 16, 19).
            // Force Default layer (0) so the raycast can hit this object.
            go.layer = 0;

            // Non-trigger CapsuleCollider for GoPointer raycast hit detection.
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1f, 0f);
            col.height = 2f;
            col.radius = 0.3f;

            // GoPointerButton requires a Renderer on the same GameObject.
            // CharacterCustomizer only has SkinnedMeshRenderers on children, so add
            // an empty MeshRenderer to the root (no mesh/material — renders nothing).
            if (go.GetComponent<Renderer>() == null)
                go.AddComponent<MeshRenderer>();

            return go;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerNearby = true;
            _ui?.Show();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerNearby = false;
            _ui?.Hide();
        }

        private void Update()
        {
            // Keep text in sync if race state changes while player is nearby.
            if (_playerNearby) _ui?.Refresh();
        }

        internal void Activate()
        {
            UISoundPlayer.instance.PlayUISound(UISounds.buttonClick, 1f, 1.2f);
            if (RaceManager.Instance.ActiveRun == null)
                RaceManager.Instance.StartRace(_race);
            else
                RaceManager.Instance.AbortRace("Aborted by Race Master.");
        }
    }
}
