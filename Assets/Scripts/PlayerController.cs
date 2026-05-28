using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 9f; // Aumenté un poco para más fluidez
    [SerializeField] private float acceleration = 50f; // Para que no sea instantáneo
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Mejoras de Sensación (Game Feel)")]
    [SerializeField] private float coyoteTime = 0.15f; // Tiempo de gracia al caer
    [SerializeField] private float jumpBufferTime = 0.2f; // Tiempo para "pre-presionar" el salto

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float currentHorizontalSpeed;

    private Vector2 moveInput;
    private Vector3 verticalVelocity;
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleTimers();
        HandleHorizontalMovement();
        ApplyGravity();
        CheckJump();



        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ShootTest();
        }
    }

    private void HandleTimers()
    {
        // Lógica de Coyote Time
        if (controller.isGrounded)
        {
            coyoteTimeCounter = coyoteTime; // Resetear contador mientras tocamos suelo
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; // Empezar a descontar al aire
        }

        // Lógica de Jump Buffer
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime; // Registramos la intención de saltar
        }
    }

    private void HandleHorizontalMovement()
    {
        float targetDirection = 0;
        if (moveInput.x > 0.1f) targetDirection = 1f;
        else if (moveInput.x < -0.1f) targetDirection = -1f;

        float targetSpeed = targetDirection * speed;
        
        // MoveTowards nos da una aceleración fluida en lugar de un cambio seco
        currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, targetSpeed, acceleration * Time.deltaTime);

        Vector3 moveDirection = new Vector3(currentHorizontalSpeed, 0, 0); 
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void CheckJump()
    {
        // Si hay un salto en el buffer Y estamos en el tiempo de gracia (Coyote Time)
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            verticalVelocity.y = jumpForce;
            jumpBufferCounter = 0; // Limpiamos el buffer para no saltar doble
            coyoteTimeCounter = 0; // Limpiamos el coyote para no saltar en el aire dos veces
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
// Requerido para capturar el teclado/ratón nuevo


    [Header("Prueba de Audio")]
    [SerializeField] private AudioClip shootSound; // Aquí arrastrarás el sonido del disparo (.wav/.mp3)

   

    void ShootTest()
    {
        // REGLA DE ORO: Validamos que el AudioManager exista antes de llamarlo
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(shootSound);
            Debug.Log("<color=green>Player:</color> ¡Piu! Sonido enviado al AudioManager.");
        }
        else
        {
            Debug.LogWarning("Player: No se encontró el AudioManager en la escena.");
        }
    }
}