using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace SailwindRegatta
{
    // Attached to a cloned ShipItemScroll placed near the Race Master NPC.
    // Suppresses the scroll's built-in texture page and renders dynamic content
    // via IMGUI when the player is holding it.
    internal class RaceScroll : MonoBehaviour
    {
        private enum LeaderboardState { Loading, Ready }

        private Race                    _race;
        private ShipItemScroll          _scroll;
        private Renderer                _pageRenderer;
        private bool                    _held;
        private int                     _currentPage;    // 0 = route, 1 = leaderboard
        private LeaderboardState        _leaderboardState = LeaderboardState.Loading;
        private bool                    _leaderboardFetchStarted;
        private LeaderboardEntryResponse[] _leaderboard;

        internal void Init(Race race)
        {
            _race   = race;
            _scroll = GetComponent<ShipItemScroll>();

            // Grab the private 'page' Renderer field once so we can suppress it while held.
            var fi = typeof(ShipItemScroll).GetField(
                "page",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _pageRenderer = fi?.GetValue(_scroll) as Renderer;
        }

        // Called by ShipItemScrollPatches postfix on OnPickup.
        internal void OnScrollPickup()
        {
            _held         = true;
            _currentPage  = 0;

            // Suppress the game's built-in page texture so it doesn't show through.
            if (_pageRenderer != null)
                _pageRenderer.enabled = false;

            // Kick off the leaderboard fetch once per instance lifetime.
            if (!_leaderboardFetchStarted)
            {
                _leaderboardFetchStarted = true;
                _ = LoadLeaderboardAsync();
            }
        }

        // Called by ShipItemScrollPatches postfix on OnDrop.
        internal void OnScrollDrop()
        {
            _held = false;
            // Re-enable the game renderer so the closed scroll looks normal on the ground.
            if (_pageRenderer != null)
                _pageRenderer.enabled = true;
        }

        // Called by ShipItemScrollPatches prefix on OnScroll (replaces ShipItemScroll's FlipPage).
        // Positive input = scroll up (previous page), negative = scroll down (next page),
        // matching ShipItemScroll's own convention.
        internal void OnScroll(float input)
        {
            int dir    = input > 0f ? -1 : 1;
            int target = _currentPage + dir;

            // Don't allow advancing to the leaderboard page until data is ready.
            if (target == 1 && _leaderboardState != LeaderboardState.Ready)
                return;

            _currentPage = Mathf.Clamp(target, 0, 1);
        }

        private async Task LoadLeaderboardAsync()
        {
            var entries = await SupabaseClient.GetLeaderboardAsync(_race.Id);

            // On success or failure, mark Ready. On failure entries is null → treat as empty.
            _leaderboard      = entries ?? new LeaderboardEntryResponse[0];
            _leaderboardState = LeaderboardState.Ready;
        }

        private void OnGUI()
        {
            if (!_held) return;

            var windowRect = new Rect(
                (Screen.width  - 500) / 2f,
                (Screen.height - 400) / 2f,
                500f, 400f);

            GUI.Window(GetInstanceID(), windowRect, DrawScrollWindow, string.Empty);
        }

        private void DrawScrollWindow(int id)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Route"))
                _currentPage = 0;

            GUI.enabled = _leaderboardState == LeaderboardState.Ready;
            if (GUILayout.Button("Leaderboard") && _leaderboardState == LeaderboardState.Ready)
                _currentPage = 1;
            GUI.enabled = true;

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (_currentPage == 0) DrawRoutePage();
            else                   DrawLeaderboardPage();
        }

        private void DrawRoutePage()
        {
            var checkpoints = _race.Checkpoints;
            int n           = checkpoints.Length;

            GUILayout.Label(_race.DisplayName);
            GUILayout.Space(8);
            GUILayout.Label("Route:");
            GUILayout.Label("  Start:  " + checkpoints[0].ToDisplayName());

            for (int i = 1; i < n - 1; i++)
                GUILayout.Label($"  Stop {i}: " + checkpoints[i].ToDisplayName());

            GUILayout.Label("  Finish: " + checkpoints[n - 1].ToDisplayName());
        }

        private void DrawLeaderboardPage()
        {
            GUILayout.Label("Top 5 — " + _race.DisplayName);
            GUILayout.Space(8);

            if (_leaderboard == null || _leaderboard.Length == 0)
            {
                GUILayout.Label("No finished runs yet.");
                return;
            }

            GUILayout.Label("#    Name                 Time");
            GUILayout.Label(new string('-', 40));
            foreach (var entry in _leaderboard)
                GUILayout.Label($"{entry.rank,-5}{entry.player_name,-21}{FormatDuration(entry.duration)}");
        }

        private static string FormatDuration(int seconds)
        {
            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            int s = seconds % 60;
            return h > 0
                ? $"{h}h {m:D2}m {s:D2}s"
                : $"{m}m {s:D2}s";
        }
    }
}
