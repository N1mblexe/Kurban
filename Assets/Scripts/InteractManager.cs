using UnityEngine;
using TMPro;
using DG.Tweening;
public class InteractManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;

    [SerializeField] private TextMeshProUGUI infoTextUI;

    private InputManager inputManager;
    private GameObject currentHoveredObject;
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
        GameObject target = GetHoveringInteractable();

        if (currentHoveredObject != null && currentHoveredObject != target)
        {
            ClearCurrentEffects();
        }

        if (target != null && target != currentHoveredObject)
        {
            currentHoveredObject = target;
            ActivateEffects(target);
        }

        if (inputManager != null && inputManager.InteractInput)
        {
            if (currentHoveredObject != null)
            {
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
                else
                {
                    currentHoveredObject.GetComponent<IInteractable>().Interact();
                }
            }
            inputManager.InteractInput = false;
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
        currentOutline = null;
    }

    GameObject GetHoveringInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
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