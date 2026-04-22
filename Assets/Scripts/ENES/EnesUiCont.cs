using UnityEngine;
using UnityEngine.UI;

public class EnesUiCont : MonoBehaviour
{
    public Image cantaFillImage;
    public Animator goldAnim;

    private void Start()
    {
        InventoryUI.Instance.cantaFillImage = cantaFillImage;
        EconomyManager.Instance.goldAnim = goldAnim;
    }

}
