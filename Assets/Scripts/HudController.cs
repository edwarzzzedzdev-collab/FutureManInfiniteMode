using UnityEngine;
using UnityEngine.SceneManagement; // Corregido: Usamos el namespace estándar directamente

public class HudController : MonoBehaviour
{
    [Header("Configuración de URLs y Escenas")]
    public string socialXURL = "https://x.com/Edwarzzz_EDZDEV";
    public string nameSceneStart = "StartScene"; 
    
    

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

      public void OpenConfig()
    {
        if (ConfigController.Instance != null)
        {
            ConfigController.Instance.ActiveConfigPanel();
        }
        else
        {
            Debug.LogError("No se encontró el ConfigController en la escena.");
        }
    }

   
}