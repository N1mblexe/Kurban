using UnityEngine;
using DG.Tweening;

public class IslandShop : MonoBehaviour, IInteractable
{
    [Header("Ada Bilgileri")]
    public string islandName = "Sisli Orman";
    public float price = 5000f;

    [Header("Ekonomi Etkisi")]
    public float newIslandBasePrice = 18f; 

    [Header("Dünya Etkileşimi (İsteğe Bağlı)")]
    [Tooltip("Satın alınca yok olacak obje (Örn: Yolu kapatan kapı veya duvar)")]
    public GameObject barrierToDestroy; 
    
    [Tooltip("Satın alınca aktif olacak obje (Örn: Adaya giden tekne veya portal)")]
    public GameObject teleporterToActivate; 

    private bool isBought = false;

    [Header("Görsel Ayarlar")]
    [SerializeField] private Renderer shopRenderer;
    [SerializeField] private Color affordableColor = Color.green;
    [SerializeField] private Color expensiveColor = Color.red;

    private Material shopMaterial;

    private void Start()
    {
        if (shopRenderer != null)
        {
            shopMaterial = shopRenderer.material;
        }
    }

    private void Update()
    {
        if (isBought) return;

        UpdateVisualStatus();
    }

    private void UpdateVisualStatus()
    {
        if (shopMaterial == null) return;

        bool canAfford = EconomyManager.Instance.currentGold >= price;
        
        Color targetColor = canAfford ? affordableColor : expensiveColor;
        shopMaterial.DOColor(targetColor, 0.5f);
    }
    public void Interact()
    {
        if (isBought)
        {
            Debug.Log($"{islandName} zaten açık! Yola koyulabilirsin.");
            return;
        }

        if (EconomyManager.Instance.TrySpendGold(price))
        {
            EconomyManager.Instance.currentIslandBasePrice = newIslandBasePrice;

            isBought = true;
            Debug.Log($"{islandName} kilidi açıldı! Yeni kg fiyatı: {newIslandBasePrice} Gold");

            PlayUnlockEffect();
        }
        else
        {
            Debug.Log($"Yeterli altının yok! Gerekli: " + price);
            EconomyManager.Instance.ShakeGoldUI(); 
        }
    }

    void PlayUnlockEffect()
    {
        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f).OnComplete(() => 
        {
            if (barrierToDestroy != null) barrierToDestroy.SetActive(false);
            if (teleporterToActivate != null) teleporterToActivate.SetActive(true);

            gameObject.SetActive(false); 
        });
    }
}