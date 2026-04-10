using System.Text;
using UnityEngine;

namespace SailwindRegatta
{
    // World-space UI shown near the Race Master NPC.
    // Activated/deactivated by RaceMasterNPC on player proximity.
    // Also hosts the leaderboard panel, shown only when data has been fetched.
    internal class RaceMasterUI : MonoBehaviour
    {
        private GameObject _actionTextGO;
        private TextMesh _actionText;
        private GameObject _leaderboardGO;
        private TextMesh _leaderboardText;

        private Race _race;
        public LeaderboardEntryResponse[] LeaderboardData { get; set; }

        internal void Init(Race race)
        {
            _race = race;
            transform.localPosition = new Vector3(0f, 1.5f, 0.3f);
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);

            _actionTextGO = new GameObject("ActionText");
            _actionTextGO.transform.SetParent(transform, worldPositionStays: false);
            _actionText = _actionTextGO.AddComponent<TextMesh>();
            _actionText.alignment = TextAlignment.Center;
            _actionText.anchor = TextAnchor.UpperCenter;
            _actionText.characterSize = 0.03f;
            _actionText.fontSize = 32;
            _actionTextGO.SetActive(false);

            // Non-trigger collider so GoPointer's raycast can hit the text for click detection.
            // GoPointerButton requires a Renderer — TextMesh auto-adds MeshRenderer, so it's satisfied.
            var clickCol = _actionTextGO.AddComponent<BoxCollider>();
            clickCol.center = Vector3.zero;
            clickCol.size = new Vector3(0.4f, 0.3f, 0.05f);
            _actionTextGO.AddComponent<RaceMasterButton>().Init(_race);

            // Leaderboard panel: child offset to the right, same plane as action text.
            _leaderboardGO = new GameObject("Leaderboard");
            _leaderboardGO.transform.SetParent(transform, worldPositionStays: false);
            _leaderboardGO.transform.localPosition = new Vector3(0.6f, 0f, 0f);

            _leaderboardText = _leaderboardGO.AddComponent<TextMesh>();
            _leaderboardText.alignment = TextAlignment.Left;
            _leaderboardText.anchor = TextAnchor.UpperLeft;
            _leaderboardText.characterSize = 0.03f;
            _leaderboardText.fontSize = 32;
            _leaderboardGO.SetActive(false);
        }

        internal void Show()
        {
            _actionTextGO.SetActive(true);
            Refresh();
        }

        internal void Hide()
        {
            _actionTextGO.SetActive(false);
            _leaderboardGO.SetActive(false);
        }

        internal void Refresh()
        {
            RefreshActionText();
            if (LeaderboardData != null)
            {
                _leaderboardGO.SetActive(true);
                RefreshLeaderboard();
            }
            else
            {
                _leaderboardGO.SetActive(false);
            }
        }

        private void RefreshActionText()
        {
            if (RaceManager.Instance.ActiveRun != null)
                _actionTextGO.GetComponent<TextMesh>().text = $"Abort the race\n\n{RaceManager.Instance.ActiveRun.Race.DisplayName}";
            else
                _actionTextGO.GetComponent<TextMesh>().text = $"Start the race\n\n{_race.DisplayName}";
        }

        private void RefreshLeaderboard()
        {
            if (LeaderboardData == null)
                return;

            if (LeaderboardData.Length == 0)
            {
                _leaderboardGO.GetComponent<TextMesh>().text = "Top 5 Times\n\nNo records yet";
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("Top 5 Times");
                sb.AppendLine();
                foreach (var e in LeaderboardData)
                    sb.AppendLine($"{e.rank}. {(e.player_name.Length > 15 ? e.player_name.Substring(0, 15) : e.player_name)}  {FormatDuration(e.duration)}");
                _leaderboardGO.GetComponent<TextMesh>().text = sb.ToString().TrimEnd();
            }
        }

        private static string FormatDuration(int seconds)
        {
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m}:{s:D2}";
        }
    }


    internal class RaceMasterButton : GoPointerButton
    {
        private Race _race;

        internal void Init(Race race)
        {
            _race = race;
            forceDisableRedOutline = true;
        }

        public override void OnActivate()
        {
            UISoundPlayer.instance.PlayUISound(UISounds.buttonClick, 1f, 1.2f);
            if (RaceManager.Instance.ActiveRun == null)
                RaceManager.Instance.StartRace(_race);
            else
                RaceManager.Instance.AbortRace("Aborted by Race Master.");
        }
    }
}
