// Scripts/Core/EventBus.cs
using System;
using System.Collections.Generic;

namespace Core
{
    public interface IGameEvent { }

    public static class EventBus
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, List<Delegate>> _subs = new();

        public static IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            lock (_lock)
            {
                var t = typeof(T);
                if (!_subs.TryGetValue(t, out var list))
                {
                    list = new List<Delegate>();
                    _subs[t] = list;
                }
                list.Add(handler);
            }

            return new Unsub<T>(handler);
        }

        public static void Publish<T>(T evt) where T : IGameEvent
        {
            List<Delegate> handlersCopy = null;
            lock (_lock)
            {
                if (_subs.TryGetValue(typeof(T), out var list))
                    handlersCopy = new List<Delegate>(list);
            }
            if (handlersCopy == null) return;

            foreach (var d in handlersCopy)
            {
                try { ((Action<T>)d)?.Invoke(evt); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
        }

        private class Unsub<T> : IDisposable where T : IGameEvent
        {
            private Action<T> _handler;
            public Unsub(Action<T> handler) { _handler = handler; }
            public void Dispose()
            {
                if (_handler == null) return;
                lock (_lock)
                {
                    if (_subs.TryGetValue(typeof(T), out var list))
                        list.Remove(_handler);
                }
                _handler = null;
            }
        }
    }
}
