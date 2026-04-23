using UnityEngine;
using Steamworks;
public class SteamAch : MonoBehaviour
{
    public static SteamAch instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UnlockAchievement("ILK_GIRIS");
    }

    //Basarimlar
    // GOAT
    // PIRAMIT (ben yaparim)
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

            Debug.Log(achievementID + " baþarýyla kazanýldý!");
        }
        else
        {
            Debug.Log("Zaten vardý yarram");
        }
    }
}
