using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Quests
{
    public class QuestDataListView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _header;
        [SerializeField] private TextMeshProUGUI _itemPrefab;

        private readonly List<TextMeshProUGUI> _pool = new();

        public void SetHeader(string text)
        {
            if (_header != null) _header.text = text;
        }

        public void SetItems(IReadOnlyList<string> items)
        {
            HideAll();

            if (items == null || items.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            for (int i = 0; i < items.Count; i++)
            {
                var field = EnsureItem(i);
                field.gameObject.SetActive(true);
                field.text = items[i];
            }
        }

        public void Clear()
        {
            HideAll();
            gameObject.SetActive(false);
        }

        private TextMeshProUGUI EnsureItem(int index)
        {
            while (_pool.Count <= index)
            {
                var instance = Instantiate(_itemPrefab, transform);
                _pool.Add(instance);
            }
            return _pool[index];
        }

        private void HideAll()
        {
            foreach (var field in _pool)
                field.gameObject.SetActive(false);
        }
    }
}
