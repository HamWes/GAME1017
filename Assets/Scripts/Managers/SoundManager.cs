using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    public float SFXVolume => sfxSource != null ? sfxSource.volume : 0f;
    public float MusicVolume => musicSource != null ? musicSource.volume : 0f;

    public void ChangeMusicVolume(float newVolume)
    {
        if (musicSource != null)
        {
            musicSource.volume = newVolume;
        }
    }

    public void ChangeSFXVolume(float newVolume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = newVolume;
        }
    }
}
