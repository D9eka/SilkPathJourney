using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Internal.Scripts.UI.Components
{
    public class HoverReporter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action Entered;
        public event Action Exited;

        public static HoverReporter GetOrAdd(GameObject go)
        {
            var h = go.GetComponent<HoverReporter>();
            return h != null ? h : go.AddComponent<HoverReporter>();
        }

        public void OnPointerEnter(PointerEventData eventData) => Entered?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => Exited?.Invoke();

        private void OnDisable()
        {
            Exited?.Invoke();
        }
    }
}
