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
    
    // Bunu Public yaptık ki Kamera Efektleri karakterin gerçekten yerde olup olmadığını net bilsin
    public bool IsGrounded { get; private set; } 

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputManager = GetComponent<InputManager>();
    }

    private void Update()
    {
        HandleMovement();
        HandleGravityAndJump();
    }

    private void HandleMovement()
    {
        float currentSpeed = inputManager.SprintInput ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * inputManager.MoveInput.x + transform.forward * inputManager.MoveInput.y;
        controller.Move(move.normalized * currentSpeed * Time.deltaTime);
    }

    private void HandleGravityAndJump()
    {
        IsGrounded = controller.isGrounded;

        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        // Eğer zıplama isteği gelmişse
        if (inputManager.JumpInput)
        {
            // Ve karakter yerdeyse zıpla
            if (IsGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            
            // Zıplama isteğini tüket (Havada basıldıysa bile sıfırla ki yere inince kendi kendine zıplamasın)
            inputManager.JumpInput = false; 
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}