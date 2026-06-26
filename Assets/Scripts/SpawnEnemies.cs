using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    [Header("Enemy Pools")]
    public GameObject[] flyEnemies;
    public GameObject[] earthEnemies;

    [Header("Raycast Settings")]
    public float distanceRay = 100f;
    public LayerMask groundLayer;

    private bool hasSpawned = false;
    private bool hitSomething = false;

    void Start()
    {
        Physics.SyncTransforms();
        ExecuteSpawn();
    }

    void Update()
    {
        Color rayColor = hitSomething ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector3.down * distanceRay, rayColor);
    }

    void ExecuteSpawn()
    {
        if (hasSpawned) return;

        RaycastHit hit;
        hitSomething = Physics.Raycast(transform.position, Vector3.down, out hit, distanceRay, groundLayer);

        if (hitSomething)
        {
            Debug.Log($"<color=green><b>[SUELO DETECTADO 3D]</b></color> El spawn '{gameObject.name}' tocó a '{hit.collider.name}'.");
            Spawn(earthEnemies, hit.point);
        }
        else
        {
            Debug.Log($"<color=orange><b>[VACÍO 3D]</b></color> Spawneando volador.");
            Spawn(flyEnemies, transform.position);
        }

        hasSpawned = true;
    }

    void Spawn(GameObject[] enemyPool, Vector3 spawnPosition)
    {
        if (enemyPool == null || enemyPool.Length == 0) return;

        int randomIndex = Random.Range(0, enemyPool.Length);
        GameObject enemyPrefab = enemyPool[randomIndex];

        if (enemyPrefab != null)
        {
            // CORRECCIÓN 1: Instanciamos con la rotación NATIVA del prefab, no con Quaternion.identity
            GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, enemyPrefab.transform.rotation);
            
            if (transform.parent != null)
            {
                // Guardamos sus proporciones y rotación originales antes de emparentar
                Vector3 originalLocalScale = spawnedEnemy.transform.localScale;
                Quaternion originalLocalRotation = spawnedEnemy.transform.localRotation;

                spawnedEnemy.transform.SetParent(transform.parent);

                // CORRECCIÓN 2: Forzamos que mantenga su escala y rotación local limpia.
                // Así, si la sala está en espejo, el enemigo NO se gira ni cambia su mirada.
                spawnedEnemy.transform.localScale = originalLocalScale;
                spawnedEnemy.transform.localRotation = originalLocalRotation;
            }
        }
    }
}