using UnityEngine;

public class EnesHayvanSes : MonoBehaviour
{

    public AudioClip[] sesler;
    private AudioSource seslendirgec;
    public float maxBeklemeSuresi;
    public float minBeklemeSuresi;
    private float beklemeSuresi;
    private float sayac;
    private void Start()
    {

        if (sesler.Length == 0)
        {
            gameObject.GetComponent<EnesHayvanSes>().enabled = false;
        }

        seslendirgec = GetComponent<AudioSource>();
        beklemeSuresi = Random.Range(minBeklemeSuresi, maxBeklemeSuresi);
    }

    private void Update()
    {
        sayac += Time.deltaTime;

        if(sayac>= beklemeSuresi)
        {
            SesCikart();
            sayac = 0;
        }
    }

    public void SesCikart()
    {
        int rastgeleIndex = Random.Range(0, sesler.Length);
        seslendirgec.clip = sesler[rastgeleIndex];
        seslendirgec.Play();
    }

}
