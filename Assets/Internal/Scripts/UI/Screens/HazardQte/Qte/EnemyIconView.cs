using System;
using Internal.Scripts.Travel.Hazards.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class EnemyIconView : MonoBehaviour
    {
        [SerializeField] private Image _base;
        [SerializeField] private Button _button;

        public event Action<EnemyIconView> OnDefeated;

        private void Awake()
        {
            _button.onClick.AddListener(HandleClick);
        }

        public void Apply(EnemyVisual visual)
        {
            _base.sprite = visual.Sprite;
            _base.color = visual.Color;
            _button.interactable = true;
        }

        private void HandleClick()
        {
            _button.interactable = false;
            OnDefeated?.Invoke(this);
        }
    }
}
