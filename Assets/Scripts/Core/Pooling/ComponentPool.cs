using System.Collections.Generic;
using UnityEngine;

namespace LinkMatch.Core.Pooling
{
    public sealed class ComponentPool<T> : IPool<T> where T : Component
    {
        private readonly Stack<T> _stack = new();
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly int _maxSize; // 0 => sınırsız

        public ComponentPool(T prefab, Transform parent, int maxSize = 0)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;
        }

        public int CountInactive => _stack.Count;

        public T Get()
        {
            T item = _stack.Count > 0 ? _stack.Pop() : Object.Instantiate(_prefab, _parent);

            if (item is IPoolable poolable)
                poolable.OnSpawned();

            var go = item.gameObject;
            if (!go.activeSelf)
                go.SetActive(true);

            return item;
        }

        public void Return(T item)
        {
            if (!item) return;
            if (item is IPoolable p) p.OnDespawned();
            var tr = item.transform;
            tr.SetParent(_parent, worldPositionStays:false);
            tr.localScale = Vector3.one; // güvenli reset
            item.gameObject.SetActive(false);

            if (_maxSize > 0 && _stack.Count >= _maxSize) { Object.Destroy(item.gameObject); return; }
            _stack.Push(item);
        }

        public void Prewarm(int count)
        {
            if (count <= 0) return;

            var tmp = new List<T>(count);
            for (int i = 0; i < count; i++)
                tmp.Add(Get());

            for (int i = 0; i < tmp.Count; i++)
                Return(tmp[i]);
        }

        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var it = _stack.Pop();
                if (it) Object.Destroy(it.gameObject);
            }
        }
    }
}