using UnityEngine;

public class EnesMerdivenCont : MonoBehaviour
{
    public float hedefAci = 180f; // Dönmek istediðin hedef (Y ekseni)
    public float donmeHizi = 90f; // Saniyede kaç derece döneceði

    void Update()
    {
        // 1. Hedef yönü bir Quaternion (rotasyon verisi) olarak tanýmlýyoruz
        Quaternion hedefRotasyon = Quaternion.Euler(0, hedefAci, 0);

        // 2. Mevcut rotasyondan hedefe doðru, belirlenen hýzla yumuþak bir geçiþ yapýyoruz
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            hedefRotasyon,
            donmeHizi * Time.deltaTime
        );
    }
}
