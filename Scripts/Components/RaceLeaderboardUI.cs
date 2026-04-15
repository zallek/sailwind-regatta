using System.Text;
using UnityEngine;

namespace SailwindRegatta
{
    // Populates and shows/hides a TextMesh leaderboard panel.
    // The TextMesh (and its MeshRenderer) must be on the same GameObject.
    // Data is supplied by LeaderboardPrefetcher via SetData().
    // Visibility is driven by the parent UIToggler via SetActive() on this GO:
    //   OnEnable  → refresh (show text if data is ready)
    //   OnDisable → hide renderer (clean up when player walks away)
    [RequireComponent(typeof(TextMesh))]
    internal class RaceLeaderboardUI : MonoBehaviour
    {
        private TextMesh _text;
        private MeshRenderer _renderer;
        private LeaderboardEntryResponse[] _leaderboardData;

        internal LeaderboardEntryResponse[] LeaderboardData
        {
            get => _leaderboardData;
            set
            {
                _leaderboardData = value;

                if (_leaderboardData == null)
                {
                    // Set by LeaderboardPrefetcher when the player exits the outer zone.
                    _renderer.enabled = false;
                }
                else
                {
                    // Set by LeaderboardPrefetcher when async fetch completes.
                    _renderer.enabled = true;
                    if (gameObject.activeInHierarchy)
                        Refresh();
                }
            }
        }

        private void Awake()
        {
            _text = GetComponent<TextMesh>();
            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;
        }

        private void OnEnable() => Refresh();

        private void Refresh()
        {
            if (_leaderboardData == null)
            {
                _renderer.enabled = false;
                return;
            }

            _renderer.enabled = true;

            var sb = new StringBuilder();
            sb.AppendLine("Leaderboard");
            sb.AppendLine();
            if (_leaderboardData.Length == 0)
            {
                sb.AppendLine("No records yet");
            }
            else
            {
                foreach (var e in _leaderboardData)
                {
                    string name = e.player_name.Length > 15 ? e.player_name.Substring(0, 15) : e.player_name;
                    sb.AppendLine($"{e.rank}. {name}  {TimeUtils.FormatDuration(e.duration_minutes)}");
                }
            }
            _text.text = sb.ToString().TrimEnd('\r', '\n');
        }
    }
}
