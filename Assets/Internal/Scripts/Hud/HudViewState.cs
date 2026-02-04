namespace Internal.Scripts.Hud
{
    public readonly struct HudViewState
    {
        public readonly bool ShowStartMove;
        public readonly bool ShowCancelMove;
        public readonly bool ShowEnterCity;

        public HudViewState(bool showStartMove, bool showCancelMove, bool showEnterCity)
        {
            ShowStartMove = showStartMove;
            ShowCancelMove = showCancelMove;
            ShowEnterCity = showEnterCity;
        }
    }
}
