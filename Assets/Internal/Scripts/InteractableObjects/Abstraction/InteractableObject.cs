using DG.Tweening;
using Internal.Scripts.Extensions;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.InteractableObjects.Abstraction
{
    public abstract class InteractableObject : MonoBehaviour, IClickable
    {
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
        
        #region Editor
        protected void Reset()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _textMesh = GetComponentInChildren<TextMeshPro>();
            Collider = GetComponent<BoxCollider2D>();
        }

        protected void OnValidate()
        {
            if (_spriteRenderer == null || _textMesh == null || Collider == null)
            {
                Reset();
            }

            ApplyChanges();
        }

        protected void ApplyChanges()
        {
            _spriteRenderer.size = _size;
            _outline.size = _size;
            Collider.size = _size;
            
            _textMesh.text = _interactableName;

            _textMesh.transform.localPosition = new Vector3(0, _size.y / 2 + _textOffsetY, 0);
            _textMesh.enableAutoSizing = true;
            _textMesh.fontSizeMin = 1;
            _textMesh.fontSizeMax = 512;

            RectTransform rectTransform = _textMesh.rectTransform;
            rectTransform.sizeDelta = new Vector2(_size.x * _textSizeModifier.x, _size.y * _textSizeModifier.y); 

            _textMesh.alignment = TextAlignmentOptions.Bottom;
        }
        #endregion

        protected virtual void Awake()
        {
            _outline.color = _outline.color.GetColorWithAlpha(0f);
        }

        public virtual void OnClick()
        {
            Debug.Log($"{_interactableName} clicked!");
        }

        public void SwitchObjectState(bool state)
        {
            SwitchHeaderState(state);
            SwitchInputState(state);
        }

        public void SwitchHeaderState(bool state)
        {
            _textMesh.DOColor(_textMesh.color.GetColorWithAlpha(state ? 1f : 0f), 0.3f);
        }

        public void SwitchInputState(bool canClick)
        {
            Collider.enabled = canClick;
        }

        public void OnHoverEnter()
        {
            _outline.DOColor(_outline.color.GetColorWithAlpha(1f), 0.3f);
        }

        public void OnHoverExit()
        {
            _outline.DOColor(_outline.color.GetColorWithAlpha(0f), 0.3f);
        }
    }
}