using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    [Header("Item Prefabs")]
    public GameObject coin1Prefab;       // Moneda común (Valor 1)
    public GameObject coin3Prefab;       // Moneda rara (Valor 3)
    public GameObject coin5Prefab;       // Moneda épica (Valor 5)
    public GameObject healingPrefab;     // Curación (¡Súper extraña!)

    [Header("Drop Chances (0% to 100%)")]
    [Range(0f, 100f)]
    [Tooltip("Very low chance for healing items to make them extremely rare.")]
    public float chanceHealing = 1.0f;   // 1% de probabilidad por defecto

    [Range(0f, 100f)]
    [Tooltip("Chance for the highest value coin (Value 5).")]
    public float chanceCoin5 = 7.0f;     // 7% de probabilidad por defecto

    [Range(0f, 100f)]
    [Tooltip("Chance for the medium value coin (Value 3).")]
    public float chanceCoin3 = 22.0f;    // 22% de probabilidad por defecto

    // NOTA: No necesitamos una variable para la moneda de 1. 
    // Todo el porcentaje restante (70% en este ejemplo) será automáticamente para ella.

    void Start()
    {
        DetermineAndSpawnItem();
    }

    void DetermineAndSpawnItem()
    {
        // Tiramos un dado con decimales entre 0.0 y 100.0
        float roll = Random.Range(0f, 100f);
        GameObject selectedPrefab = null;

        // Evaluamos usando rangos acumulativos
        if (roll < chanceHealing)
        {
            // Rango: 0 a 1.0 -> Curación
            selectedPrefab = healingPrefab;
        }
        else if (roll < chanceHealing + chanceCoin5)
        {
            // Rango: 1.0 a 8.0 -> Moneda de 5
            selectedPrefab = coin5Prefab;
        }
        else if (roll < chanceHealing + chanceCoin5 + chanceCoin3)
        {
            // Rango: 8.0 a 30.0 -> Moneda de 3
            selectedPrefab = coin3Prefab;
        }
        else
        {
            // Rango: 30.0 a 100.0 -> Moneda de 1 (El resto del pastel)
            selectedPrefab = coin1Prefab;
        }

        // Proceso de instanciado seguro y escalado
        if (selectedPrefab != null)
        {
            GameObject spawnedItem = Instantiate(selectedPrefab, transform.position, Quaternion.identity);
            
            // Se vuelve hijo para mantener el orden y heredar el borrado infinito de memoria
            spawnedItem.transform.SetParent(this.transform);

            // Reseteamos la escala local a 1 para que se multiplique perfecto por el 1.5 del chunk
            spawnedItem.transform.localScale = Vector3.one;
        }
        else
        {
            Debug.LogWarning($"SpawnItem en {gameObject.name}: El prefab seleccionado no está asignado en el Inspector.");
        }
    }
}