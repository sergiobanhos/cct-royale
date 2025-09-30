// Scripts/Core/MainThread.cs
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Core
{
    public class MainThread : MonoBehaviour
    {
        private static MainThread _inst;
        private static readonly ConcurrentQueue<Action> _q = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_inst != null) return;
            var go = new GameObject("[MainThread]");
            _inst = go.AddComponent<MainThread>();
            GameObject.DontDestroyOnLoad(go);
        }

        public static void Run(Action a) => _q.Enqueue(a);

        private void Update()
        {
            while (_q.TryDequeue(out var a))
                a?.Invoke();
        }
    }
}
