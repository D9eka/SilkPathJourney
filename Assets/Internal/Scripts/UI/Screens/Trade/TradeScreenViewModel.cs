using System;
using Internal.Scripts.Input;
using Internal.Scripts.Npc.Encounter;
using Internal.Scripts.Trading;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Tooltip;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Internal.Scripts.UI.Screens.Trade
{
    public sealed class TradeScreenViewModel : ScreenViewModelBase
    {
        private readonly TradeModel _model;
        private readonly InputRouter _inputManager;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly TooltipService _tooltipService;
        private float _ignoreSubmitUntil;

        public event Action<Vector2> Navigate;
        public event Action Submit;
        public event Action SubmitAll;
        public event Action NextArea;
        public event Action PrevArea;

        public TradeScreenViewModel(TradeModel model, InputRouter inputManager,
            ResourceIconCatalog resourceIcons, TooltipService tooltipService)
        {
            _model = model;
            _inputManager = inputManager;
            _resourceIcons = resourceIcons;
            _tooltipService = tooltipService;
        }

        public override ScreenId Id => ScreenId.Trade;

        public Observable<TradeViewState> State => _model.State;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public TooltipService TooltipService => _tooltipService;

        protected override void OnOpen(object args)
        {
            if (args is NpcTradeArgs npcArgs)
                _model.ActivateWithNpc(npcArgs);
            else if (args is GuildTradeArgs guildArgs)
                _model.ActivateWithGuild(guildArgs.CityId);
            else
                _model.Activate(args as string);
            _inputManager.OnUiNavigate += HandleNavigate;
            _inputManager.OnUiSubmit += HandleSubmit;
            _inputManager.OnUiSubmitAll += HandleSubmitAll;
            _inputManager.OnUiNextArea += HandleNextArea;
            _inputManager.OnUiPrevArea += HandlePrevArea;
        }

        protected override void OnClose()
        {
            _inputManager.OnUiNavigate -= HandleNavigate;
            _inputManager.OnUiSubmit -= HandleSubmit;
            _inputManager.OnUiSubmitAll -= HandleSubmitAll;
            _inputManager.OnUiNextArea -= HandleNextArea;
            _inputManager.OnUiPrevArea -= HandlePrevArea;
            _model.Deactivate();
        }

        public void MoveToBuy(string itemId, bool addAll) => _model.MoveToBuy(itemId, addAll);
        public void MoveToSell(string itemId, bool addAll) => _model.MoveToSell(itemId, addAll);
        public void ReturnFromBuy(string itemId, bool addAll) => _model.ReturnFromBuy(itemId, addAll);
        public void ReturnFromSell(string itemId, bool addAll) => _model.ReturnFromSell(itemId, addAll);
        public void ExecuteTrade() => _model.ExecuteTrade();

        private void HandleNavigate(Vector2 value) => Navigate?.Invoke(value);

        private void HandleSubmit()
        {
            if (Time.unscaledTime < _ignoreSubmitUntil)
                return;

            if (Keyboard.current != null && Keyboard.current.shiftKey.isPressed)
                return;

            Submit?.Invoke();
        }

        private void HandleSubmitAll()
        {
            _ignoreSubmitUntil = Time.unscaledTime + 0.1f;
            SubmitAll?.Invoke();
        }

        private void HandleNextArea() => NextArea?.Invoke();
        private void HandlePrevArea() => PrevArea?.Invoke();
    }
}
