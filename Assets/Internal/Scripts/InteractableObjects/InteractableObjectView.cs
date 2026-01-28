using System;
using DG.Tweening;
using UnityEngine;
namespace Internal.Scripts.InteractableObjects
{
    public abstract class InteractableObjectView : MonoBehaviour, IInteractableObject
    {
        [Header("Interactable")]
        [SerializeField] protected float _hoverScale = 1.1f;
    
        public event Action<IInteractableObject> OnClick;
    
        protected Vector3 OriginalScale;
    
        protected virtual void Awake()
        {
            OriginalScale = transform.localScale;
        }
        
        protected virtual void OnEnable()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(OriginalScale, 0.3f);
        }

        public virtual void Enable()
        {
            gameObject.SetActive(true);
        }

        public virtual void Disable()
        {
            transform.DOScale(Vector3.zero, 0.3f)
                .OnComplete(() => gameObject.SetActive(false));
        }
    
        public void TriggerClick()
        {
            OnClickEffect();
            OnClick?.Invoke(this);
        }
    
        public void TriggerHoverEnter()
        {
            transform.localScale = OriginalScale * _hoverScale;
            OnHoverEnterEffect();
        }
    
        public void TriggerHoverExit()
        {
            transform.localScale = OriginalScale;
            OnHoverExitEffect();
        }
    
        protected virtual void OnClickEffect() { }
        protected virtual void OnHoverEnterEffect() { }
        protected virtual void OnHoverExitEffect() { }
    }
}