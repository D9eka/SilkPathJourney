using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.World.VisualObjects
{
    public class MonoBehVisualObject : MonoBehaviour, IVisualObject
    {
        [field: SerializeField] public List<WorldDetailLevel> ViewMode { get; private set; }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}