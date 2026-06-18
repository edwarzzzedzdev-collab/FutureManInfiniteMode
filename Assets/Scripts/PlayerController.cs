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
    private Vector3 externalVelocity;

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

    // ... dentro de PlayerController.cs

    private Vector3 moveDirection; // 1. Declarar esta variable aquí arriba, fuera de los métodos

    private void Update()
    {
        HandleTimers();

        // 2. Definir movimiento base
        moveDirection = new Vector3(currentHorizontalSpeed, 0, 0);

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

        // 3. Aplicar el impulso del muelle (externalVelocity)
        if (!controller.isGrounded)
        {
            externalVelocity.y += Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            externalVelocity.y = -0.5f; 
        }

        // 4. Moverse usando la suma de todo
        Vector3 finalMovement = moveDirection + new Vector3(0, verticalVelocity.y, 0) + externalVelocity;
        controller.Move(finalMovement * Time.deltaTime);
    }

public void ApplySpringJump(float force)
{
    // 1. SIMULAMOS EL SALTO (Lo que hace OnJump internamente)
    // Forzamos a que el buffer se active como si hubiera presionado el botón en este instante
    jumpBufferCounter = jumpBufferTime; 
    
    // Forzamos también el Coyote Time para asegurarnos de que el método CheckJump() 
    // valide el salto aunque el personaje esté flotando en el aire.
    coyoteTimeCounter = coyoteTime; 

    // 2. EJECUTAMOS EL SALTO DE INMEDIATO
    // Llamamos a tu método para que aplique la 'jumpForce' base
    CheckJump();

    // 3. LE SUMAMOS LA FUERZA DEL MUELLE
    // Ahora que verticalVelocity.y ya tiene la fuerza del salto normal, le acumulamos el impulso extra
    verticalVelocity.y += force;
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

        jumpBufferCounter = 0f; 
        coyoteTimeCounter = 0f; 
        
    }
}

   private void ApplyGravity()
{
    // ¡EL FIX! Solo aplicamos el pegamento si el jugador REALMENTE está cayendo o quieto.
    // Si verticalVelocity.y es mayor a 0, significa que acaba de saltar, así que NO lo frenamos.
    if (controller.isGrounded && verticalVelocity.y < 0)
    {
        verticalVelocity.y = -2f;
    }
    else
    {
        // Aplicamos gravedad normal solo si no está rígidamente pegado al suelo
        verticalVelocity.y += gravity * Time.deltaTime;
    }

    // Tu movimiento vertical independiente se queda intacto
    controller.Move(verticalVelocity * Time.deltaTime);
}

   
}