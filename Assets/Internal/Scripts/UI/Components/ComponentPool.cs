using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _pool = new();

        public ComponentPool(T prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public T Get()
        {
            if (_pool.Count > 0)
            {
                var instance = _pool.Pop();
                instance.gameObject.SetActive(true);
                return instance;
            }
            return Object.Instantiate(_prefab, _parent);
        }

        public void Release(T instance)
        {
            if (instance == null) return;
            instance.gameObject.SetActive(false);
            _pool.Push(instance);
        }
    }
}
