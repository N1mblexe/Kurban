using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Matematiksel Ekonomi Ayarları")]
    public float baseMeatPrice = 1f;           
    public float targetMinutesPerIsland = 15f; 
    public float playerEfficiency = 0.75f;    
    
    [Header("Üretim Hızı Verileri (Simülasyon İçin)")]
    public float playerMeatPerMin = 10f;       
    public float catMeatPerMin = 3f;         
    
    [Header("Geliştirme Taban Fiyatları ve Katsayıları")]
    public float knifeBasePrice = 100f;
    public float knifeGrowthRate = 1.4f;       
    
    public float packBasePrice = 50f;
    public float packGrowthRate = 1.2f;       
    
    public float catBasePrice = 100f;
    public float catGrowthRate = 2.5f;       

    [Header("Mevcut Çarpanlar (Kayıttan Yüklenecek)")]
    public float currentKnifeMultiplier = 1.0f;
    public float currentIslandMultiplier = 1.0f;

    [Header("Otomatik Oluşturulan Fiyat Listeleri")]
    public int maxLevelCount = 10;             
    public int maxIslandCount = 5;            
    
    public List<int> knifePrices;
    public List<int> packPrices;
    public List<int> islandPrices;

    [System.Serializable]
    public class SerializedList<T>
    {
        public List<T> values = new List<T>();
    }
    public List<SerializedList<int>> catPrices;

    [Header("Para Ayarları")]
    public float currentGold = 0;
    public TextMeshProUGUI goldTextUI;
    public Animator goldAnim;

    private void Awake()
    {
        Instance = this;
        UpdateGoldUI();
        DOTween.SetTweensCapacity(12000, 700);
    }

    void Update()
    {
        //remove this when building
        if(Input.GetKeyDown(KeyCode.K))
        {
            AddGold(50000);
        }
    }

    public void SellMeat(int meatAmount)
    {
        float meatValue = baseMeatPrice * currentIslandMultiplier * currentKnifeMultiplier;
        float totalEarnings = meatAmount * meatValue;
        
        AddGold(totalEarnings);
    }

    public void AddGold(float amount)
    {
        currentGold += amount;
        UpdateGoldUI();
        if (goldAnim != null) goldAnim.SetTrigger("a");
    }

    void UpdateGoldUI()
    {
        if (goldTextUI != null) goldTextUI.text = currentGold.ToString("F0") + " G";
    }

    public bool TrySpendGold(float amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldUI();
            return true;
        }
        return false;
    }

    public void ShakeGoldUI()
    {
        if (goldTextUI != null)
        {
            goldTextUI.rectTransform.DOKill(true);
            goldTextUI.DOKill(true);

            goldTextUI.rectTransform.DOShakePosition(0.4f, new Vector3(15f, 0, 0), 20);
            
            Sequence colorSeq = DOTween.Sequence();
            colorSeq.Append(goldTextUI.DOColor(Color.red, 0.1f)); 
            colorSeq.Append(goldTextUI.DOColor(Color.white, 0.3f));
        }
    }

    [ContextMenu("Fiyat Listelerini Otomatik Hesapla")]
    public void GenerateEconomyLists()
    {
        knifePrices.Clear();
        packPrices.Clear();
        islandPrices.Clear();
        catPrices.Clear();

        for (int i = 0; i < maxLevelCount; i++)
        {
            int kPrice = Mathf.RoundToInt(knifeBasePrice * Mathf.Pow(knifeGrowthRate, i));
            int pPrice = Mathf.RoundToInt(packBasePrice * Mathf.Pow(packGrowthRate, i));
            
            knifePrices.Add(kPrice);
            packPrices.Add(pPrice);
        }

        float simIslandMultiplier = 1f;
        float simKnifeMultiplier = 1.5f; 

        for (int i = 0; i < maxIslandCount; i++)
        {
            SerializedList<int> islandCatPrices = new SerializedList<int>();
            float islandCatBaseValue = catBasePrice * simIslandMultiplier; 

            for (int c = 0; c < 5; c++) 
            {
                int cPrice = Mathf.RoundToInt(islandCatBaseValue * Mathf.Pow(catGrowthRate, c));
                islandCatPrices.values.Add(cPrice);
            }
            catPrices.Add(islandCatPrices);

            float maxMeatPerMin = playerMeatPerMin + (5 * catMeatPerMin);
            float currentG_dk = maxMeatPerMin * (baseMeatPrice * simIslandMultiplier * simKnifeMultiplier);
            
            float nextIslandCost = targetMinutesPerIsland * (currentG_dk * playerEfficiency);
            islandPrices.Add(Mathf.RoundToInt(nextIslandCost));

            simIslandMultiplier *= 3f;
            simKnifeMultiplier *= 1.2f; 
        }

        Debug.Log("<color=green>Ekonomi listeleri matematiksel modele göre başarıyla dolduruldu!</color>");
    }
}