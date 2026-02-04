using System;
using Internal.Scripts.Input;
using Internal.Scripts.Inventory;
using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screen.ViewModel;
using R3;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Inventory
{
    public sealed class InventoryScreenViewModel : ScreenViewModelBase
    {
        private readonly InventoryModel _model;
        private readonly InputManager _inputManager;
        private int _lastDropFrame = -1;

        public event Action<Vector2> Navigate;
        public event Action Submit;
        public event Action SubmitAll;
        public event Action Action;

        public InventoryScreenViewModel(InventoryModel model, InputManager inputManager)
        {
            _model = model;
            _inputManager = inputManager;
        }

        public override ScreenId Id => ScreenId.Inventory;

        public Observable<InventoryViewState> State => _model.State;

        protected override void OnOpen(object args)
        {
            _model.Activate();
            _inputManager.OnUiNavigate += HandleNavigate;
            _inputManager.OnUiSubmit += HandleSubmit;
            _inputManager.OnUiSubmitAll += HandleSubmitAll;
            _inputManager.OnUiAction += HandleAction;
        }

        protected override void OnClose()
        {
            _inputManager.OnUiNavigate -= HandleNavigate;
            _inputManager.OnUiSubmit -= HandleSubmit;
            _inputManager.OnUiSubmitAll -= HandleSubmitAll;
            _inputManager.OnUiAction -= HandleAction;
            _model.Deactivate();
        }

        public void DropItem(string itemId, int count)
        {
            if (_lastDropFrame == Time.frameCount)
                return;

            _lastDropFrame = Time.frameCount;
            _model.DropItem(itemId, count);
        }

        private void HandleNavigate(Vector2 value)
        {
            Navigate?.Invoke(value);
        }

        private void HandleSubmit()
        {
            Submit?.Invoke();
        }

        private void HandleSubmitAll()
        {
            SubmitAll?.Invoke();
        }

        private void HandleAction()
        {
            Action?.Invoke();
        }
    }
}
