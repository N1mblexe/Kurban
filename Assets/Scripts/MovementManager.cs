using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(InputManager))]
public class MovementManager : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -25f;

    private CharacterController controller;
    private InputManager inputManager;
    private Vector3 velocity;
    
    public bool IsGrounded { get; private set; } 

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputManager = GetComponent<InputManager>();
    }

    private void Update()
    {
        IsGrounded = controller.isGrounded;

        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        float currentSpeed = inputManager.SprintInput ? sprintSpeed : walkSpeed;
        Vector3 moveInput = transform.right * inputManager.MoveInput.x + transform.forward * inputManager.MoveInput.y;
        
        Vector3 horizontalMove = Vector3.ClampMagnitude(moveInput, 1f) * currentSpeed;

        if (inputManager.JumpInput)
        {
            if (IsGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            inputManager.JumpInput = false; 
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = horizontalMove + velocity;
        controller.Move(finalMovement * Time.deltaTime);
    }
}