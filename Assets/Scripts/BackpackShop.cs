using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class BackpackShop : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class BackpackData
    {
        public string backpackName;
        public GameObject shopPrefab;    
        public float price;
        
        [Header("Çanta Kapasite Değerleri")]
        public float newCapacity;
        public float newMaxVolumePerItem;
    }

    [Header("Çanta Listesi")]
    [SerializeField] private List<BackpackData> backpacks; 
    [SerializeField] private int currentBackpack = 0;

    [Header("Görsel Animasyon Ayarları")]
    [SerializeField] private float rotateSpeed = 45f;
    [SerializeField] private float bobHeight = 0.2f;  
    [SerializeField] private float bobDuration = 1.5f; 
    [SerializeField] private bool rotateToZAxis = false;
    
    [Header("Referanslar")]
    [SerializeField] private GameObject holder;
    [SerializeField] private TextMeshProUGUI price_text;

    private Vector3 originalHolderScale;
    private Vector3 originalTextScale;

    private void Start()
    {
        if (price_text != null)
        {
            originalTextScale = price_text.gameObject.transform.localScale;
            
            if (backpacks.Count > 0)
                price_text.text = backpacks[currentBackpack].price.ToString();
        }

        if (holder != null)
        {
            originalHolderScale = holder.transform.localScale;
            InitializeShopVisuals();
            StartIdleAnimations();
        }
    }

    private void InitializeShopVisuals()
    {
        for (int i = 0; i < backpacks.Count; i++)
        {
            if (backpacks[i].shopPrefab != null)
            {
                backpacks[i].shopPrefab.SetActive(i == currentBackpack);
            }
        }
    }

    private void StartIdleAnimations()
    {
        Vector3 rotation = rotateToZAxis ? new Vector3(0, 0, 360) : new Vector3(0, 360, 0);

        holder.transform.DORotate(rotation, 360f / rotateSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);

        holder.transform.DOMoveY(holder.transform.position.y + bobHeight, bobDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    static float cooldown = 3f;
    static bool onCooldown = false;

    IEnumerator CooldownThread()
    {
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }


    public void Interact()
    {
        if(onCooldown) 
            return;

        // Tüm çantalar satıldıysa
        if (currentBackpack >= backpacks.Count)
        {
            Debug.Log("Dükkandaki tüm çantalar tükendi!");
            return;
        }

        BackpackData data = backpacks[currentBackpack];

        if (EconomyManager.Instance.TrySpendGold(data.price))
        {
            onCooldown = true;
            StartCoroutine(CooldownThread());
            
            Backpack upgradedBackpack = new Backpack(data.newCapacity, 0, data.newMaxVolumePerItem);
            InventoryManager.Instance.UpdateBackpack(upgradedBackpack);

            Debug.Log($"{data.backpackName} satın alındı! Yeni Kapasite: {data.newCapacity} kg");

            float currentWeight = InventoryManager.Instance.FreeInventory(); 

            AnimationSequence();
        }
        else
        {
            Debug.Log("Yeterli altının yok! Gerekli: " + data.price);
            EconomyManager.Instance.ShakeGoldUI();
        }
    }

    private void AnimationSequence()
    {
        if (price_text != null)
        {
            price_text.gameObject.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
            {
                price_text.gameObject.SetActive(false); 
            });
        }

        holder.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => 
        {
            backpacks[currentBackpack].shopPrefab.SetActive(false);

            currentBackpack++;

            if (currentBackpack >= backpacks.Count)
            {
                Debug.Log("Son çanta satın alındı. Çanta dükkanı boş.");
                holder.SetActive(false);
                return;
            }

            backpacks[currentBackpack].shopPrefab.SetActive(true);
            holder.transform.DOScale(originalHolderScale, 0.3f).SetEase(Ease.OutBack);
            
            if (price_text != null)
            {
                price_text.text = backpacks[currentBackpack].price.ToString();
                price_text.gameObject.SetActive(true); 
                price_text.gameObject.transform.DOScale(originalTextScale, 0.3f).SetEase(Ease.OutBack);
            }
        });
    }
}