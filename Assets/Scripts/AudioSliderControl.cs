using UnityEngine;
using UnityEngine.UI;

public enum ESoundType
{
    None,
    Music,
    SFX
}

public class AudioSliderControl : MonoBehaviour
{
    [SerializeField] private ESoundType soundType;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
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
                SoundManager.Instance.ChangeMusicVolume(newVolume);
                break;

            case ESoundType.SFX:
                SoundManager.Instance.ChangeSFXVolume(newVolume);
                break;
        }
    }
}
