using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public Transform hand;
    public float pickupRange = 3f;
    public GameObject currentItem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentItem == null)
                TryPickup();
            else
                DropItem();
        }
    }

    void TryPickup()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.CompareTag("Item"))
            {
                currentItem = hit.collider.gameObject;
                currentItem.GetComponent<Rigidbody>().isKinematic = true;
                currentItem.transform.SetParent(hand);
                currentItem.transform.localPosition = Vector3.zero;
                currentItem.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void DropItem()
    {
        currentItem.transform.SetParent(null);
        currentItem.GetComponent<Rigidbody>().isKinematic = false;
        currentItem = null;
    }
}
