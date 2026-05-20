using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Requerido para capturar la tecla de Pausa

public class HudInGameController : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [SerializeField] private string nameExitScene = "StartScene"; 

    [Header("Componentes de Interfaz")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private CanvasGroup pauseCanvasGroup; // Controlará la opacidad de la animación
    [SerializeField] private float animationDuration = 0.25f; // Duración del Fade

    private bool isPaused = false;

    private void Awake()
    {
        // Aseguramos un estado inicial limpio
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        // Soporte híbrido: Pausar con la tecla Escape o la tecla P
        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        
        // CONGELA EL TIEMPO GLOBAL: Afecta físicas, partículas, IA y Updates tradicionales
        Time.timeScale = 0f; 

        StopAllCoroutines();
        StartCoroutine(FadePauseMenu(1f, null));
    }

    public void Resume()
    {
        isPaused = false;
        
        // DEVUELVE EL TIEMPO A LA NORMALIDAD
        Time.timeScale = 1f; 

        StopAllCoroutines();
        // Desvanecemos el panel y lo apagamos al terminar la animación
        StartCoroutine(FadePauseMenu(0f, () => pausePanel.SetActive(false)));
    }

    public void Exit()
    {
        // ALERTA DE BUG CRÍTICO DE UNITY SOLUCIONADO:
        // Si cargas una escena mientras Time.timeScale es 0, la nueva escena nacerá congelada.
        Time.timeScale = 1f; 
        
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.ChangeSceneWithFade(nameExitScene);
        }
        else
        {
            SceneManager.LoadScene(nameExitScene);
        }
    }

    // Corrutina de animación inmune al congelamiento del tiempo
    private IEnumerator FadePauseMenu(float targetAlpha, System.Action onComplete)
    {
        if (pauseCanvasGroup == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float startAlpha = pauseCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Usamos unscaledDeltaTime porque es independiente de Time.timeScale
            elapsedTime += Time.unscaledDeltaTime; 
            pauseCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / animationDuration);
            yield return null;
        }

        pauseCanvasGroup.alpha = targetAlpha;
        onComplete?.Invoke(); // Ejecuta acciones secundarias (como desactivar el GameObject)
    }
}