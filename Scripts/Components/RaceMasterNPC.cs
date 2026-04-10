using PsychoticLab;
using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime as a child of a Port GameObject.
    // Builds its own visual hierarchy and handles player proximity + click interaction.
    internal class RaceMasterNPC : MonoBehaviour
    {
        private Race _race;

        internal void Init(Race race, RaceMaster raceMaster)
        {
            _race = race;
            transform.localPosition = raceMaster.Position;
            transform.localEulerAngles = raceMaster.EulerAngles;

            var body = BuildCharacter(raceMaster);
            if (body == null)
                return;

            var uiGO = new GameObject("RaceMasterUI");
            uiGO.transform.SetParent(body.transform, worldPositionStays: false);
            var uiCol = uiGO.AddComponent<SphereCollider>();
            uiCol.isTrigger = true;
            uiCol.radius = 3f;
            var _ui = uiGO.AddComponent<RaceMasterUI>();
            _ui.Init(race);
            var uiController = uiGO.AddComponent<RaceMasterUIController>();
            uiController.Init(_ui);

            // Outer prefetch trigger — larger radius so the leaderboard fetch starts
            // before the player reaches the NPC, making data ready on arrival.
            var prefetchGO = new GameObject("RaceMasterLeaderboardFetcher");
            prefetchGO.transform.SetParent(body.transform, worldPositionStays: false);
            var prefetchCol = prefetchGO.AddComponent<SphereCollider>();
            prefetchCol.isTrigger = true;
            prefetchCol.radius = 8f;
            var prefetcher = prefetchGO.AddComponent<RaceMasterLeaderboardFetcher>();
            prefetcher.Init(_ui, _race);
        }

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
    }

    internal class RaceMasterUIController : MonoBehaviour
    {
        private bool _playerNearby;
        private RaceMasterUI _ui;
        private float _refreshTimer;

        internal void Init(RaceMasterUI ui)
        {
            _ui = ui;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerNearby = true;
                _refreshTimer = 0f;
                _ui.Show();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerNearby = false;
                _ui.Hide();
            }
        }

        private void Update()
        {
            if (!_playerNearby)
                return;
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _ui.Refresh();
                _refreshTimer = 0.5f;
            }
        }
    }

    // Sits on the outer-radius trigger child GameObject.
    // Starts the leaderboard prefetch when the player enters range.
    internal class RaceMasterLeaderboardFetcher : MonoBehaviour
    {
        private LeaderboardEntryResponse[] _leaderboardData;
        private bool _leaderboardFetching;

        private RaceMasterUI _ui;
        private Race _race;

        internal void Init(RaceMasterUI ui, Race race)
        {
            _ui = ui;
            _race = race;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                FetchLeaderboard();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _leaderboardData = null;
                _leaderboardFetching = false;
            }
        }

        private async void FetchLeaderboard()
        {
            if (_leaderboardData != null || _leaderboardFetching)
                return;

            _leaderboardFetching = true;
            _leaderboardData = await SupabaseClient.GetLeaderboardAsync(_race.Id);
            _leaderboardFetching = false;
            _ui.LeaderboardData = _leaderboardData;
        }
    }
}
