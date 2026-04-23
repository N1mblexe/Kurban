using UnityEngine;
using Steamworks;
public class SteamAch : MonoBehaviour
{
    public static SteamAch instance;

    [Header("Piramit")]
    public GameObject ufo_pirtamit;
    


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UnlockAchievement("ILK_GIRIS");
        SteamUserStats.GetAchievement("PIRAMIT", out bool isUnlocked);
        if (isUnlocked)
        {
            PiramitInsaEt();
        }
        
    }

    //Basarimlar
    // GOAT
    // PIRAMIT 
    // ILK_GIRIS (ben yaparim)
    // YANAR_DAG
    // HOROZ
    // DEVAM_EDECEK (ben yaparim)

    public void UnlockAchievement(string achievementID)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam baðlý deðil Linuxundan olabilir :)");
            return;
        }

        
        // basarium varmi yokmu kontrol edio
        SteamUserStats.GetAchievement(achievementID, out bool isUnlocked);

        // 2. Eðer alýnmamýþsa (!isUnlocked) verioz
        if (!isUnlocked)
        {
            // basarimi verioz
            SteamUserStats.SetAchievement(achievementID);

            // sunucuya al dioz
            SteamUserStats.StoreStats();

            if(achievementID == "PIRAMIT")
            {
                PiramitInsaEt();
            }

            Debug.Log(achievementID + " baþarýyla kazanýldý!");
        }
        else
        {
            Debug.Log("Zaten vardý yarram");
        }
    }



    public void PiramitInsaEt()
    {
        // burda piramiti olusturcam
        Instantiate(ufo_pirtamit, Vector3.zero, Quaternion.identity);
    }
}
