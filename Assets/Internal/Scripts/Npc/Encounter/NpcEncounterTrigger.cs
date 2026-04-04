using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.Trading;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.World.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Npc.Encounter
{
    public sealed class NpcEncounterTrigger : IFixedTickable, IInitializable, IDisposable
    {
        private readonly PlayerController _player;
        private readonly NpcLifeSimulator _npcSimulator;
        private readonly ScreenStackService _screenStackService;
        private readonly GameClock _gameClock;
        private readonly EventTrigger _eventTrigger;
        private readonly NpcEncounterSettings _settings;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly TradeModel _tradeModel;

        private NpcCaravanAgent _pendingAgent;
        private NpcCaravanAgent _lastTradeAgent;
        private string _pendingPriceCityId;
        private float _cooldownUntil;
        private bool _waitingForTradeClose;

        public NpcEncounterTrigger(
            PlayerController player,
            NpcLifeSimulator npcSimulator,
            ScreenStackService screenStackService,
            GameClock gameClock,
            EventTrigger eventTrigger,
            NpcEncounterSettings settings,
            ICityNodeResolver cityNodeResolver,
            IRoadNodeLookup nodeLookup,
            TradeModel tradeModel)
        {
            _player = player;
            _npcSimulator = npcSimulator;
            _screenStackService = screenStackService;
            _gameClock = gameClock;
            _eventTrigger = eventTrigger;
            _settings = settings;
            _cityNodeResolver = cityNodeResolver;
            _nodeLookup = nodeLookup;
            _tradeModel = tradeModel;
        }

        public void Initialize()
        {
            _eventTrigger.OnEventClosed += HandleEventClosed;
            _screenStackService.OnScreenClosed += HandleScreenClosed;
        }

        public void Dispose()
        {
            _eventTrigger.OnEventClosed -= HandleEventClosed;
            _screenStackService.OnScreenClosed -= HandleScreenClosed;
        }

        public void FixedTick()
        {
            if (_settings == null || _settings.EncounterEvents == null || _settings.EncounterEvents.Count == 0)
                return;

            if (_waitingForTradeClose || _pendingAgent != null)
                return;

            if (Time.time < _cooldownUntil)
                return;

            if (_screenStackService.IsOpen(ScreenId.Event) || _screenStackService.IsOpen(ScreenId.Trade))
                return;

            if (_player.State != PlayerState.Moving)
                return;

            Vector3 playerPos = _player.CurrentPosition;
            var agents = _npcSimulator.Agents;
            for (int i = 0; i < agents.Count; i++)
            {
                NpcCaravanAgent agent = agents[i];
                if (agent?.RoadAgent == null)
                    continue;

                float dist = Vector3.Distance(playerPos, agent.RoadAgent.CurrentPose.Position);
                if (dist < _settings.EncounterDistance)
                {
                    TriggerEncounter(agent);
                    return;
                }
            }
        }

        private void TriggerEncounter(NpcCaravanAgent agent)
        {
            _pendingAgent = agent;
            _cooldownUntil = Time.time + _settings.CooldownSeconds;
            _eventTrigger.LastChoiceIndex = -1;

            EventData eventData = _settings.EncounterEvents[
                UnityEngine.Random.Range(0, _settings.EncounterEvents.Count)];

            string nodeId = _nodeLookup.FindNearestNodeId(agent.RoadAgent.CurrentPose.Position);
            _cityNodeResolver.TryGetCityByNodeId(nodeId, out CityData city);
            _pendingPriceCityId = city?.Id;

            var args = new EventTriggerArgs(eventData, city, false,
                new object[] { agent.EconomyState.Name });
            _gameClock.Pause();
            _screenStackService.TryOpen(ScreenId.Event, args, out _);
        }

        private void HandleEventClosed()
        {
            if (_pendingAgent == null)
                return;

            NpcCaravanAgent agent = _pendingAgent;
            _pendingAgent = null;

            if (_eventTrigger.LastChoiceIndex == 0)
            {
                _lastTradeAgent = agent;
                _gameClock.Pause();
                var tradeArgs = new NpcTradeArgs(agent, _pendingPriceCityId,
                    _settings.NpcTradeMarkup, _settings.SuppliesMarkupMultiplier);

                if (_screenStackService.TryOpen(ScreenId.Trade, tradeArgs, out ScreenOpenResult result))
                {
                    _waitingForTradeClose = true;
                }
                else
                {
                    Debug.LogError($"[SPJ NPC] Cannot open trade screen: {result}");
                    _lastTradeAgent = null;
                    _gameClock.Resume();
                }
            }
        }

        private void HandleScreenClosed(ScreenId id)
        {
            if (id != ScreenId.Trade || !_waitingForTradeClose)
                return;

            _waitingForTradeClose = false;

            var result = _tradeModel.GetNpcTradeResult();
            if (result.HasValue && _lastTradeAgent != null)
            {
                _lastTradeAgent.EconomyState.Inventory = result.Value.inventory;
                _lastTradeAgent.EconomyState.Money = result.Value.money;
            }

            _lastTradeAgent = null;
            _gameClock.Resume();
        }
    }
}
