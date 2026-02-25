using Internal.Scripts.UI.Screens.Core.View;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Pause
{
    public class PauseScreen : ScreenViewBase
    {
        [SerializeField] private Button _backToGameButton;

        private void OnEnable()
        {
            if (_backToGameButton != null)
                _backToGameButton.onClick.AddListener(OnBackToGame);
        }

        private void OnDisable()
        {
            if (_backToGameButton != null)
                _backToGameButton.onClick.RemoveListener(OnBackToGame);
        }

        private void OnBackToGame() => RaiseCloseRequested();
    }
}
