using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Npc.Core;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Player
{
    public sealed class ConvoyVisualizer : ITickable, IDisposable
    {
        private const int BUFFER_SIZE = 600;
        private const float ROTATION_SPEED = 10f;
        private const float RECORD_INTERVAL = 0.05f;
        private const float MIN_MOVE_THRESHOLD = 0.01f;
        private const float CARAVAN_GAP_METERS = 0.1f;
        private const float DEFAULT_UNIT_LENGTH = 3f;

        private readonly RoadAgentView _playerView;
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly ConvoyPrefabCatalog _catalog;
        private readonly CaravanDatabase _caravanDb;

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
        private bool _animalIdDiagWarned;
        private bool _animalPrefabDiagWarned;
        private bool _extraCartDiagWarned;

        public ConvoyVisualizer(
            RoadAgentView playerView,
            PlayerResourceRepository resourceRepo,
            CaravanDatabase caravanDb,
            [Inject(Optional = true)] ConvoyPrefabCatalog catalog)
        {
            _playerView = playerView;
            _resourceRepo = resourceRepo;
            _caravanDb = caravanDb;
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
            Vector3 currentPos = _mainCartView != null
                ? _mainCartView.AnimalFrontWorld
                : _playerView.VisualRoot.position;
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
                var data = _caravanDb.ExtraCarts.Find(e => e.Id == typeId);
                if (data == null && !string.IsNullOrEmpty(typeId) && !_extraCartDiagWarned)
                {
                    Debug.LogWarning($"[ConvoyVisualizer] No ExtraCartData for TypeId='{typeId}'");
                    _extraCartDiagWarned = true;
                }
                var extraType = data?.Type ?? ExtraCartType.Unknown;
                _followers.Add(CreateFollower(extraType));
            }
        }

        private void UpdateFollowerPositions()
        {
            if (_bufferCount == 0 || _followers.Count == 0)
                return;

            float playerUnitLength = ComputeHeadDistance(_mainCartView) + ComputeTailDistance(_mainCartView);
            float distanceAlongPath = playerUnitLength + CARAVAN_GAP_METERS;

            for (int i = 0; i < _followers.Count; i++)
            {
                var follower = _followers[i];
                if (follower == null)
                {
                    distanceAlongPath += DEFAULT_UNIT_LENGTH + CARAVAN_GAP_METERS;
                    continue;
                }
                float headDist = ComputeHeadDistance(follower);
                float tailDist = ComputeTailDistance(follower);

                float pivotDistance = distanceAlongPath + headDist;
                int sampleSteps = Mathf.RoundToInt(pivotDistance / RECORD_INTERVAL);
                if (sampleSteps >= _bufferCount) sampleSteps = _bufferCount - 1;
                if (sampleSteps < 0) sampleSteps = 0;

                int idx = (_bufferHead - 1 - sampleSteps + BUFFER_SIZE * 2) % BUFFER_SIZE;

                follower.transform.position = _positionBuffer[idx] + Vector3.up * RoadAgentView.ROAD_GROUND_OFFSET;
                RoadAgentView.SmoothRotate(follower.transform, _forwardBuffer[idx], ROTATION_SPEED);
                follower.UpdateMovementState();

                distanceAlongPath += headDist + tailDist + CARAVAN_GAP_METERS;
            }
        }

        private static float ComputeHeadDistance(CartFollowerView view)
        {
            if (view == null) return DEFAULT_UNIT_LENGTH * 0.5f;
            return Vector3.Distance(view.AnimalFrontWorld, view.transform.position);
        }

        private static float ComputeTailDistance(CartFollowerView view)
        {
            if (view == null) return DEFAULT_UNIT_LENGTH * 0.5f;
            return Vector3.Distance(view.CaravanBackWorld, view.transform.position);
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
            {
                if (!_animalIdDiagWarned)
                {
                    Debug.LogWarning($"[ConvoyVisualizer] DraftAnimalId='{animalId}' is not a valid DraftAnimalType");
                    _animalIdDiagWarned = true;
                }
                return;
            }

            if (animalType != _currentAnimalType)
            {
                var entry = _catalog.GetAnimal(animalType);
                if (entry.Prefab != null)
                {
                    _mainCartView.SetAnimal(entry.Prefab, entry.Scale > 0 ? entry.Scale : 1f, entry.Offset);
                    foreach (var follower in _followers)
                        if (follower != null)
                            follower.SetAnimal(entry.Prefab, entry.Scale > 0 ? entry.Scale : 1f, entry.Offset);
                }
                else
                {
                    if (!_animalPrefabDiagWarned)
                    {
                        Debug.LogWarning($"[ConvoyVisualizer] No prefab for animal type {animalType} in catalog");
                        _animalPrefabDiagWarned = true;
                    }
                    _mainCartView.ClearAnimal();
                    foreach (var follower in _followers)
                        if (follower != null)
                            follower.ClearAnimal();
                }
                _currentAnimalType = animalType;
            }
        }

        private CartFollowerView CreateFollower(ExtraCartType extraType)
        {
            GameObject prefab = _catalog?.GetExtraCart(extraType);
            if (prefab == null) return null;

            GameObject go = UnityEngine.Object.Instantiate(prefab, _playerView.VisualRoot);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
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

    }
}
