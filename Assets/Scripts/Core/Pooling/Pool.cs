using System;
using System.Collections.Generic;

namespace LinkMatch.Core.Pooling
{
    public sealed class Pool<T> : IPool<T>
    {
        private readonly Stack<T> _stack = new();
        private readonly Func<T> _create;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onReturn;
        private readonly int _maxSize; // 0 => sınırsız

        public Pool(Func<T> create, Action<T> onGet = null, Action<T> onReturn = null, int maxSize = 0)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _onGet = onGet; _onReturn = onReturn; _maxSize = maxSize;
        }

        public int CountInactive => _stack.Count;

        public T Get()
        {
            var item = _stack.Count > 0 ? _stack.Pop() : _create();
            _onGet?.Invoke(item);
            if (item is IPoolable p) p.OnSpawned();
            return item;
        }

        public void Return(T item)
        {
            if (item is IPoolable p) p.OnDespawned();
            _onReturn?.Invoke(item);
            if (_maxSize > 0 && _stack.Count >= _maxSize) return; // drop
            _stack.Push(item);
        }

        public void Prewarm(int count)
        {
            var tmp = new List<T>(count);
            for (int i = 0; i < count; i++) tmp.Add(Get());
            foreach (var t in tmp) Return(t);
        }

        public void Clear() => _stack.Clear();
    }
}