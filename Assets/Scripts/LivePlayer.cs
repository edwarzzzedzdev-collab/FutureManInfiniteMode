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
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float knockbackUpwardForce = 6f; // <-- NUEVA: Fuerza del pequeño salto vertical
    [Tooltip("Tiempo que el jugador perderá el control para ser empujado")]
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Componentes e Interfaz")]
    public GameObject liveBar;
    public GameObject diePrefab;

    private Vector3 initialBarScale;
    private PlayerController playerController;

    private struct RendererData
    {
        public Renderer renderer;
        public Material[] originalMaterials;
    }
    private RendererData[] cachedRenderers;

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
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();

        if (liveBar != null)
        {
            initialBarScale = liveBar.transform.localScale;
        }
    }

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
            StartCoroutine(DamageEffectsRoutine(damageSourcePosition));
        }
    }

    private IEnumerator DamageEffectsRoutine(Vector3 damageSourcePosition)
    {
        isInvincible = true;

        // --- RETROCESO MEJORADO CON IMPULSO VERTICAL ---
        if (playerController != null)
        {
            Vector3 knockbackDir = (transform.position - damageSourcePosition);
            knockbackDir.y = 0; 
            knockbackDir.Normalize();

            // Enviamos tanto la fuerza horizontal como la nueva fuerza vertical hacia arriba
            playerController.ApplyKnockback(knockbackDir, knockbackForce, knockbackUpwardForce, knockbackDuration);
        }

        // --- EFECTO VISUAL ---
        ToggleFlashEffect(true);
        yield return new WaitForSeconds(0.15f); 
        ToggleFlashEffect(false);

        // --- ESPERA DE INVULNERABILIDAD ---
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

        if (GameOverController.Instance != null)
        {
            GameOverController.Instance.TriggerGameOver();
        }

        if (diePrefab != null) Instantiate(diePrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (isDead || isInvincible) return;

        if (other.CompareTag("Damage"))
        {
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