using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(InputManager))]
public class MovementManager : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -25f;

    [Header("Etkileşim & Fizik (Knockback)")]
    [SerializeField] private float mass = 3f;           
    [SerializeField] private float impactDamping = 5f;  

    private CharacterController controller;
    private InputManager inputManager;
    private Vector3 velocity;         
    private Vector3 impactVelocity;   
    
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

        // Eğer karakter güçlü bir şekilde savruluyorsa, havadaki oyuncu kontrolünü kısıtlayabilirsiniz (Opsiyonel)
        // if (impactVelocity.magnitude > 2f) horizontalMove *= 0.2f; 

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

        if (impactVelocity.magnitude > 0.2f)
        {
            finalMovement += impactVelocity;
        }
        
        impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, impactDamping * Time.deltaTime);

        controller.Move(finalMovement * Time.deltaTime);
    }

    public void AddImpact(Vector3 direction, float force)
    {
        direction.Normalize();
        
        if (direction.y < 0) 
        {
            direction.y = -direction.y;
        } 
        
        impactVelocity += direction * (force / mass);
    }
}