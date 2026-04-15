using UnityEngine;

namespace SailwindRegatta
{
    // Populates a TextMesh with start/abort text for a race.
    // The TextMesh must be on the same GameObject.
    // Visibility is driven by the parent UIToggler via SetActive() on this GO:
    //   OnEnable → immediate refresh + periodic refresh timer reset
    //   Active   → Update() refreshes every 0.5s (tracks ActiveRun changes)
    [RequireComponent(typeof(TextMesh))]
    internal class RaceStartUI : MonoBehaviour
    {
        [SerializeField]
        public Race race;

        private TextMesh _text;

        private void Awake()
        {
            _text = GetComponent<TextMesh>();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (RaceManager.Instance == null)
                return;
            if (RaceManager.Instance.ActiveRun != null)
                _text.text = $"Abort the race\n\n{RaceManager.Instance.ActiveRun.Race.DisplayName}";
            else
                _text.text = $"Start the race\n\n{race.DisplayName}";
        }
    }
}
