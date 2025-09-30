using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Base genérica para Singletons de MonoBehaviour.
    /// Uso: public class AudioManager : MonoSingleton<AudioManager> { ... }
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationQuitting;
        
        /// <summary>
        /// Se true, o GameObject portador não será destruído ao carregar novas cenas.
        /// Altere no Awake do filho antes de base.Awake() ou crie um construtor estático.
        /// </summary>
        protected virtual bool Persistent => true;

        /// <summary>
        /// Acesso à instância. Cria automaticamente um GameObject se não existir.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationQuitting) return null;

                if (_instance != null) return _instance;

                lock (_lock)
                {
                    if (_instance != null) return _instance;

                    // Tentar encontrar na cena (ex.: já colocado manualmente)
                    _instance = FindObjectOfType<T>();
                    if (_instance != null) return _instance;

                    // Criar dinamicamente
                    var go = new GameObject($"[{typeof(T).Name}]");
                    _instance = go.AddComponent<T>();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Override se quiser lógica custom no primeiro Awake.
        /// </summary>
        protected virtual void OnInit() {}

        protected virtual void Awake()
        {
            if (_applicationQuitting) return;

            if (_instance == null)
            {
                _instance = (T)this;
                if (Persistent) DontDestroyOnLoad(gameObject);
                OnInit();
            }
            else if (_instance != this)
            {
                // Evita duplicatas (pode logar se quiser)
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
