using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpForce = 8f;
    
    [Header("Estado Actual")]
    private Vector2 moveInput;
    private Vector3 verticalVelocity;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleHorizontalMovement();
        ApplyGravity();
    }

    // --- Inputs ---
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (controller.isGrounded)
        {
            verticalVelocity.y = jumpForce; 
        }
    }

    // --- Logica ---
    private void HandleHorizontalMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, 0); 
        controller.Move(moveDirection * speed * Time.deltaTime);
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