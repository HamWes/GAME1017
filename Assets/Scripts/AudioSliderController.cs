using UnityEngine;
using UnityEngine.UI;

public enum ESoundType
{
    None,
    Music,
    SFX
}

public class AudioSliderController : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    [SerializeField] private ESoundType soundType;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        float savedVolume = 0f;

        switch (soundType)
        {
            case ESoundType.Music:
                savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0f);
                break;

            case ESoundType.SFX:
                savedVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0f);
                break;
        }

        slider.SetValueWithoutNotify(savedVolume);
        ChangeSoundVolume(savedVolume);
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(ChangeSoundVolume);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(ChangeSoundVolume);
    }

    private void ChangeSoundVolume(float newVolume)
    {
        switch (soundType)
        {
            case ESoundType.Music:
                GameManager.Instance.SoundManager.ChangeMusicVolume(newVolume);
                PlayerPrefs.SetFloat(MusicVolumeKey, newVolume);
                break;

            case ESoundType.SFX:
                GameManager.Instance.SoundManager.ChangeSFXVolume(newVolume);
                PlayerPrefs.SetFloat(SFXVolumeKey, newVolume);
                break;
        }

        PlayerPrefs.Save();
    }
}
