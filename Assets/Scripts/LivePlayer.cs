using System.Collections;
using UnityEngine;

public class LivePlayer : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("Invulnerabilidad y Efectos")]
    public float invincibilityDuration = 1.0f;
    [SerializeField] private Material flashMaterial;
    private bool isInvincible = false;

    [Header("Configuración de Retroceso (Knockback)")]
    [SerializeField] private float knockbackForce = 8f;
    [Tooltip("Tiempo que el jugador perderá el control para ser empujado")]
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Componentes e Interfaz")]
    public GameObject liveBar;
    public GameObject diePrefab;

    private Vector3 initialBarScale;
    private Rigidbody rb;

    // Estructura para almacenar los materiales de cada pieza del traje/cuerpo del Player
    private struct RendererData
    {
        public Renderer renderer;
        public Material[] originalMaterials;
    }
    private RendererData[] cachedRenderers;

    void Awake()
    {
        // 1. Guardamos de forma automática todos los materiales originales del Player
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
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();

        if (liveBar != null)
        {
            initialBarScale = liveBar.transform.localScale;
        }
    }

    // Ahora este método exige saber la POSICIÓN del daño para calcular el retroceso
    public void TakeDamage(int damageAmount, Vector3 damageSourcePosition)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Iniciamos la rutina que controla el combo de efectos post-daño
            StartCoroutine(DamageEffectsRoutine(damageSourcePosition));
        }
    }

    private IEnumerator DamageEffectsRoutine(Vector3 damageSourcePosition)
    {
        isInvincible = true;

        // --- EFECTO 1: RETROCESO (KNOCKBACK) ---
        if (rb != null)
        {
            // Calculamos la dirección opuesta al origen del daño
            Vector3 knockbackDir = (transform.position - damageSourcePosition);
            knockbackDir.y = 0; // Evitamos que el jugador salga volando hacia arriba
            knockbackDir.Normalize();

            // Reseteamos la velocidad previa para que el impacto sea seco y predecible
            rb.velocity = Vector3.zero; 
            
            // Aplicamos un impulso físico inmediato
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }

        // --- EFECTO 2: DESTELLO VISUAL ---
        ToggleFlashEffect(true);
        
        // El flash visual suele durar menos que toda la invencibilidad para no cansar la vista
        yield return new WaitForSeconds(0.15f); 
        ToggleFlashEffect(false);

        // --- EFECTO 3: ESPERA DE INVULNERABILIDAD ---
        // Restamos el tiempo que ya gastamos en el flash visual
        float remainingInvincibility = invincibilityDuration - 0.15f;
        if (remainingInvincibility > 0)
        {
            yield return new WaitForSeconds(remainingInvincibility);
        }

        isInvincible = false;
    }

    private void ToggleFlashEffect(bool enableFlash)
    {
        if (flashMaterial == null) return;

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i].renderer == null) continue;

            if (enableFlash)
            {
                Material[] flashArray = new Material[cachedRenderers[i].originalMaterials.Length];
                for (int j = 0; j < flashArray.Length; j++)
                {
                    flashArray[j] = flashMaterial;
                }
                cachedRenderers[i].renderer.materials = flashArray;
            }
            else
            {
                cachedRenderers[i].renderer.materials = cachedRenderers[i].originalMaterials;
            }
        }
    }

    public void AddLive()
    {
        if (isDead || currentHealth >= maxHealth) return;

        currentHealth++;
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (liveBar != null)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            liveBar.transform.localScale = new Vector3(
                initialBarScale.x * healthPercentage, 
                initialBarScale.y, 
                initialBarScale.z
            );
        }
    }

    private void Die()
    {
        isDead = true;

        if (diePrefab != null)
        {
            Instantiate(diePrefab, transform.position, transform.rotation);
        }

        Debug.Log("<color=red><b>[Player]</b></color> El jugador ha muerto.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) 
    {
        // Si el jugador ya está en su tiempo de invulnerabilidad, el trigger ignora los ataques por completo
        if (isDead || isInvincible) return;

        if (other.CompareTag("Damage"))
        {
            // Pasamos la posición del objeto dañino para saber de dónde viene el golpe
            TakeDamage(1, other.transform.position);
        }
        else if (other.CompareTag("Damage2"))
        {
            TakeDamage(2, other.transform.position);
        }
        else if (other.CompareTag("Live"))
        {
            AddLive();
            Destroy(other.gameObject);
        }
    }
}