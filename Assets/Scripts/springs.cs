using UnityEngine;
using System.Collections;

public class springs : MonoBehaviour
{
    [Header("Perfil de Datos")]
    [SerializeField] private SpringData data;
    
    private bool isReady = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isReady)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplySpringJump(data.springForce);
                StartCoroutine(SpringCooldown());
            }
        }
    }

    private IEnumerator SpringCooldown()
    {
        isReady = false;
        yield return new WaitForSeconds(0.2f); // Tiempo suficiente para que el player suba y salga del trigger
        isReady = true;
    }
}