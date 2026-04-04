using UnityEngine;

namespace SailwindRegatta
{
    // World-space action text shown near the Race Master NPC.
    // Activated/deactivated by RaceMasterNPC on player proximity.
    internal class RaceMasterUI : MonoBehaviour
    {
        private TextMesh _text;

        internal void Init()
        {
            transform.localPosition = new Vector3(0f, 1.5f, 0.3f);
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);

            _text = gameObject.AddComponent<TextMesh>();
            _text.alignment = TextAlignment.Center;
            _text.anchor = TextAnchor.MiddleCenter;
            _text.characterSize = 0.03f;
            _text.fontSize = 32;

            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        internal void Hide() => gameObject.SetActive(false);

        internal void Refresh()
        {
            _text.text = RaceManager.Instance?.ActiveRun == null
                ? "Start the race"
                : "Sbort the race";
        }
    }
}
