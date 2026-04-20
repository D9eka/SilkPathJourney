using System;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Input;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class MultiClickMinigame : MonoBehaviour, IQteMinigameView
    {
        [SerializeField] private RectTransform _enemiesContainer;
        [SerializeField] private EnemyIconView _iconPrefab;
        [SerializeField] private float _punchScale = 0.3f;
        [SerializeField] private float _punchDuration = 0.2f;

        public event Action<bool> OnCompleted;
        public bool DidPlayerSucceed() => _clickCount >= _required;

        private readonly List<EnemyIconView> _pool = new();
        private IQteInput _input;
        private int _clickCount;
        private int _required;
        private bool _active;

        public void Show(IHazardInputConfig config, IQteInput input)
        {
            if (config is not MultiClickInputConfig mc || mc.EnemyPool == null || mc.EnemyPool.Visuals.Length == 0)
            {
                Debug.LogError($"[MultiClickMinigame] Invalid config or missing EnemyPool on {config?.GetType().Name}");
                OnCompleted?.Invoke(false);
                return;
            }

            _input = input;
            _clickCount = 0;
            _active = true;
            _required = UnityEngine.Random.Range(mc.MinClicks, mc.MaxClicks + 1);

            while (_pool.Count < _required)
                _pool.Add(Instantiate(_iconPrefab, _enemiesContainer));

            for (int i = 0; i < _required; i++)
            {
                var visual = mc.EnemyPool.Visuals[UnityEngine.Random.Range(0, mc.EnemyPool.Visuals.Length)];
                var view = _pool[i];
                view.gameObject.SetActive(true);
                view.Apply(visual);
                view.OnDefeated -= HandleEnemyDefeated;
                view.OnDefeated += HandleEnemyDefeated;
            }

            for (int i = _required; i < _pool.Count; i++)
            {
                _pool[i].OnDefeated -= HandleEnemyDefeated;
                _pool[i].gameObject.SetActive(false);
            }

            _input.Enable();
        }

        public void Hide()
        {
            foreach (var view in _pool)
                view.OnDefeated -= HandleEnemyDefeated;

            if (_input == null) return;
            _input.Disable();
            _input = null;
        }

        private void HandleEnemyDefeated(EnemyIconView view)
        {
            if (!_active) return;

            view.transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration).SetUpdate(true);
            _clickCount++;

            if (_clickCount >= _required)
            {
                _active = false;
                Hide();
                OnCompleted?.Invoke(true);
            }
        }
    }
}
