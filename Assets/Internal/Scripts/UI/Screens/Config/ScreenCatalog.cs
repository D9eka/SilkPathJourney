using System.Collections.Generic;
using Internal.Scripts.UI.Screen.Config;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Config
{
    [CreateAssetMenu(menuName = "SPJ/UI/Screen Catalog", fileName = "ScreenCatalog")]
    public sealed class ScreenCatalog : ScriptableObject
    {
        [SerializeField] private List<ScreenConfig> _configs = new();

        public bool TryGet(ScreenId id, out ScreenConfig config)
        {
            if (_configs != null)
            {
                foreach (ScreenConfig entry in _configs)
                {
                    if (entry != null && entry.Id == id)
                    {
                        config = entry;
                        return true;
                    }
                }
            }

            config = null;
            return false;
        }
    }
}
