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