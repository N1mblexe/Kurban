using UnityEngine;

public class EnesMerdivenCont : MonoBehaviour
{
    public float nereyeDoncen = 180f;
    public bool artiYon = true;
    private float sayac;
    public float rotSpeed;
    // Update is called once per frame
    void Update()
    {
        if(transform.rotation.y != nereyeDoncen)
        {
            sayac += rotSpeed * Time.deltaTime;

            if (artiYon)
            {
                
                transform.Rotate(0,sayac,0);
            }
            else
            {
                transform.Rotate(0, -sayac, 0);
            }
        }    
    }
}
