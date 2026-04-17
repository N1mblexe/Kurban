using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class FpsLook : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 150f;

    private InputManager inputManager;
    private float xRotation = 0f;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = inputManager.LookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = inputManager.LookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}