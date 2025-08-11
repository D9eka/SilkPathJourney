using System;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Internal.Scripts.InteractableObjects
{
    public class InteractableObject : MonoBehaviour
    {
        public event Action<InteractableObject> OnClick;

        [Header("Visual")]
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] protected SpriteRenderer _outline;

        [Header("Text")]
        [SerializeField] protected TextMeshPro _textMesh;
        [SerializeField] protected float _textOffsetY = 0;
        [SerializeField] protected Vector2 _textSizeModifier = new Vector2(1.5f, 0.25f);

        [Header("Settings")]
        [SerializeField] protected string _interactableName = "Building";
        [SerializeField] protected Vector2 _size = new Vector2(2, 2);

        protected BoxCollider2D Collider;

        private void Awake()
        {
            _outline.color = _outline.color.WithAlpha(0f);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _textMesh = GetComponentInChildren<TextMeshPro>();
            Collider = GetComponent<BoxCollider2D>();
        }

        private void OnValidate()
        {
            if (_spriteRenderer == null || _textMesh == null || Collider == null)
            {
                Reset();
            }
            ApplyChanges();
        }

        private void ApplyChanges()
        {
            _spriteRenderer.size = _size;
            _outline.size = _size;
            Collider.size = _size;

            _textMesh.text = _interactableName;
            _textMesh.transform.localPosition = new Vector3(0, _size.y / 2 + _textOffsetY, 0);
            _textMesh.enableAutoSizing = true;
            _textMesh.fontSizeMin = 1;
            _textMesh.fontSizeMax = 512;
            _textMesh.rectTransform.sizeDelta = new Vector2(_size.x * _textSizeModifier.x, _size.y * _textSizeModifier.y);
            _textMesh.alignment = TextAlignmentOptions.Bottom;
        }
#endif
        
        public void SwitchObjectState(bool state)
        {
            SwitchHeaderState(state);
            SwitchInputState(state);
        }

        public void SwitchHeaderState(bool state)
        {
            _textMesh.DOColor(_textMesh.color.WithAlpha(state ? 1f : 0f), 0.3f);
        }

        public void SwitchInputState(bool canClick)
        {
            Collider.enabled = canClick;
        }

        public void TriggerClick() => OnClick?.Invoke(this);

        public void TriggerHoverEnter()
        {
            _outline.DOColor(_outline.color.WithAlpha(1f), 0.3f);
        }

        public void TriggerHoverExit()
        {
            _outline.DOColor(_outline.color.WithAlpha(0f), 0.3f);
        }
    }
}
