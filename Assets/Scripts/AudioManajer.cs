using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Fuentes de Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Lista de Música")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip[] gamePlaylist; 

    private List<AudioClip> shuffledPlaylist = new List<AudioClip>();
    private int currentTrackIndex = 0;
    private Coroutine playlistCoroutine;

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

    // ==================== SECCIÓN MÚSICA ====================

  public void PlayMenuMusic()
    {
        if (playlistCoroutine != null) StopCoroutine(playlistCoroutine);
        
        musicSource.Stop(); // ◄── NUEVO: Limpia el reproductor antes de empezar
        musicSource.loop = true;
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void StartGamePlaylist()
    {
        if (playlistCoroutine != null) StopCoroutine(playlistCoroutine);
        
        musicSource.Stop(); // ◄── NUEVO: Detiene la música del menú inmediatamente
        musicSource.loop = false; 
        ResetAndShufflePlaylist();
        playlistCoroutine = StartCoroutine(PlaylistRoutine());
    }

    private IEnumerator PlaylistRoutine()
    {
        while (true)
        {
            if (shuffledPlaylist.Count == 0)
            {
                yield return null;
                continue;
            }

            // SOLUClÓN: Si no está sonando nada, reproduce la siguiente canción completa
            if (!musicSource.isPlaying)
            {
                musicSource.clip = shuffledPlaylist[currentTrackIndex];
                musicSource.Play();
                Debug.Log($"<color=green>AudioManager:</color> Sonando ahora: {musicSource.clip.name}");

                // Avanza al siguiente track
                currentTrackIndex++;
                if (currentTrackIndex >= shuffledPlaylist.Count)
                {
                    ResetAndShufflePlaylist(); // Si se acaban las 4, baraja de nuevo
                }
            }

            // Espera al siguiente frame (Inmune a Time.timeScale = 0)
            yield return null; 
        }
    }

    private void ResetAndShufflePlaylist()
    {
        shuffledPlaylist = new List<AudioClip>(gamePlaylist);
        
        for (int i = 0; i < shuffledPlaylist.Count; i++)
        {
            AudioClip temp = shuffledPlaylist[i];
            int randomIndex = Random.Range(i, shuffledPlaylist.Count);
            shuffledPlaylist[i] = shuffledPlaylist[randomIndex];
            shuffledPlaylist[randomIndex] = temp;
        }
        currentTrackIndex = 0;
    }

    // ==================== SECCIÓN SFX ====================

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip); 
        }
    }
}