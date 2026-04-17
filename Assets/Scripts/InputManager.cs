using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    
    public bool JumpInput { get; set; } 
    
    public bool SprintInput { get; private set; }
    public bool AttackInput { get;  set; }
    public bool InteractInput { get;  set; }

    private InputSystem_Actions playerInput;

    private void Awake()
    {
        playerInput = new InputSystem_Actions();

        playerInput.Player.Jump.performed += ctx => JumpInput = true;

        playerInput.Player.Attack.performed += ctx => AttackInput = true;
        playerInput.Player.Attack.canceled  += ctx => AttackInput = false;
    }

    private void OnEnable()  => playerInput.Enable();
    private void OnDisable() => playerInput.Disable();

    private void Update()
    {
        MoveInput     = playerInput.Player.Move.ReadValue<Vector2>();
        LookInput     = playerInput.Player.Look.ReadValue<Vector2>();
        SprintInput   = playerInput.Player.Sprint.IsPressed();
        InteractInput = playerInput.Player.Interact.IsPressed();
    }
}