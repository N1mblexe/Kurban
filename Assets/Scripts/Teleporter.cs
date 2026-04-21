using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform teleportPoint;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            CharacterController controller = other.gameObject.GetComponent<CharacterController>();
            controller.enabled = false;

            PlayerInventory.Instance.FreeInventory();
            other.transform.position = teleportPoint.transform.position;

            controller.enabled = true;

        }

        else
        {
            Destroy(other.gameObject);
        }
    }
}
