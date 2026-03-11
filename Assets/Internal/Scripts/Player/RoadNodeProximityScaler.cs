using System;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.Road.Nodes;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Player
{
    public class RoadNodeProximityScaler : ITickable, IDisposable
    {
        private const float FLATTEN_RADIUS = 1f;
        private const float FULL_SCALE_RADIUS = 10f;

        private readonly IPlayerStateProvider _player;
        private readonly CityViewSpawner _cityViewSpawner;
        private readonly IRoadNodeLookup _nodeLookup;

        private CityView _fromView;
        private CityView _toView;
        private string _lastFromId;
        private string _lastToId;
        private bool _wasMoving;

        public RoadNodeProximityScaler(
            IPlayerStateProvider player,
            CityViewSpawner cityViewSpawner,
            IRoadNodeLookup nodeLookup)
        {
            _player = player;
            _cityViewSpawner = cityViewSpawner;
            _nodeLookup = nodeLookup;
        }

        public void Tick()
        {
            bool isMoving = _player.State == PlayerState.Moving;

            if (isMoving != _wasMoving)
            {
                SetHoverOnAll(!isMoving);
                _wasMoving = isMoving;
            }

            string fromId = _player.CurrentFromNodeId;
            string toId = _player.CurrentToNodeId;

            if (fromId == null && toId == null)
            {
                fromId = _player.CurrentNodeId;
            }

            if (fromId != _lastFromId || toId != _lastToId)
            {
                ReleaseViews();
                _lastFromId = fromId;
                _lastToId = toId;
                _fromView = FindView(fromId);
                _toView = FindView(toId);
            }

            Vector3 playerPos = _player.CurrentPosition;
            ApplyScale(_fromView, _lastFromId, playerPos);
            ApplyScale(_toView, _lastToId, playerPos);
        }

        public void Dispose()
        {
            ReleaseViews();
            SetHoverOnAll(true);
        }

        private void ApplyScale(CityView view, string nodeId, Vector3 playerPos)
        {
            if (view == null || nodeId == null) return;

            Vector3? nodePos = _nodeLookup.GetPosition(nodeId);
            if (nodePos == null) return;

            float distance = Vector3.Distance(playerPos, nodePos.Value);
            float factor = Mathf.InverseLerp(FLATTEN_RADIUS, FULL_SCALE_RADIUS, distance);
            view.SetYScaleFactor(factor);
        }

        private void ReleaseViews()
        {
            if (_fromView != null)
            {
                _fromView.AnimateRestoreScale();
                _fromView = null;
            }

            if (_toView != null)
            {
                _toView.AnimateRestoreScale();
                _toView = null;
            }
        }

        private void SetHoverOnAll(bool enabled)
        {
            foreach (CityView view in _cityViewSpawner.Views)
                view.IsHoverEnabled = enabled;
        }

        private CityView FindView(string nodeId)
        {
            return nodeId != null ? _cityViewSpawner.FindByNodeId(nodeId) : null;
        }
    }
}
