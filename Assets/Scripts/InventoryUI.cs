using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI weightText;

    [HideInInspector]public Image cantaFillImage;

    private void Awake()
    {
        Instance = this;
    }


    // Ağırlık her değiştiğinde bu metod çağrılır
    public void UpdateWeightDisplay(float current, float max)
    {
        weightText.text = $"{current:F1} / {max:F1} kg";
        cantaFillImage.fillAmount = current / max;
    }

    // Çanta tamamen dolduğunda tetiklenecek sarsıntı
    public void ShakeWeightUI()
    {
        weightText.rectTransform.DOKill(true);
        weightText.DOKill(true);

        weightText.rectTransform.DOShakePosition(0.4f, 10f, 20);
        
        Sequence s = DOTween.Sequence();
        s.Append(weightText.DOColor(Color.red, 0.1f));
        s.Append(weightText.DOColor(Color.white, 0.3f));
    }
}