using UnityEngine;
using UnityEngine.SceneManagement; // Corregido: Usamos el namespace estándar directamente

public class HudController : MonoBehaviour
{
    [Header("Configuración de URLs y Escenas")]
    public string socialXURL = "https://x.com/Edwarzzz_EDZDEV";
    public string nameSceneStart = "StartScene"; 
    
    [Header("Componentes de UI")]
    public GameObject panelConfig;

    // Cambiamos 'void' por 'System.Collections.IEnumerator' para que el truco del yield funcione
    private System.Collections.IEnumerator Start()
    {
        // Truco para forzar a la UI a inicializar sus componentes hijos (como TextMeshPro o Layouts)
        panelConfig.SetActive(true); 
        yield return null; // Espera exactamente un cuadro (frame), es más limpio que 0.0001s
        panelConfig.SetActive(false);
    }

    // IMPORTANTE: Tienen que ser 'public' para que aparezcan en el evento OnClick() del botón en el Inspector
    public void OnPressRedX()
    {
        Application.OpenURL(socialXURL);
        Debug.Log("Abriendo Red X: " + socialXURL);
    }

    public void OnPressStartBtn()
    {
        // Llama al FadeManager global, este cerrará la pantalla, cargará la escena y la volverá a abrir
        FadeManager.Instance.ChangeSceneWithFade(nameSceneStart);
    }

    public void OnPressExitBtn()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego... (Esto solo funciona en el APK/Build, no en el Editor)");
    }

    public void OpenConfigPanel()
    {
        panelConfig.SetActive(true);
    }

    public void CloseConfigPanel()
    {
        panelConfig.SetActive(false);
    }
}