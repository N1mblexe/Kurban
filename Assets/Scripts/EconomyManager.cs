using UnityEngine;
using TMPro;
using DG.Tweening;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Para Ayarları")]
    public float currentGold = 0;
    public TextMeshProUGUI goldTextUI;

    [Header("Mevcut Çarpanlar")]
    public float currentKnifeMultiplier = 1.0f;
    public float currentIslandBasePrice = 10f;

    public Animator goldAnim;

    private void Awake()
    {
        Instance = this;
        UpdateGoldUI();
        DOTween.SetTweensCapacity(12000, 700);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {

            AddGold(5000);

        }
    }

    public void AddGold(float amount)
    {
        currentGold += amount;
        UpdateGoldUI();
        goldAnim.SetTrigger("a");
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

}