using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer _masterMixer;

    public float MouseSensitivity { get; private set; } = 2f;
    public float AimSensitivity { get; private set; } = 0.8f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    public void SetMouseSensitivity(float val)
    {
        MouseSensitivity = val;
        PlayerPrefs.SetFloat("MouseSens", val);
    }

    public void SetAimSensitivity(float val)
    {
        AimSensitivity = val;
        PlayerPrefs.SetFloat("AimSens", val);
    }

    public void SetMasterVolume(float val)
    {
        float db = val > 0 ? Mathf.Log10(val / 100f) * 20f : -80f;
        _masterMixer?.SetFloat("MasterVol", db);
        PlayerPrefs.SetFloat("MasterVol", val);
    }

    public void SetSFXVolume(float val)
    {
        float db = val > 0 ? Mathf.Log10(val / 100f) * 20f : -80f;
        _masterMixer?.SetFloat("SFXVol", db);
        PlayerPrefs.SetFloat("SFXVol", val);
    }

    public void SetMusicVolume(float val)
    {
        float db = val > 0 ? Mathf.Log10(val / 100f) * 20f : -80f;
        _masterMixer?.SetFloat("MusicVol", db);
        PlayerPrefs.SetFloat("MusicVol", val);
    }

    private void Load()
    {
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSens", 2f);
        AimSensitivity = PlayerPrefs.GetFloat("AimSens", 0.8f);
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVol", 80f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 70f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 50f));
    }
}
