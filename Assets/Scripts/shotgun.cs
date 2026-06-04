using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Necesario para detectar el InputValue

public class Shotgun : MonoBehaviour
{
    [Header("Componentes del Arma")]
    public GameObject damageArea;
    [SerializeField] private ParticleSystem shootParticles; 

    [Header("Configuración del Disparo")]
    [Tooltip("Tiempo en segundos que el área de daño permanecerá activa")]
    [SerializeField] private float activeDuration = 0.15f; 
    [SerializeField] private AudioClip shootSound; 

    private Coroutine attackCoroutine;

    void Start()
    {
        if (damageArea != null)
        {
            damageArea.SetActive(false);
        }
    }

    // Escucha directamente la acción "Attack" del PlayerInput (si está en el mismo objeto)
    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Fire();
        }
    }

    // Método principal de disparo. Centraliza partículas, audio y daño.
    public void Fire()
    {
        // 1. Efecto de Partículas (Muzzle Flash / Perdigones)
        if (shootParticles != null)
        {
            shootParticles.Play();
        }

        // 2. Reproducción de Audio con Logs de validación
        if (AudioManager.Instance != null && shootSound != null)
        {
            AudioManager.Instance.PlaySFX(shootSound);
            Debug.Log("<color=green>Shotgun:</color> ¡Boom! Sonido enviado al AudioManager.");
        }
        else if (AudioManager.Instance == null)
        {
            Debug.LogWarning("Shotgun: No se encontró el AudioManager en la escena.");
        }

        // 3. Ráfaga del área de daño (Trigger)
        if (damageArea != null)
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(BurstDamageArea());
        }
    }

    private IEnumerator BurstDamageArea()
    {
        damageArea.SetActive(true);
        yield return new WaitForSeconds(activeDuration);
        damageArea.SetActive(false);
    }
}