using cakeslice;
using UnityEngine;

namespace SailwindRegatta
{
    // Click-to-start/abort button placed on the RaceMasterNPCStartUI GameObject.
    // Extends GoPointerButton so GoPointer's raycast can activate it on look + click.
    // The BoxCollider (for raycast hit detection) is added by RaceMasterNPC.
    internal class RaceStartButton : GoPointerButton
    {
        [SerializeField]
        public Race race;

        [SerializeField]
        public RaceStartUI raceStartUI;

        private TextMesh _text;
        private Outline _outline;

        public override void Start()
        {
            base.Start(); // adds the Outline component
            _text = GetComponent<TextMesh>();
            _outline = GetComponent<Outline>();
        }

        public override void ExtraLateUpdate()
        {
            if (_outline != null)
                _outline.enabled = false;
            _text.fontStyle = IsLookedAt() ? FontStyle.Bold : FontStyle.Normal;
        }

        public override void OnActivate()
        {
            if (RaceManager.Instance == null)
                return;
            UISoundPlayer.instance.PlayUISound(UISounds.buttonClick, 1f, 1.2f);
            if (RaceManager.Instance.ActiveRun == null)
                RaceManager.Instance.StartRace(race);
            else
                RaceManager.Instance.AbortRace();
            raceStartUI.Refresh();
        }
    }
}
