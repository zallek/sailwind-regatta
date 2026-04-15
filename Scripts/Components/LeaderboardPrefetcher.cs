using UnityEngine;

namespace SailwindRegatta
{
    // Sits on an outer trigger zone (large radius) to start the leaderboard fetch
    // before the player reaches the NPC, so data is ready on arrival.
    // The Collider radius is configured by RaceMasterNPC — this component only
    // reacts to the trigger events and feeds data to RaceLeaderboardUI.
    [RequireComponent(typeof(Collider))]
    internal class LeaderboardPrefetcher : MonoBehaviour
    {
        [SerializeField]
        public Race race;

        [SerializeField]
        public RaceLeaderboardUI raceLeaderboardUI;

        private bool _fetching;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                FetchLeaderboard();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            _fetching = false;
            raceLeaderboardUI.LeaderboardData = null;
        }

        private async void FetchLeaderboard()
        {
            if (Plugin.Session == null || _fetching)
                return;

            _fetching = true;
            var data = await SupabaseClient.GetLeaderboardAsync(Plugin.Session, race.Id);
            _fetching = false;
            raceLeaderboardUI.LeaderboardData = data;
        }
    }
}
