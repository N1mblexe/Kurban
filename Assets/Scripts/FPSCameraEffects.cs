using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    // Hangi sesin çalınacağını belirlemek için bir kategori oluşturduk
    public enum SoundActionType { Walk, Sprint, Jump, Land }

    [System.Serializable]
    public struct SurfaceAudioSet
    {
        public string surfaceTag;
        public AudioClip[] footstepSounds;
        public AudioClip[] sprintSounds;
        public AudioClip[] jumpSounds; // Zemine özel zıplama sesleri
        public AudioClip[] landSounds; // Zemine özel yere inme sesleri
    }

    public static CameraEffects Instance;

    [Header("Referanslar")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioSource footstepAudioSource;
    private MovementManager movement;
    private InputManager inputManager;
    private FpsLook fpsLook;

    [Header("Zemin Ses Havuzları")]
    public SurfaceAudioSet[] surfaceAudioSets;

    [Header("Varsayılan Sesler (Tanımsız Zeminler İçin)")]
    public AudioClip[] defaultFootstepSounds;
    public AudioClip[] defaultSprintSounds;
    public AudioClip[] defaultJumpSounds;
    public AudioClip[] defaultLandSounds;

    [Header("Head Bobbing")]
    [SerializeField] private float bobFrequency = 14f;
    [SerializeField] private float bobAmplitude = 0.05f;
    private Vector3 cameraDefaultLocalPos;
    private float bobTimer = 0f;
    private float bobIntensity = 0f;
    private float groundedBuffer = 0f;
    private bool wasGrounded;

    [Header("Dynamic FOV")]
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float sprintFOV = 75f;
    [SerializeField] private float fovSmoothness = 10f;

    [Header("Ayak Sesleri Ayarları")]
    [SerializeField] private float stepThreshold = -0.5f;
    [SerializeField] private float raycastDistance = 2.5f; // Zıplarken zemini kaçırmamak için biraz artırdık
    [SerializeField] private LayerMask groundLayer;
    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    private bool stepTaken = false;

    [Header("Leaning")]
    [SerializeField] private float leanAngle = 4f;
    [SerializeField] private float leanSmoothness = 8f;
    private float currentLean = 0f;

    [Header("Camera Shake")]
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;
    private Vector3 shakePosOffset = Vector3.zero;
    private Vector3 shakeRotOffset = Vector3.zero;

    private void Awake()
    {
        movement = GetComponent<MovementManager>();
        inputManager = GetComponent<InputManager>();
        fpsLook = GetComponent<FpsLook>();
        Instance = this;
    }

    private void Start()
    {
        if (playerCamera != null)
        {
            cameraDefaultLocalPos = playerCamera.transform.localPosition;
            playerCamera.fieldOfView = baseFOV;
        }
        wasGrounded = movement.IsGrounded;
    }

    private void LateUpdate()
    {
        if (playerCamera == null) return;

        HandleJumpAndLandSounds();
        HandleShake();
        HandleHeadBob();
        HandleDynamicFOV();
        HandleLean();
    }

    private void HandleJumpAndLandSounds()
    {
        // Yere inme kontrolü
        if (movement.IsGrounded && !wasGrounded)
        {
            PlaySurfaceSound(SoundActionType.Land);
        }
        // Zıplama/Yerden kesilme kontrolü
        else if (!movement.IsGrounded && wasGrounded)
        {
            PlaySurfaceSound(SoundActionType.Jump);
        }

        wasGrounded = movement.IsGrounded;
    }

    public void TriggerShake(float magnitude, float duration)
    {
        shakeMagnitude = magnitude;
        shakeDuration = duration;
        shakeTimer = duration;
    }

    private void HandleShake()
    {
        if (shakeTimer > 0)
        {
            float fadeOut = shakeTimer / shakeDuration;
            float currentMag = shakeMagnitude * fadeOut;

            shakePosOffset = UnityEngine.Random.insideUnitSphere * currentMag;
            shakeRotOffset = UnityEngine.Random.insideUnitSphere * (currentMag * 10f);

            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakePosOffset = Vector3.zero;
            shakeRotOffset = Vector3.zero;
        }
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

        bobIntensity = Mathf.Lerp(bobIntensity, targetIntensity, Time.deltaTime * 10f);

        if (isMoving)
        {
            bool isSprinting = inputManager.SprintInput;
            float speedMultiplier = isSprinting ? 1.5f : 1f;
            bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;

            float waveValue = Mathf.Sin(bobTimer);

            if (waveValue < stepThreshold && !stepTaken)
            {
                SoundActionType currentAction = isSprinting ? SoundActionType.Sprint : SoundActionType.Walk;
                PlaySurfaceSound(currentAction);
                stepTaken = true;
            }
            else if (waveValue > 0)
            {
                stepTaken = false;
            }
        }
        else if (bobIntensity < 0.01f)
        {
            bobTimer = 0f;
        }

        float bobY = Mathf.Sin(bobTimer) * bobAmplitude * bobIntensity;
        float bobX = Mathf.Cos(bobTimer / 2f) * (bobAmplitude / 2f) * bobIntensity;

        playerCamera.transform.localPosition = new Vector3(
            cameraDefaultLocalPos.x + bobX,
            cameraDefaultLocalPos.y + bobY,
            cameraDefaultLocalPos.z
        ) + shakePosOffset;
    }

    // Diğer handle metodları...
    private void HandleDynamicFOV()
    {
        float targetFOV = inputManager.SprintInput && inputManager.MoveInput.magnitude > 0.1f ? sprintFOV : baseFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovSmoothness);
    }

    private void HandleLean()
    {
        float targetLean = -inputManager.MoveInput.x * leanAngle;
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSmoothness);

        float currentXRotation = fpsLook != null ? fpsLook.XRotation : playerCamera.transform.localEulerAngles.x;

        playerCamera.transform.localRotation = Quaternion.Euler(
            currentXRotation + shakeRotOffset.x,
            shakeRotOffset.y,
            currentLean + shakeRotOffset.z
        );
    }

    // Merkezi Ses Çalma Metodu
    private void PlaySurfaceSound(SoundActionType actionType)
    {
        if (footstepAudioSource == null) return;

        AudioClip clipToPlay = GetClipFromSurface(actionType);

        if (clipToPlay != null)
        {
            footstepAudioSource.PlayOneShot(clipToPlay, footstepVolume);
        }
    }

    // Zemine ve Yapılan Harekete Göre Sesi Seçen Metot
    private AudioClip GetClipFromSurface(SoundActionType actionType)
    {
        if (Physics.Raycast(transform.position, Vector3.down * 2, out RaycastHit hit, raycastDistance, groundLayer))
        {
            string hitTag = hit.collider.tag;

            foreach (SurfaceAudioSet set in surfaceAudioSets)
            {
                Debug.Log(set.surfaceTag + " : " + hitTag);

                if (set.surfaceTag == hitTag)
                {
                    AudioClip[] selectedClips = GetClipsFromSet(set, actionType);

                    if (selectedClips != null && selectedClips.Length > 0)
                    {
                        return selectedClips[UnityEngine.Random.Range(0, selectedClips.Length)];
                    }
                }
            }
        }

        // Eğer zemin tag'i bulunamazsa varsayılan ses havuzlarından seç
        AudioClip[] defaultClips = GetDefaultClips(actionType);
        if (defaultClips != null && defaultClips.Length > 0)
        {
            return defaultClips[UnityEngine.Random.Range(0, defaultClips.Length)];
        }

        return null;
    }

    // Seçilen zemin paketinden doğru diziyi çeker
    private AudioClip[] GetClipsFromSet(SurfaceAudioSet set, SoundActionType actionType)
    {
        switch (actionType)
        {
            case SoundActionType.Walk: return set.footstepSounds;
            case SoundActionType.Sprint: return (set.sprintSounds != null && set.sprintSounds.Length > 0) ? set.sprintSounds : set.footstepSounds;
            case SoundActionType.Jump: return set.jumpSounds;
            case SoundActionType.Land: return set.landSounds;
            default: return null;
        }
    }

    // Tanımsız zeminler için varsayılan dizileri çeker
    private AudioClip[] GetDefaultClips(SoundActionType actionType)
    {
        switch (actionType)
        {
            case SoundActionType.Walk: return defaultFootstepSounds;
            case SoundActionType.Sprint: return (defaultSprintSounds != null && defaultSprintSounds.Length > 0) ? defaultSprintSounds : defaultFootstepSounds;
            case SoundActionType.Jump: return defaultJumpSounds;
            case SoundActionType.Land: return defaultLandSounds;
            default: return null;
        }
    }
}