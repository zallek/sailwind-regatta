namespace SailwindRegatta
{
    // GoPointerButton subclass that delegates activation to RaceMasterNPC.
    // lookText is set dynamically by RaceMasterNPC based on current race state.
    // GoPointerButton.Start() auto-adds the Outline component (highlight-on-look).
    internal class RaceMasterButton : GoPointerButton
    {
        internal RaceMasterNPC npc;

        public override void OnActivate()
        {
            npc?.Activate();
        }
    }
}
