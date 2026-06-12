using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 9f; 
    [SerializeField] private float acceleration = 50f; 
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Mejoras de Sensación (Game Feel)")]
    [SerializeField] private float coyoteTime = 0.15f; 
    [SerializeField] private float jumpBufferTime = 0.2f; 

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float currentHorizontalSpeed;

    private Vector2 moveInput;
    private Vector3 verticalVelocity;
    private CharacterController controller;

    // Variables para el control del retroceso
    private float knockbackTimer = 0f;
    private Vector3 knockbackDirection;
    private float knockbackForce;

    // FIX 2.5D: Variable para congelar el carril
    private float lockedZPosition;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Guardamos la posición Z inicial del jugador al arrancar el nivel
        lockedZPosition = transform.position.z;
    }

    private void Update()
    {
        HandleTimers();

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            ExecuteKnockbackMovement();
        }
        else
        {
            HandleHorizontalMovement();
            CheckJump();
        }

        ApplyGravity();
    }

    // --- EL FIX SÚPER IMPORTANTE ---
    private void LateUpdate()
    {
        // Si por culpa de un enemigo o un choque el jugador se movió en Z...
        if (transform.position.z != lockedZPosition)
        {
            // Lo regresamos a la fuerza a su carril correcto
            Vector3 correctedPosition = transform.position;
            correctedPosition.z = lockedZPosition;
            transform.position = correctedPosition;
        }
    }

    private void HandleTimers()
    {
        if (controller.isGrounded)
        {
            coyoteTimeCounter = coyoteTime; 
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; 
        }

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
        if (knockbackTimer > 0) return;

        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime; 
        }
    }

    private void HandleHorizontalMovement()
    {
        float targetDirection = 0;
        if (moveInput.x > 0.1f) targetDirection = 1f;
        else if (moveInput.x < -0.1f) targetDirection = -1f;

        float targetSpeed = targetDirection * speed;
        currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, targetSpeed, acceleration * Time.deltaTime);

        Vector3 moveDirection = new Vector3(currentHorizontalSpeed, 0, 0); 
        controller.Move(moveDirection * Time.deltaTime);
    }

   // Reemplaza este método en tu PlayerController.cs
    public void ApplyKnockback(Vector3 direction, float force, float upwardForce, float duration)
    {
        knockbackTimer = duration;
        
        knockbackDirection = new Vector3(direction.x, 0f, 0f).normalized;
        knockbackForce = force;
        currentHorizontalSpeed = 0f; // Cortamos en seco su velocidad horizontal

        verticalVelocity.y = upwardForce; 
    }

    private void ExecuteKnockbackMovement()
    {
        Vector3 knockbackMove = knockbackDirection * knockbackForce;
        controller.Move(knockbackMove * Time.deltaTime);
    }

    private void CheckJump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            verticalVelocity.y = jumpForce;
            jumpBufferCounter = 0; 
            coyoteTimeCounter = 0; 
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
}