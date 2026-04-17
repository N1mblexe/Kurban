using UnityEngine;
using DG.Tweening;

public class KnifeShop : MonoBehaviour, IInteractable
{
    [Header("Market Bilgileri")]
    public string knifeName = "Çelik Şef Bıçağı";
    public float price = 1500f;

    [Header("Verilecek İstatistikler")]
    public WeaponStats upgradedStats;

    [Header("Görsel Animasyon Ayarları")]
    [SerializeField] private float rotateSpeed = 45f;
    [SerializeField] private float bobHeight = 0.2f;  
    [SerializeField] private float bobDuration = 1.5f; 

    private bool isBought = false;

    private void Start()
    {
        StartIdleAnimations();
    }

    private void StartIdleAnimations()
    {
        transform.DORotate(new Vector3(0, 360, 0), 360f / rotateSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);

        transform.DOMoveY(transform.position.y + bobHeight, bobDuration)
            .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
    }

    public void Interact()
    {
        if (isBought)
        {
            Debug.Log("Bu bıçağı zaten satın aldın!");
            return;
        }

        if (EconomyManager.Instance.TrySpendGold(price))
        {
            KnifeController.Instance.EquipKnife(upgradedStats);
            EconomyManager.Instance.currentKnifeMultiplier = upgradedStats.economyMultiplier;

            isBought = true;
            
            transform.DOKill();

            PlayPurchaseEffect();
        }
        else
        {
            Debug.Log("Yeterli altının yok! Gerekli: " + price);
            EconomyManager.Instance.ShakeGoldUI(); 
        }
    }

    void PlayPurchaseEffect()
    {
        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f).OnComplete(() => 
        {
            gameObject.SetActive(false); 
        });
    }
}