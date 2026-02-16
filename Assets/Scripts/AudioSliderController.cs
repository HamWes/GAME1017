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
    [SerializeField] private ESoundType soundType;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        ChangeSoundVolume(slider.value);
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
                break;

            case ESoundType.SFX:
                GameManager.Instance.SoundManager.ChangeSFXVolume(newVolume);
                break;
        }
    }
}
