using PsychoticLab;
using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime as a child of a Port GameObject.
    // Builds the full NPC visual and UI hierarchy, then wires the reusable
    // controller components (UIToggler, RaceLeaderboardUI, LeaderboardPrefetcher,
    // RaceStartUI, RaceStartButton) together.
    internal class RaceMasterNPC : MonoBehaviour
    {
        internal void Init(Race race, RaceMaster raceMaster)
        {
            transform.localPosition = raceMaster.Position;
            transform.localEulerAngles = raceMaster.EulerAngles;

            var body = BuildCharacter(raceMaster);
            if (body == null)
                return;

            if (raceMaster.CanStartRace)
                BuildStartUI(body.transform, race);

            BuildLeaderboard(body.transform, race);
        }

        // ── Character ─────────────────────────────────────────────────────────

        // Clones the CharacterCustomizer mesh from Port.ports[config.Avatar] and
        // sets up all components needed for GoPointer interaction.
        private GameObject BuildCharacter(RaceMaster raceMaster)
        {
            if (raceMaster.Avatar < 0 || raceMaster.Avatar >= Port.ports.Length)
            {
                Plugin.Log.LogError($"RaceMasterNPC: avatar index {raceMaster.Avatar} is out of range (Port.ports.Length = {Port.ports.Length}).");
                return null;
            }

            var dude = Port.ports[raceMaster.Avatar].GetDude();
            if (dude == null)
            {
                Plugin.Log.LogError($"RaceMasterNPC: GetDude() returned null for port index {raceMaster.Avatar}.");
                return null;
            }

            var customizer = dude.GetComponentInChildren<CharacterCustomizer>();
            if (customizer == null)
            {
                Plugin.Log.LogError($"RaceMasterNPC: no CharacterCustomizer found on dude at port index {raceMaster.Avatar}.");
                return null;
            }

            var go = Instantiate(customizer.gameObject, transform, false);
            go.name = "Character";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            return go;
        }

        // ── Start UI ──────────────────────────────────────────────────────────

        private void BuildStartUI(Transform parent, Race race)
        {
            var container = new GameObject("RaceMasterNPCStart");
            container.transform.SetParent(parent, worldPositionStays: false);

            // Visuals: TextMesh + BoxCollider (click) + RaceStartUI + RaceStartButton.
            var uiGO = new GameObject("RaceMasterNPCStartUI");
            uiGO.transform.SetParent(container.transform, worldPositionStays: false);
            uiGO.transform.localPosition = new Vector3(0f, 1.4f, 0.3f);
            uiGO.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            var text = uiGO.AddComponent<TextMesh>();
            text.alignment = TextAlignment.Center;
            text.anchor = TextAnchor.MiddleCenter;
            text.characterSize = 0.03f;
            text.fontSize = 32;
            var clickCol = uiGO.AddComponent<BoxCollider>();
            clickCol.center = Vector3.zero;
            clickCol.size = new Vector3(0.8f, 0.3f, 0.05f);
            var startUi = uiGO.AddComponent<RaceStartUI>();
            startUi.race = race;
            var startButton = uiGO.AddComponent<RaceStartButton>();
            startButton.race = race;
            startButton.raceStartUI = startUi;
            uiGO.SetActive(false); // ProximityToggler activates on player proximity

            // Proximity trigger (3 m): shows/hides the start UI GO.
            // Deactivating the GO also disables the BoxCollider → button not clickable when hidden.
            var togglerGO = new GameObject("RaceMasterNPCStartUIToggler");
            togglerGO.transform.SetParent(container.transform, worldPositionStays: false);
            var togglerCol = togglerGO.AddComponent<SphereCollider>();
            togglerCol.isTrigger = true;
            togglerCol.radius = 3f;
            var toggler = togglerGO.AddComponent<ProximityToggler>();
            toggler.target = uiGO;
        }

        // ── Leaderboard ───────────────────────────────────────────────────────

        private void BuildLeaderboard(Transform parent, Race race)
        {
            var container = new GameObject("RaceMasterLeaderboard");
            container.transform.SetParent(parent, worldPositionStays: false);

            // Visuals: TextMesh + RaceLeaderboardUI on the same GO.
            var uiGO = new GameObject("RaceMasterLeaderboardUI");
            uiGO.transform.SetParent(container.transform, worldPositionStays: false);
            uiGO.transform.localPosition = new Vector3(-1f, 1.4f, 0.3f);
            uiGO.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            var text = uiGO.AddComponent<TextMesh>();
            text.alignment = TextAlignment.Left;
            text.anchor = TextAnchor.MiddleCenter;
            text.characterSize = 0.03f;
            text.fontSize = 32;
            var lbUi = uiGO.AddComponent<RaceLeaderboardUI>();
            uiGO.SetActive(false); // ProximityToggler activates on player proximity

            // Proximity trigger (3 m): shows/hides the leaderboard UI GO.
            var togglerGO = new GameObject("RaceMasterLeaderboardUIToggler");
            togglerGO.transform.SetParent(container.transform, worldPositionStays: false);
            var togglerCol = togglerGO.AddComponent<SphereCollider>();
            togglerCol.isTrigger = true;
            togglerCol.radius = 3f;
            var toggler = togglerGO.AddComponent<ProximityToggler>();
            toggler.target = uiGO;

            // Prefetch trigger (8 m): starts the async fetch before the player arrives.
            var prefetchGO = new GameObject("RaceMasterLeaderboardPrefetcher");
            prefetchGO.transform.SetParent(container.transform, worldPositionStays: false);
            var prefetchCol = prefetchGO.AddComponent<SphereCollider>();
            prefetchCol.isTrigger = true;
            prefetchCol.radius = 8f;
            var prefetcher = prefetchGO.AddComponent<LeaderboardPrefetcher>();
            prefetcher.race = race;
            prefetcher.raceLeaderboardUI = lbUi;
        }
    }
}
