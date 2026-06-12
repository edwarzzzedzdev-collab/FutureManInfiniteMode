using UnityEngine;
using System.IO; // Necesario para la lectura y escritura de archivos

// 1. LA CLASE CON LOS DATOS PUROS QUE IRÁN AL JSON
[System.Serializable]
public class GameData
{
    public int totalCoins = 0;

    // Puedes dejar esto aquí como comentario para el futuro:
    // public System.Collections.Generic.List<string> unlockedCharacters;
    // public System.Collections.Generic.List<string> unlockedWeapons;
}

// 2. EL CONTROLADOR QUE GESTIONA EL ARCHIVO
public class DataController : MonoBehaviour
{
    // Patrón Singleton para que cualquier script pueda acceder al DataController fácilmente
    public static DataController Instance { get; private set; }

    [Header("Datos del Juego")]
    public GameData savedData = new GameData();

    private string saveFilePath;

    void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Hace que este objeto no se destruya al cambiar de escena
            
            // Definimos la ruta del archivo JSON de forma automática según la plataforma
            saveFilePath = Path.Combine(Application.persistentDataPath, "GameSave.json");
            
            // Cargamos los datos en cuanto el script se despierta
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- MÉTODOS PARA MODIFICAR DATOS ---

    public void AddCoins(int amount)
    {
        savedData.totalCoins += amount;
        SaveGame(); // Guardamos automáticamente al recibir monedas (puedes cambiar esto si prefieres guardar solo al final)
    }

    // --- MÉTODOS DE CONEXIÓN CON EL JSON ---

    [ContextMenu("Guardar Juego")] // Te permite probar el guardado desde el Inspector haciendo clic derecho en el componente
    public void SaveGame()
    {
        try
        {
            // Convertimos el objeto 'savedData' a texto JSON legible (gracias al 'true')
            string jsonText = JsonUtility.ToJson(savedData, true);
            
            // Escribimos el archivo de texto en la ruta asignada
            File.WriteAllText(saveFilePath, jsonText);
            
            Debug.Log($"<color=green><b>[DataController]</b></color> Juego guardado en: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataController] Error al guardar el juego: {e.Message}");
        }
    }

    public void LoadGame()
    {
        // Verificamos si el archivo JSON ya existe en el dispositivo
        if (File.Exists(saveFilePath))
        {
            try
            {
                // Leemos el texto completo del JSON
                string jsonText = File.ReadAllText(saveFilePath);
                
                // Reconstruimos la clase 'savedData' con el texto que leímos
                savedData = JsonUtility.FromJson<GameData>(jsonText);
                
                Debug.Log("<color=green><b>[DataController]</b></color> Datos del juego cargados correctamente.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DataController] Error al cargar el juego: {e.Message}");
                // Si el archivo está corrupto por alguna razón, creamos datos nuevos limpios para evitar que el juego falle
                savedData = new GameData();
            }
        }
        else
        {
            // Si el jugador abre el juego por primera vez, creamos un archivo JSON vacío con 0 monedas
            savedData = new GameData();
            Debug.Log("[DataController] No se encontró guardado previo. Creando archivo inicial...");
            SaveGame();
        }
    }
}