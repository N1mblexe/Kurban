using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class WeaponStats
{
    public float damage = 1f;
    public float attackRange = 4f;
    public float attackSpeed = .7f;
    
    public float economyMultiplier = 1.0f; 
    public float minSliceVolume = 0.5f;    
    public int visualIndex = 0;
}

public class KnifeController : MonoBehaviour
{
    public static KnifeController Instance { get; private set; }

    [Header("Bıçak İstatistikleri")]
    public WeaponStats currentStats;

    [Header("Referanslar")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject[] knifeVisualModels;
    [SerializeField] private Transform knifeModel;

    [Header("Animasyon Ayarları (Dikey Kesme)")]
    [SerializeField] private Vector3 attackPositionOffset = new Vector3(1f, 0f, 0f); 
    [SerializeField] private Vector3 attackRotationOffset = new Vector3(55f, 0f, 180f); 
    [SerializeField] private float animationSpeed = 20f;

    private float nextAttackTime = 0f;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private bool isAttacking = false;
    private float attackTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        //Default knife
        if(currentStats.visualIndex == 0)
        {
            currentStats.damage = 1;
            currentStats.attackRange = 4; 
            currentStats.attackSpeed = 1.1f;
            currentStats.minSliceVolume = .1f;
        }
    }

    private void Start()
    {
        if (knifeModel != null)
        {
            defaultPosition = knifeModel.localPosition;
            defaultRotation = knifeModel.localRotation;
        }
    }

    private void Update()
    {
        HandleAttackInput();
        HandleProceduralAnimation();

        EquipKnife(currentStats);
    }

    public void EquipKnife(WeaponStats newStats)
    {
        currentStats = newStats;

        for (int i = 0; i < knifeVisualModels.Length; i++)
        {
            if (knifeVisualModels[i] != null)
            {
                knifeVisualModels[i].SetActive(i == currentStats.visualIndex);
            }
        }
    }

    private void HandleAttackInput()
    {
        if (inputManager.AttackInput && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + (1f / currentStats.attackSpeed);
        }
        
        inputManager.AttackInput = false;
    }

    private void PerformAttack()
{
    isAttacking = true;
    attackTimer = 0f;

    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, currentStats.attackRange))
    {
        if (hit.collider.TryGetComponent(out Health health))
        {
            health.TakeDamage(currentStats.damage, hit.point, inputManager.LookInput);
            Debug.Log($"Hasar Verildi! Kalan Can: {health.MaxHealth}");
        }
    }
}
    private void HandleProceduralAnimation()
    {
        if (knifeModel == null) return;

        if (isAttacking)
        {
            attackTimer += Time.deltaTime * animationSpeed;
            
            knifeModel.localPosition = Vector3.Lerp(knifeModel.localPosition, defaultPosition + attackPositionOffset, Time.deltaTime * animationSpeed);
            knifeModel.localRotation = Quaternion.Lerp(knifeModel.localRotation, defaultRotation * Quaternion.Euler(attackRotationOffset), Time.deltaTime * animationSpeed);

            if (attackTimer > 1f)
            {
                isAttacking = false;
            }
        }
        else
        {
            knifeModel.localPosition = Vector3.Lerp(knifeModel.localPosition, defaultPosition, Time.deltaTime * (animationSpeed / 2.5f));
            knifeModel.localRotation = Quaternion.Lerp(knifeModel.localRotation, defaultRotation, Time.deltaTime * (animationSpeed / 2.5f));
        }
    }
}