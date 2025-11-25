using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public Transform hand;
    public float pickupRange = 5f;
    public GameObject currentItem;

    public float grabSpeed = 10f;
    private bool isGrabbing = false;


    void Update()
    {
    
        if (isGrabbing)
            SmoothGrab();

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

        RaycastHit hit;

        // Debug spherecast (for Scene view)
        Debug.DrawRay(origin, direction * maxDistance, Color.cyan);

        if (Physics.SphereCast(origin, radius, direction, out hit, maxDistance))
        {
            if (Physics.SphereCast(origin, radius, direction, out hit, maxDistance))
            {
                DrawSphereCast(origin, direction, maxDistance, radius, Color.green);
            }
                // Make sure ONLY ITEM gets picked
                if (hit.collider.CompareTag("Item"))
            {
                Debug.Log("Picked up: " + hit.collider.name);

                currentItem = hit.collider.gameObject;

                Rigidbody rb = currentItem.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;
                currentItem.transform.SetParent(null);
                isGrabbing = true;

                currentItem.transform.SetParent(hand);
                currentItem.transform.localPosition = Vector3.zero;
                currentItem.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void SmoothGrab()
    {
        if (currentItem == null) return;

        // Move item toward the hand smoothly
        currentItem.transform.position = Vector3.Lerp(
            currentItem.transform.position,
            hand.position,
            Time.deltaTime * grabSpeed
        );

        // Rotate to match hand rotation smoothly
        currentItem.transform.rotation = Quaternion.Slerp(
            currentItem.transform.rotation,
            hand.rotation,
            Time.deltaTime * grabSpeed
        );

        // Check if item is close enough to call it "grabbed"
        float dist = Vector3.Distance(currentItem.transform.position, hand.position);

        if (dist < 0.05f)
        {
            // Snap + parent
            currentItem.transform.SetParent(hand);
            currentItem.transform.localPosition = Vector3.zero;
            currentItem.transform.localRotation = Quaternion.identity;

            isGrabbing = false;
        }
    }

// Draws the full spherecast (start sphere, end sphere, and lines)
void DrawSphereCast(Vector3 origin, Vector3 direction, float distance, float radius, Color color)
{
    DebugDrawSphere(origin, radius, color);

    Vector3 end = origin + direction.normalized * distance;
    DebugDrawSphere(end, radius, color);

    // Connect spheres
    Debug.DrawLine(origin + Vector3.up * radius,    end + Vector3.up * radius,    color);
    Debug.DrawLine(origin - Vector3.up * radius,    end - Vector3.up * radius,    color);
    Debug.DrawLine(origin + Vector3.right * radius, end + Vector3.right * radius, color);
    Debug.DrawLine(origin - Vector3.right * radius, end - Vector3.right * radius, color);
    Debug.DrawLine(origin + Vector3.forward * radius, end + Vector3.forward * radius, color);
    Debug.DrawLine(origin - Vector3.forward * radius, end - Vector3.forward * radius, color);
}


// Draws a simple wireframe sphere (pseudo-debug sphere)
void DebugDrawSphere(Vector3 position, float radius, Color color)
{
    Debug.DrawLine(position + Vector3.up * radius,       position - Vector3.up * radius,       color);
    Debug.DrawLine(position + Vector3.right * radius,    position - Vector3.right * radius,    color);
    Debug.DrawLine(position + Vector3.forward * radius,  position - Vector3.forward * radius,  color);

    Debug.DrawLine(position + Vector3.up * radius,       position + Vector3.right * radius,    color);
    Debug.DrawLine(position + Vector3.right * radius,    position - Vector3.up * radius,       color);
    Debug.DrawLine(position - Vector3.up * radius,       position - Vector3.right * radius,    color);
    Debug.DrawLine(position - Vector3.right * radius,    position + Vector3.up * radius,       color);
}



    void DropItem()
    {
        currentItem.transform.SetParent(null);
        currentItem.GetComponent<Rigidbody>().isKinematic = false;
        currentItem = null;
    }
}
