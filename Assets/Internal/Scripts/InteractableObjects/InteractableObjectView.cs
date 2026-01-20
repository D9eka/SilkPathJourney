using System;
using UnityEngine;
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