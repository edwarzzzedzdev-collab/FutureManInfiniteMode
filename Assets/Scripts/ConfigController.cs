using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.Audio; 

public class ConfigController : MonoBehaviour
{
    public static ConfigController Instance { get; private set; }

    [Header("Componentes de Interfaz")]
    [SerializeField] private GameObject configPanel;
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Slider sliderEffects;

    [Header("Control de Audio Profesional")]
    [SerializeField] private AudioMixer audioMixer; 

    // Variables internas para rastrear el estado del Mute
    private bool isMusicMuted = false;
    private bool isEffectsMuted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CloseConfig();

        // Cargar volumen guardado o usar 0.75f por defecto
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        if (sliderMusic != null) 
        {
            sliderMusic.value = savedMusic;
            sliderMusic.onValueChanged.AddListener(SetMusicVolume);
            SetMusicVolume(savedMusic); 
        }
        
        if (sliderEffects != null) 
        {
            sliderEffects.value = savedSFX;
            sliderEffects.onValueChanged.AddListener(SetEffectsVolume);
            SetEffectsVolume(savedSFX); 
        }
    }

    // --- MÉTODOS DE INTERFAZ ---

    public void ActiveConfigPanel()
    {
        if (configPanel != null) configPanel.SetActive(true);
    }

    public void CloseConfig()
    {
        if (configPanel != null) configPanel.SetActive(false);
    }

    // --- MÉTODOS DE AUDIO ---

    public void SetMusicVolume(float value)
    {
        if (audioMixer == null) return;

        // Si el jugador mueve el slider manualmente, asumimos que quiere desmutear
        if (value > 0) isMusicMuted = false;

        float dB = value > 0 ? Mathf.Log10(value) * 20 : -80f;
        audioMixer.SetFloat("MusicVol", dB);

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetEffectsVolume(float value)
    {
        if (audioMixer == null) return;

        if (value > 0) isEffectsMuted = false;

        float dB = value > 0 ? Mathf.Log10(value) * 20 : -80f;
        audioMixer.SetFloat("SFXVol", dB);

        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // --- BOTONES DE MUTE (NUEVO) ---

    // Asigna esta función al botón de Mute de la Música
    public void VolMusicMute()
    {
        if (audioMixer == null || sliderMusic == null) return;

        isMusicMuted = !isMusicMuted; // Invierte el estado (si era true pasa a false y viceversa)

        if (isMusicMuted)
        {
            audioMixer.SetFloat("MusicVol", -80f); // Silencio absoluto
            Debug.Log("<color=yellow>ConfigController:</color> Música Muteada.");
        }
        else
        {
            // Restaura el volumen exacto que tiene el slider actualmente
            SetMusicVolume(sliderMusic.value);
            Debug.Log("<color=yellow>ConfigController:</color> Música Desmuteada.");
        }
    }

    // Asigna esta función al botón de Mute de los Efectos
    public void VolEffectsMute()
    {
        if (audioMixer == null || sliderEffects == null) return;

        isEffectsMuted = !isEffectsMuted;

        if (isEffectsMuted)
        {
            audioMixer.SetFloat("SFXVol", -80f);
            Debug.Log("<color=yellow>ConfigController:</color> Efectos Muteados.");
        }
        else
        {
            SetEffectsVolume(sliderEffects.value);
            Debug.Log("<color=yellow>ConfigController:</color> Efectos Desmuteados.");
        }
    }
}