using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class IslandShop : MonoBehaviour, IInteractable
{
    [Header("Ada Bilgileri")]
    public string islandName = "Sisli Orman";
    public float price = 5000f;
    public bool isFree = false;
    public IslandType islandIndex;

    [Header("Ekonomi Etkisi")]
    public float newIslandBasePrice = 18f;

    [Header("Dünya Etkileşimi")]
    [Tooltip("Satın alınca yok olacak obje (Örn: Yolu kapatan kapı veya duvar)")]
    public GameObject barrierToDestroy;

    [Tooltip("Açılma animasyonu bittikten EN SONRA aktif olacak obje (Örn: Teleporter)")]
    public GameObject teleporterToActivate;

    private bool isBought = false;

    [Header("Görsel Ayarlar")]
    [SerializeField] private Renderer shopRenderer;
    [SerializeField] private Color affordableColor = Color.green;
    [SerializeField] private Color expensiveColor = Color.red;
    private bool? wasAffordable = null;

    private Material shopMaterial;

    [Header("Harita Animasyonu")]
    [Tooltip("Altındaki objelerin sırayla açılacağı ana obje")]
    [SerializeField] private GameObject parent;
    [SerializeField] private float animDuration = 0.5f;
    [SerializeField] private float delayBetween = 0.1f;

    private void Start()
    {
        price = (isFree) ? 0 : EconomyManager.Instance.islandPrices[islandIndex.GetHashCode()];
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

        if (wasAffordable == null || canAfford != wasAffordable)
        {
            wasAffordable = canAfford;

            Color targetColor = canAfford ? affordableColor : expensiveColor;
            shopMaterial.DOColor(targetColor, 0.5f);
        }
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

            gameObject.SetActive(false);

            if (parent != null)
            {
                parent.SetActive(true);
                MapUnlockEffect();
            }
            else
            {
                if (teleporterToActivate != null) teleporterToActivate.SetActive(true);
            }
        });
    }

    public void MapUnlockEffect()
    {
        List<Transform> children = parent.transform.Cast<Transform>()
                                         .OrderBy(child => child.position.y)
                                         .ToList();

        if (children.Count == 0)
        {
            if (teleporterToActivate != null) teleporterToActivate.SetActive(true);
            return;
        }

        for (int i = 0; i < children.Count; i++)
        {
            Transform child = children[i];
            Vector3 originalScale = child.localScale;
            child.localScale = new Vector3(0.001f, 0.001f, 0.001f);

            child.gameObject.SetActive(true);

            // Animasyonu değişkene ata
            var tween = child.DOScale(originalScale, animDuration)
                             .SetDelay(i * delayBetween)
                             .SetEase(Ease.OutBack);

            if (i == children.Count - 1)
            {
                tween.OnComplete(() =>
                {
                    if (teleporterToActivate != null)
                    {
                        teleporterToActivate.SetActive(true);
                    }
                });
            }
        }
    }
}