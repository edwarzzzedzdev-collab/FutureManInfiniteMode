using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance { get; private set; }

    [Header("UI Componentes")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI projectedPointsText;
    public TextMeshProUGUI projectedCoinsText;
    public TextMeshProUGUI projectedTimeText;
    private TextMeshProUGUI realTimeCoinText;

    [Header("Estadísticas de la Partida")]
    public int coinsByRun = 0;
    public float playerTime = 0f;
    
    private bool isTimerRunning = false;
    private bool timerStarted = false;
    private bool isPlayerDead = false;

    // Variables para el rastreo de posición
    private GameObject playerRef;
    private Vector3 initialPlayerPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            ResetAndInitializeRound();
        }
    }

    // Soporte para pruebas directas en el Editor de Unity
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            ResetAndInitializeRound();
        }
    }

    void ResetAndInitializeRound()
    {
        coinsByRun = 0;
        playerTime = 0f;
        timerStarted = false;
        isTimerRunning = false;
        isPlayerDead = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Buscamos al Player y guardamos su posición exacta de aparición
        playerRef = GameObject.FindWithTag("Player");
        if (playerRef != null)
        {
            initialPlayerPosition = playerRef.transform.position;
        }
        if (realTimeCoinText == null)
    {
        GameObject coinTagObject = GameObject.FindWithTag("RealTimeCoinText");
        if (coinTagObject != null)
        {
            realTimeCoinText = coinTagObject.GetComponent<TextMeshProUGUI>();
        }
    }

    // Inicializamos el contador visual en 0
    viewCoins();
        
    }

    void Update()
    {
        if (isPlayerDead) return;

        // --- DETECCIÓN POR CAMBIO DE POSICIÓN ---
        if (playerRef != null && !timerStarted)
        {
            // Calculamos la distancia entre la posición actual y la inicial
            float distanceMoved = Vector3.Distance(playerRef.transform.position, initialPlayerPosition);

            // Si se ha movido más de 0.1 unidades (un margen para ignorar si el personaje se asienta en el suelo por gravedad)
            if (distanceMoved > 0.1f)
            {
                StartTimer();
            }
        }

        if (isTimerRunning)
        {
            playerTime += Time.deltaTime;
        }
    }

    public void StartTimer()
    {
        timerStarted = true;
        isTimerRunning = true;
        Debug.Log("<color=green><b>[GameOverController]</b></color> ¡El jugador se ha movido! Cronómetro iniciado.");
    }

    // Reemplaza este método en tu GameOverController
public void AddCoinFromRun(int value)
{
    if (isPlayerDead) return;
    
    coinsByRun += value; 
    
    viewCoins(); 
}

    public void viewCoins()
{
    if (realTimeCoinText != null)
    {
        realTimeCoinText.text = "Coins:" + coinsByRun.ToString();
    }
}

public void RegisterRealTimeCoinText(TextMeshProUGUI textComponent)
{
    realTimeCoinText = textComponent;
    viewCoins(); // Actualiza el texto inmediatamente con el valor actual
}

    public void TriggerGameOver()
    {
        isPlayerDead = true;
        isTimerRunning = false; 

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        UpdateGameOverUI();

        if (DataController.Instance != null)
        {
            DataController.Instance.AddCoins(coinsByRun);
        }
    }

    void UpdateGameOverUI()
    {
        int finalPoints = coinsByRun * 100;

        if (projectedPointsText != null) projectedPointsText.text = finalPoints.ToString();
        if (projectedCoinsText != null) projectedCoinsText.text = coinsByRun.ToString();
        
        if (projectedTimeText != null)
        {
            int minutes = Mathf.FloorToInt(playerTime / 60f);
            int seconds = Mathf.FloorToInt(playerTime % 60f);
            projectedTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }


    // Método para reiniciar el juego desde el Game Over
    public void RestartGame()
    {
        if (FadeManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            FadeManager.Instance.ChangeSceneWithFade(currentSceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    public void ReturnToMainMenu()
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.ChangeSceneWithFade("StartScene");
        }
        else
        {
            SceneManager.LoadScene("StartScene");
        }
    }
    public void Closepanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
   
}