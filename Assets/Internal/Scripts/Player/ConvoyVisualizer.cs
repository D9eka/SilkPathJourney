using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Npc.Core;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Player
{
    public sealed class ConvoyVisualizer : ITickable, IDisposable
    {
        private const int BUFFER_SIZE = 120;
        private const float SPACING_METERS = 0.5f;
        private const float ROTATION_SPEED = 10f;
        private const int SAMPLES_PER_SPACING = 10;
        private const float RECORD_INTERVAL = SPACING_METERS / SAMPLES_PER_SPACING;
        private const float MIN_MOVE_THRESHOLD = 0.01f;
        private static readonly Vector3 FALLBACK_CART_SCALE = new(0.6f, 0.4f, 1.2f);

        private readonly RoadAgentView _playerView;
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly ConvoyPrefabCatalog _catalog;

        private readonly Vector3[] _positionBuffer = new Vector3[BUFFER_SIZE];
        private readonly Vector3[] _forwardBuffer = new Vector3[BUFFER_SIZE];
        private int _bufferHead;
        private int _bufferCount;
        private float _accumulatedDistance;
        private Vector3 _lastRecordedPosition;

        private readonly List<CartFollowerView> _followers = new();

        private CartClass _currentMainCartClass = (CartClass)(-1);
        private DraftAnimalType _currentAnimalType = (DraftAnimalType)(-1);
        private CartFollowerView _mainCartView;

        public ConvoyVisualizer(
            RoadAgentView playerView,
            PlayerResourceRepository resourceRepo,
            [Inject(Optional = true)] ConvoyPrefabCatalog catalog)
        {
            _playerView = playerView;
            _resourceRepo = resourceRepo;
            _catalog = catalog;
            _lastRecordedPosition = _playerView.VisualRoot.position;
        }

        public void Tick()
        {
            RecordPosition();
            SyncMainCart();
            SyncFollowerCount();
            UpdateFollowerPositions();
        }

        private void RecordPosition()
        {
            Vector3 currentPos = _playerView.VisualRoot.position;
            Vector3 currentFwd = _playerView.VisualRoot.forward;

            float moved = Vector3.Distance(currentPos, _lastRecordedPosition);
            if (moved < MIN_MOVE_THRESHOLD)
                return;

            _accumulatedDistance += moved;
            _lastRecordedPosition = currentPos;

            if (_accumulatedDistance < RECORD_INTERVAL)
                return;

            _accumulatedDistance = 0f;
            _positionBuffer[_bufferHead] = currentPos;
            _forwardBuffer[_bufferHead] = currentFwd;
            _bufferHead = (_bufferHead + 1) % BUFFER_SIZE;
            if (_bufferCount < BUFFER_SIZE)
                _bufferCount++;
        }

        private void SyncFollowerCount()
        {
            var carts = _resourceRepo.Current.Carts;
            int cartCount = carts?.Count ?? 0;

            while (_followers.Count > cartCount)
            {
                int last = _followers.Count - 1;
                if (_followers[last] != null)
                    UnityEngine.Object.Destroy(_followers[last].gameObject);
                _followers.RemoveAt(last);
            }

            while (_followers.Count < cartCount)
            {
                string typeId = carts[_followers.Count].TypeId ?? "";
                Enum.TryParse<ExtraCartType>(typeId, true, out var extraType);
                _followers.Add(CreateFollower(extraType));
            }

            SyncRiders();
        }

        private void UpdateFollowerPositions()
        {
            if (_bufferCount == 0 || _followers.Count == 0)
                return;

            for (int i = 0; i < _followers.Count; i++)
            {
                int offset = (i + 1) * SAMPLES_PER_SPACING;
                if (offset >= _bufferCount)
                    offset = _bufferCount - 1;

                int idx = (_bufferHead - 1 - offset + BUFFER_SIZE * 2) % BUFFER_SIZE;

                _followers[i].transform.position = _positionBuffer[idx];
                RoadAgentView.SmoothRotate(_followers[i].transform, _forwardBuffer[idx], ROTATION_SPEED);
                _followers[i].UpdateMovementState();
            }
        }

        public void Dispose()
        {
            foreach (var follower in _followers)
            {
                if (follower != null)
                    UnityEngine.Object.Destroy(follower.gameObject);
            }
            _followers.Clear();

            if (_mainCartView != null)
                UnityEngine.Object.Destroy(_mainCartView.gameObject);
            _mainCartView = null;
        }

        private void SyncMainCart()
        {
            if (_catalog == null) return;

            string cartClassId = _resourceRepo.Current.CartClassId;
            if (!Enum.TryParse<CartClass>(cartClassId, true, out var cartClass))
                return;

            if (cartClass != _currentMainCartClass)
            {
                if (_mainCartView != null)
                    UnityEngine.Object.Destroy(_mainCartView.gameObject);

                GameObject prefab = _catalog.GetMainCart(cartClass);
                if (prefab != null)
                {
                    GameObject go = UnityEngine.Object.Instantiate(prefab, _playerView.VisualRoot);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    _mainCartView = go.GetComponent<CartFollowerView>() ?? go.AddComponent<CartFollowerView>();
                }

                _currentMainCartClass = cartClass;
                _currentAnimalType = (DraftAnimalType)(-1);
            }

            SyncMainAnimal();
        }

        private void SyncMainAnimal()
        {
            if (_catalog == null || _mainCartView == null) return;

            string animalId = _resourceRepo.Current.DraftAnimalId;
            if (!Enum.TryParse<DraftAnimalType>(animalId, true, out var animalType))
                return;

            if (animalType != _currentAnimalType)
            {
                var entry = _catalog.GetAnimal(animalType);
                if (entry.Prefab != null)
                    _mainCartView.SetAnimal(entry.Prefab, entry.Scale > 0 ? entry.Scale : 1f, entry.Offset);
                else
                    _mainCartView.ClearAnimal();
                _currentAnimalType = animalType;
            }
        }

        private CartFollowerView CreateFollower(ExtraCartType extraType)
        {
            GameObject prefab = _catalog?.GetExtraCart(extraType);
            GameObject go;
            if (prefab != null)
            {
                go = UnityEngine.Object.Instantiate(prefab);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "ConvoyCart";
                go.transform.localScale = FALLBACK_CART_SCALE;

                var collider = go.GetComponent<Collider>();
                if (collider != null)
                    UnityEngine.Object.Destroy(collider);
            }

            var view = go.GetComponent<CartFollowerView>() ?? go.AddComponent<CartFollowerView>();
            SetAnimalOnCart(view);
            return view;
        }

        private void SetAnimalOnCart(CartFollowerView view)
        {
            if (_catalog == null) return;
            string animalId = _resourceRepo.Current.DraftAnimalId;
            if (!Enum.TryParse<DraftAnimalType>(animalId, true, out var animalType))
                return;
            var entry = _catalog.GetAnimal(animalType);
            if (entry.Prefab != null)
                view.SetAnimal(entry.Prefab, entry.Scale > 0 ? entry.Scale : 1f, entry.Offset);
        }

        private void SyncRiders()
        {
            var companions = _resourceRepo.Current.Companions;
            GameObject riderPrefab = _catalog?.RiderPrefab;

            for (int i = 0; i < _followers.Count; i++)
            {
                bool hasRider = riderPrefab != null
                    && i < (companions?.Count ?? 0)
                    && !companions[i].IsInjured;

                if (hasRider)
                    _followers[i].SetRider(riderPrefab);
                else
                    _followers[i].ClearRider();
            }
        }
    }
}
