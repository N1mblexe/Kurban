using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Oyuncunun dünyadaki objelerle etkileşimini (Raycast ile) yönetir.
/// </summary>
public class InteractManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private TextMeshProUGUI infoTextUI;

    private InputManager inputManager;
    private GameObject currentHoveredObject;
    private IInteractable currentInteractable; // Performans için arayüzü önbellekte tutuyoruz
    private Outline currentOutline;

    void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main.gameObject;
        }
        
        inputManager = GetComponent<InputManager>();

        if (infoTextUI != null)
        {
            infoTextUI.text = "";
        }
    }

    void Update()
    {
        // 1. Baktığımız objeyi bul
        GameObject target = GetHoveringObject(out IInteractable interactable);

        // 2. Eğer hedef değiştiyse veya boşa bakıyorsak eski efektleri temizle
        if (currentHoveredObject != null && currentHoveredObject != target)
        {
            ClearCurrentEffects();
        }

        // 3. Yeni bir hedefe bakmaya başladıysak efektleri aktif et
        if (target != null && target != currentHoveredObject)
        {
            currentHoveredObject = target;
            currentInteractable = interactable; // Arayüzü kaydet
            ActivateEffects(target);
        }

        // 4. Hedefe bakmaya devam ediyorsak OnHover'ı çağır (GetComponent kullanmadan!)
        if(currentHoveredObject != null && currentInteractable != null)
        {
            currentInteractable.OnHover();
        }

        // 5. Etkileşim tuşuna basıldıysa (örn: E tuşu)
        if (inputManager != null && inputManager.InteractInput)
        {
            HandleInteraction();
            inputManager.InteractInput = false;
        }
    }

    private void HandleInteraction()
    {
        if (currentHoveredObject == null) return;

        // Eğer kesilebilir (dilimlenebilir) bir objeyse envanter mantığını çalıştır
        if (currentHoveredObject.TryGetComponent(out SliceableObject sliceable))
        {
            float volume = sliceable.CurrentVolume;

            if (volume > PlayerInventory.Instance.GetMaxItemSize())
            {
                ShakeHUDVolume();
            }
            else
            {
                bool isPickedUp = PlayerInventory.Instance.TryPickupItem(currentHoveredObject, volume);

                if (isPickedUp)
                {
                    ClearCurrentEffects();
                }
                else
                {
                    InventoryUI.Instance.ShakeWeightUI();
                }
            }
        }
        else if (currentInteractable != null)
        {
            // Normal bir interactable obje ise (örn: Storage) Interact metodunu çağır
            currentInteractable.Interact();
        }
    }

    private void ActivateEffects(GameObject obj)
    {
        if (!obj.TryGetComponent(out currentOutline))
        {
            currentOutline = obj.AddComponent<Outline>();
            currentOutline.OutlineMode = Outline.Mode.OutlineAll;
            currentOutline.OutlineColor = Color.black;
            currentOutline.OutlineWidth = 8f;
        }
        currentOutline.enabled = true;

        if (infoTextUI != null && obj.TryGetComponent(out SliceableObject sliceable))
        {
            infoTextUI.text = $"Hacim: {sliceable.CurrentVolume:F2} m³";
        }
    }

    private void ClearCurrentEffects()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
        }

        if (infoTextUI != null)
        {
            infoTextUI.text = "";
        }

        currentHoveredObject = null;
        currentInteractable = null; // Arayüzü sıfırla
        currentOutline = null;
    }

    /// <summary>
    /// Raycast göndererek bakılan objeyi ve IInteractable arayüzünü döndürür.
    /// </summary>
    private GameObject GetHoveringObject(out IInteractable interactable)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        interactable = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 3))
        {
            if (hit.collider.TryGetComponent(out interactable))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    private void ShakeHUDVolume()
    {
        if (infoTextUI != null)
        {
            infoTextUI.rectTransform.DOKill(true);
            infoTextUI.DOKill(true);

            infoTextUI.rectTransform.DOShakePosition(0.4f, new Vector3(15f, 0, 0), 20);

            Sequence colorSeq = DOTween.Sequence();
            colorSeq.Append(infoTextUI.DOColor(Color.red, 0.1f)); 
            colorSeq.Append(infoTextUI.DOColor(Color.white, 0.3f));
        }
    }
}