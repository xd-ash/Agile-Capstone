using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject otherUIElements;
    
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            if (masterSlider != null)
            {
                masterSlider.value = AudioManager.instance.masterVolume;
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.instance.sfxVolume;
            }

            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.instance.musicVolume;
            }
        }

        // Hook up listeners
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
    }

    private void OnDestroy()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        }
    }
    
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        otherUIElements.SetActive(false);
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        otherUIElements.SetActive(true);
    }
    

    private void OnMasterChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMasterVolume(value);
        }
    }

    private void OnSfxChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSfxVolume(value);
        }
    }

    private void OnMusicChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(value);
        }
    }
}