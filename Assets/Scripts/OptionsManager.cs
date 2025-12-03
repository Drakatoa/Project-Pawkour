using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("Master Volume")]
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeValue;
    
    [Header("SFX Volume")]
    public Slider sfxSlider;
    public TextMeshProUGUI sfxValue;
    
    [Header("Music Volume")]
    public Slider musicSlider;
    public TextMeshProUGUI musicValue;
    
    [Header("Mouse Sensitivity")]
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI mouseSensitivityValue;
    
    void Start()
    {
        // 1. Setup listeners FIRST
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);

        // 2. Then load the saved data
        LoadSettings();
    }
    
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        if (masterVolumeValue != null)
            masterVolumeValue.text = Mathf.RoundToInt(volume * 100).ToString();
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        if (sfxValue != null)
            sfxValue.text = Mathf.RoundToInt(volume * 100).ToString();
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        if (musicValue != null)
            musicValue.text = Mathf.RoundToInt(volume * 100).ToString();
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    public void SetMouseSensitivity(float value)
    {
        if (mouseSensitivityValue != null)
            mouseSensitivityValue.text = Mathf.RoundToInt(value * 100).ToString();
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.Save();
        Debug.Log("Settings saved!");
    }
    
    public void CancelSettings()
    {
        LoadSettings();
        Debug.Log("Settings cancelled - reverted to saved values");
    }
    
    void LoadSettings()
    {
        // Load saved values or use defaults (1.0 = 100%)
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float mouseSens = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        
        // --- MASTER VOLUME ---
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVol;
        // FIX: Manually update the text right now
        if (masterVolumeValue != null)
            masterVolumeValue.text = Mathf.RoundToInt(masterVol * 100).ToString();
            
        // --- SFX VOLUME ---
        if (sfxSlider != null)
            sfxSlider.value = sfxVol;
        // FIX: Manually update the text right now
        if (sfxValue != null)
            sfxValue.text = Mathf.RoundToInt(sfxVol * 100).ToString();
            
        // --- MUSIC VOLUME ---
        if (musicSlider != null)
            musicSlider.value = musicVol;
        // FIX: Manually update the text right now
        if (musicValue != null)
            musicValue.text = Mathf.RoundToInt(musicVol * 100).ToString();
            
        // --- MOUSE SENSITIVITY ---
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = mouseSens;
        // FIX: Manually update the text right now
        if (mouseSensitivityValue != null)
            mouseSensitivityValue.text = Mathf.RoundToInt(mouseSens * 100).ToString();
        
        // Apply master volume immediately to the game engine
        AudioListener.volume = masterVol;
    }
}