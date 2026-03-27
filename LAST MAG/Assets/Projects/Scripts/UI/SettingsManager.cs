using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private AudioMixer _masterMixer;

    public float MouseSensitivity { get; private set; } = 2f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        LoadSettings();
    }

    public void SetMouseSensitivity(float val)
    {
        MouseSensitivity = val;
        PlayerPrefs.SetFloat("MouseSens", val);
    }

    public void SetMasterVolume(float val)
    {
        float dB = Mathf.Log10(Mathf.Clamp(val, 0.0001f, 1f)) * 20f;

        if (_masterMixer != null)
            _masterMixer.SetFloat("MasterVol", dB);

        PlayerPrefs.SetFloat("MasterVol", val);
    }

    private void LoadSettings()
    {
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSens", 2f);
        float savedVol = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        SetMasterVolume(savedVol);
    }
}