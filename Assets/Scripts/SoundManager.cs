using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource sfxSource, musicSource;

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
