using System.Collections;
using UnityEngine;

public class LiveEnemy : MonoBehaviour
{
    [Header("Perfil de Datos")]
    [SerializeField] private Enemy1Data data; 

    // Estados de instancia individuales
    private int currentHealth;
    private bool isDead = false;

    private struct RendererData
    {
        public Renderer renderer;
        public Material[] originalMaterials;
    }

    private RendererData[] cachedRenderers;
    private MonoBehaviour movementScript; 

    void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        cachedRenderers = new RendererData[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            cachedRenderers[i].renderer = renderers[i];
            cachedRenderers[i].originalMaterials = renderers[i].sharedMaterials;
        }
    }

    void Start()
    {
        if (data == null) return;
        
        currentHealth = data.maxHealth; // La vida inicial se copia de la configuración estática
        movementScript = GetComponent<Enemi1Move>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDead && other.CompareTag("Attack"))
        {
            ApplyDamage(1);
        }
    }

    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StopAllCoroutines(); 
            StartCoroutine(HitEffectRoutine());
        }
    }

    private IEnumerator HitEffectRoutine()
    {
        if (movementScript != null) movementScript.enabled = false;

        ToggleFlashEffect(true);
        yield return new WaitForSeconds(data.flashDuration);
        ToggleFlashEffect(false);

        yield return new WaitForSeconds(data.stunDuration - data.flashDuration);
        if (!isDead && movementScript != null) movementScript.enabled = true;
    }

    private void ToggleFlashEffect(bool enableFlash)
    {
        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i].renderer == null) continue;

            if (enableFlash)
            {
                Material[] flashArray = new Material[cachedRenderers[i].originalMaterials.Length];
                for (int j = 0; j < flashArray.Length; j++)
                {
                    flashArray[j] = data.flashMaterial;
                }
                cachedRenderers[i].renderer.materials = flashArray;
            }
            else
            {
                cachedRenderers[i].renderer.materials = cachedRenderers[i].originalMaterials;
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (data.explosionPrefab != null)
        {
            Instantiate(data.explosionPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log($"<color=black><b>[Enemigo]</b></color> {gameObject.name} explotó.");
        Destroy(gameObject);
    }
}