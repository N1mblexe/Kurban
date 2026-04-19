using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public class KnifeShop : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class Data
    {
        public WeaponStats stats;
        public GameObject shopPrefab;    
        public int price;
    }

    [SerializeField] private List<Data> weapons; 
    [SerializeField] private int currentWeapon = 0;

    [Header("Görsel Animasyon Ayarları")]
    [SerializeField] private float rotateSpeed = 45f;
    [SerializeField] private float bobHeight = 0.2f;  
    [SerializeField] private float bobDuration = 1.5f; 
    [SerializeField] private bool rotateToZAxis = false;
    [SerializeField] private GameObject holder;
    [SerializeField] private TextMeshProUGUI price_text;

    private Vector3 originalHolderScale;
    private Vector3 originalTextScale;

    private void Start()
    {
        if (price_text != null)
        {
            originalTextScale = price_text.gameObject.transform.localScale;
            
            // Oyun başında doğru fiyatı ekrana yazdır (Eksikti, eklendi)
            if (weapons.Count > 0)
                price_text.text = weapons[currentWeapon].price.ToString();
        }

        if (holder != null)
        {
            originalHolderScale = holder.transform.localScale;
            InitializeShopVisuals();
            StartIdleAnimations();
        }
        
        // Start'ın sonundaki manuel index 0 atama döngüsünü sildim 
        // çünkü InitializeShopVisuals zaten bu işi dinamik olarak yapıyor.
    }

    private void InitializeShopVisuals()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].shopPrefab != null)
            {
                weapons[i].shopPrefab.SetActive(i == currentWeapon);
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

    public void Interact()
    {
        if (currentWeapon >= weapons.Count)
        {
            Debug.Log("Dükkandaki tüm bıçaklar tükendi!");
            return;
        }

        if (EconomyManager.Instance.TrySpendGold(weapons[currentWeapon].price))
        {
            KnifeController.Instance.EquipKnife(weapons[currentWeapon].stats);
            EconomyManager.Instance.currentKnifeMultiplier = weapons[currentWeapon].stats.economyMultiplier;

            AnimationSequence();
        }
        else
        {
            Debug.Log("Yeterli altının yok! Gerekli: " + weapons[currentWeapon].price);
            EconomyManager.Instance.ShakeGoldUI(); 
        }
    }

    private void AnimationSequence()
    {
        // Yazı sıfıra küçüldüğünde kapatıyoruz
        price_text.gameObject.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
        {
           price_text.gameObject.SetActive(false); 
        });

        holder.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => 
        {
            weapons[currentWeapon].shopPrefab.SetActive(false);

            currentWeapon++;

            // Dükkan boşaldıysa
            if (currentWeapon >= weapons.Count)
            {
                Debug.Log("Son bıçak satın alındı. Dükkan boş.");
                holder.SetActive(false);
                return; // price_text zaten yukarıda SetActive(false) olmuştu, kapalı kalacak.
            }

            // Yeni silahı aç
            weapons[currentWeapon].shopPrefab.SetActive(true);
            holder.transform.DOScale(originalHolderScale, 0.3f).SetEase(Ease.OutBack);
            
            // Fiyat yazısını güncelle, TEKRAR AKTİF ET ve büyüt (Hata buradaydı)
            price_text.text = weapons[currentWeapon].price.ToString();
            price_text.gameObject.SetActive(true); 
            price_text.gameObject.transform.DOScale(originalTextScale, 0.3f).SetEase(Ease.OutBack);
        });
    }
}