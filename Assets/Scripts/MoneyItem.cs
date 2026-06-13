using UnityEngine;

// Forzamos a que el objeto tenga lo necesario para no pinchar
[RequireComponent(typeof(MeshRenderer))] 
public class MoneyItem : MonoBehaviour
{
    [Header("Inyección de Datos (Cerebro)")]
    [SerializeField] private CoinData data;

    [Header("Eje de Rotación Manual")]
    [Tooltip("Usa (0,1,0) para Y, (1,0,0) para X, o (0,0,1) para Z según cómo gire bien")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        InitializeCoin();
    }

    private void InitializeCoin()
    {
        if (data == null)
        {
            Debug.LogWarning($"¡Falta asignar el ScriptableObject en {gameObject.name}!");
            return;
        }

        // Le aplicamos el material del ScriptableObject a la malla nativa
        if (meshRenderer != null && data.coinMaterial != null)
        {
            meshRenderer.material = data.coinMaterial;
        }
    }

    private void Update()
    {
        if (data == null) return;

        // Rotación ultra ligera en espacio local
        transform.Rotate(rotationAxis * data.rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameOverController.Instance != null && data != null)
            {
                // Enviamos el valor económico puro
                GameOverController.Instance.AddCoinFromRun(data.coinValue);
            }

            Destroy(gameObject);
        }
    }
}