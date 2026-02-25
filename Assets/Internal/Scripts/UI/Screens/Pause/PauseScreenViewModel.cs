using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.World.State;

namespace Internal.Scripts.UI.Screens.Pause
{
    public sealed class PauseScreenViewModel : ScreenViewModelBase
    {
        private readonly GameClock _gameClock;

        public PauseScreenViewModel(GameClock gameClock)
        {
            _gameClock = gameClock;
        }

        public override ScreenId Id => ScreenId.Pause;

        protected override void OnOpen(object args)
        {
            _gameClock.Pause();
        }

        protected override void OnClose()
        {
            _gameClock.Resume();
        }
    }
}
