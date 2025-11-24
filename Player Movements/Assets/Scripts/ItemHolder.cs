using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public Transform hand;
    public float pickupRange = 5f;
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
        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward;

        float radius = 0.5f; // radius of the sphere (tweak)
        float maxDistance = pickupRange;

        Debug.DrawRay(origin, direction * maxDistance, Color.red);

        RaycastHit hit;

        // Debug spherecast (for Scene view)
        Debug.DrawRay(origin, direction * maxDistance, Color.cyan);

        if (Physics.SphereCast(origin, radius, direction, out hit, maxDistance))
        {
            // Make sure ONLY ITEM gets picked
            if (hit.collider.CompareTag("Item"))
            {
                Debug.Log("Picked up: " + hit.collider.name);

                currentItem = hit.collider.gameObject;

                Rigidbody rb = currentItem.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;

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
