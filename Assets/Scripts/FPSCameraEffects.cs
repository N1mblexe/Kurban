using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    
    private MovementManager movement;
    private InputManager inputManager;
    private FpsLook fpsLook; // Kameranın tam X rotasyonunu almak için (To get perfect X rotation)

    [Header("Head Bobbing")]
    [SerializeField] private float bobFrequency = 14f;
    [SerializeField] private float bobAmplitude = 0.05f;
    private Vector3 cameraDefaultLocalPos;
    private float bobTimer = 0f;
    private float bobIntensity = 0f;
    private float groundedBuffer = 0f;

    [Header("Dynamic FOV")]
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float sprintFOV = 75f;
    [SerializeField] private float fovSmoothness = 10f;

    [Header("Ayak Sesleri")]
    [SerializeField] private float stepThreshold = -0.5f; 
    private bool stepTaken = false;

    [Header("Leaning")]
    [SerializeField] private float leanAngle = 4f;
    [SerializeField] private float leanSmoothness = 8f;
    private float currentLean = 0f;

    private void Awake()
    {
        movement = GetComponent<MovementManager>();
        inputManager = GetComponent<InputManager>();
        fpsLook = GetComponent<FpsLook>(); 
    }

    private void Start()
    {
        if (playerCamera != null)
        {
            cameraDefaultLocalPos = playerCamera.transform.localPosition;
            playerCamera.fieldOfView = baseFOV;
        }
    }

    private void LateUpdate()
    {
        if (playerCamera == null) return;

        HandleHeadBob();
        HandleDynamicFOV();
        HandleLean();
    }

    private void HandleHeadBob()
    {
        if (movement.IsGrounded)
        {
            groundedBuffer = 0.1f;
        }
        else
        {
            groundedBuffer -= Time.deltaTime;
        }

        bool isMoving = (groundedBuffer > 0f) && inputManager.MoveInput.magnitude > 0.1f;
        float targetIntensity = isMoving ? 1f : 0f;

        // Bob şiddetini yumuşakça artır/azalt (Smoothly lerp bob intensity)
        bobIntensity = Mathf.Lerp(bobIntensity, targetIntensity, Time.deltaTime * 10f);

        if (isMoving)
        {
            float speedMultiplier = inputManager.SprintInput ? 1.5f : 1f;
            bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;

            // Dalga değerini Sin ile hesapla ki sıfırdan başlasın (Use Sin so it starts at 0 naturally)
            float waveValue = Mathf.Sin(bobTimer);
            
            if (waveValue < stepThreshold && !stepTaken)
            {
                PlayFootstepSound();
                stepTaken = true;
            }
            else if (waveValue > 0)
            {
                stepTaken = false;
            }
        }
        else if (bobIntensity < 0.01f)
        {
            // Kamera tamamen durduğunda zamanlayıcıyı sıfırla ki bir sonraki hareket pürüzsüz başlasın
            // (Reset timer only when fully stopped to prevent snapping)
            bobTimer = 0f;
        }

        // Y ekseni için Cos yerine Sin kullandık (Swapped Cos for Sin on Y axis for stability)
        float bobY = Mathf.Sin(bobTimer) * bobAmplitude * bobIntensity;
        float bobX = Mathf.Cos(bobTimer / 2f) * (bobAmplitude / 2f) * bobIntensity;

        playerCamera.transform.localPosition = new Vector3(
            cameraDefaultLocalPos.x + bobX,
            cameraDefaultLocalPos.y + bobY,
            cameraDefaultLocalPos.z
        );
    }

    private void HandleDynamicFOV()
    {
        float targetFOV = inputManager.SprintInput && inputManager.MoveInput.magnitude > 0.1f ? sprintFOV : baseFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovSmoothness);
    }

    private void HandleLean()
    {
        float targetLean = -inputManager.MoveInput.x * leanAngle;
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSmoothness);

        // Titremeyi önlemek için localEulerAngles yerine doğrudan FpsLook'tan rotasyonu alıyoruz
        // (Getting exact rotation directly from FpsLook to prevent Unity Euler jitter)
        float currentXRotation = fpsLook != null ? fpsLook.XRotation : playerCamera.transform.localEulerAngles.x;

        playerCamera.transform.localRotation = Quaternion.Euler(currentXRotation, 0f, currentLean);
    }

    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0 || footstepAudioSource == null) return;

        int index = Random.Range(0, footstepSounds.Length);
        footstepAudioSource.PlayOneShot(footstepSounds[index]);
    }
}