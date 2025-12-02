using System.Collections.Generic;
using UnityEngine;
using Utils;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private int _initialPoolSize = 10;
    
    private Queue<AudioSourceController> _pool = new Queue<AudioSourceController>();
    private Transform _poolContainer;

    protected override void Awake()
    {
        base.Awake();
        InitializePool();
    }

    private void InitializePool()
    {
        _poolContainer = new GameObject("AudioPool").transform;
        _poolContainer.SetParent(transform);

        for (int i = 0; i < _initialPoolSize; i++)
        {
            CreateNewController();
        }
    }

    private AudioSourceController CreateNewController()
    {
        GameObject go = new GameObject($"AudioSource_{_pool.Count}");
        go.transform.SetParent(_poolContainer);
        
        var controller = go.AddComponent<AudioSourceController>();
        controller.Initialize(this);
        
        go.SetActive(false);
        _pool.Enqueue(controller);
        
        return controller;
    }

    public static void Play(AudioClip clip, Vector3? position = null, float volume = 1f)
    {
        if (Instance != null)
        {
            Instance.PlayInternal(clip, position, volume);
        }
    }

    private void PlayInternal(AudioClip clip, Vector3? position, float volume)
    {
        if (clip == null) return;

        AudioSourceController controller = GetController();
        
        // If position is null, we assume 2D sound (position doesn't matter much, but we can use camera pos or zero)
        // If position is provided, we assume 3D sound at that position
        bool is3D = position.HasValue;
        Vector3 playPosition = position ?? Vector3.zero;

        // If 2D, we might want to attach it to the camera or just play it. 
        // For simplicity in this setup, we just play at 0,0,0 or provided pos with spatial blend 0.
        
        controller.gameObject.SetActive(true);
        controller.Play(clip, playPosition, volume, is3D);
    }

    private AudioSourceController GetController()
    {
        if (_pool.Count == 0)
        {
            return CreateNewController();
        }

        return _pool.Dequeue();
    }

    public void ReturnController(AudioSourceController controller)
    {
        controller.gameObject.SetActive(false);
        _pool.Enqueue(controller);
    }
}

