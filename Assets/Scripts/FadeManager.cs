using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class FadeManager : MonoBehaviour
{
    // Patrón Singleton: Permite que cualquier script acceda a este Manager fácilmente
    public static FadeManager Instance { get; private set; }

    [Header("Configuración del Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        // Regla del Singleton: Si ya existe otro FadeManager, se destruye el nuevo para no duplicar
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // ¡LA MAGIA! Esto hace que este GameObject y su Canvas sobrevivan al cambiar de escena
        DontDestroyOnLoad(gameObject);

        // Nos suscribimos al evento de Unity que detecta cuándo se termina de cargar CUALQUIER escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update() {
        PruebaFade();
    }

    private void OnDestroy()
    {
        // Buena práctica: Desuscribirse del evento al destruir el objeto para evitar errores de memoria
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Se ejecuta AUTOMÁTICAMENTE cada vez que entras a una nueva escena
   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    StartCoroutine(Fade(0f));

    // Control automático de música ambiental
    if (scene.name == "StartScene")
    {
        AudioManager.Instance.PlayMenuMusic();
    }
    else if (scene.name == "Game") // Nombre de tu escena procedimental
    {
        AudioManager.Instance.StartGamePlaylist();
    }
}

    // Método PÚBLICO que llamarás desde tus botones (ej: desde el HudController)
    public void ChangeSceneWithFade(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        // 1. Fade Out: De Transparente a Negro
        yield return StartCoroutine(Fade(1f));

        // 2. Cargamos la escena en background
        SceneManager.LoadScene(sceneName);
    }

    // Corrutina universal para mover el Alpha de la imagen de forma fluida
    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        // Si va hacia negro (1), bloquea la pantalla para que el jugador no pueda hacer clics fantasmas
        fadeCanvasGroup.blocksRaycasts = (targetAlpha > 0);

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null; // Espera al siguiente frame
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }


    private void PruebaFade()
    {
        // Forma correcta en el Nuevo Input System para detectar una tecla rápido
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            string escenaActual = SceneManager.GetActiveScene().name;
            
            Debug.Log("Probando Fade con nuevo Input System. Reiniciando: " + escenaActual);
            
            ChangeSceneWithFade(escenaActual);
        }
    }
}