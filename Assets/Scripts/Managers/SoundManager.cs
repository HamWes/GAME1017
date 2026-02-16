using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource, musicSource;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    [ContextMenu("Change Music Volume")]
    public void ChangeMusicVolume(float newVolume)
    {
        musicSource.volume = newVolume;
    }

    [ContextMenu("Change SFX Volume")]
    public void ChangeSFXVolume(float newVolume)
    {
        sfxSource.volume = newVolume;
    }
}
