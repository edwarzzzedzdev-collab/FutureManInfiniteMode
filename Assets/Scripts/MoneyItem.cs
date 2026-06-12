using UnityEngine;

public class MoneyItem : MonoBehaviour
{
    [Header("Configuración Visual (Opcional)")]
    [SerializeField] private float rotationSpeed = 100f;

    void Update()
    {
        // Un pequeño toque estético: la moneda gira sobre su propio eje Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto que atravesó la moneda tiene el Tag del Player
        if (other.CompareTag("Player"))
        {
            // Validamos que el GameOverController exista en la escena antes de llamarlo
            if (GameOverController.Instance != null)
            {
                GameOverController.Instance.AddCoinFromRun();
            }

            
            Destroy(gameObject);
        }
    }
}