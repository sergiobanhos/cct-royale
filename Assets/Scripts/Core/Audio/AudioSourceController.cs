using System.Collections;
using UnityEngine;

public class AudioSourceController : MonoBehaviour
{
    private AudioSource _audioSource;
    private AudioManager _manager;

    public void Initialize(AudioManager manager)
    {
        _manager = manager;
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    public void Play(AudioClip clip, Vector3 position, float volume, bool is3D)
    {
        transform.position = position;
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.spatialBlend = is3D ? 0.9f : 0f; // 1f is 3D, 0f is 2D
        
        _audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(clip.length));
    }

    private IEnumerator ReturnToPoolAfterPlay(float duration)
    {
        yield return new WaitForSeconds(duration);
        _audioSource.Stop();
        _audioSource.clip = null;
        _manager.ReturnController(this);
    }
}
