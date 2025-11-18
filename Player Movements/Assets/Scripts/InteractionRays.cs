using UnityEngine;

public class InteractionRays : MonoBehaviour
{
    public float rayDistance = 5f;
    public KeyCode triggerKey = KeyCode.E;
    public LayerMask interactLayer;

    private Animator targetAnimator;

    void Update()
    {
        RaycastHit hit;
        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward;

        if (Physics.Raycast(origin, direction, out hit, rayDistance, interactLayer))
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.green);

            targetAnimator = hit.collider.GetComponent<Animator>();

            if (Input.GetKeyDown(triggerKey))
            {
                if (targetAnimator != null)
                {
                    targetAnimator.SetTrigger("PlayAnim");
                    Debug.Log("Animation triggered on: " + hit.collider.name);
                }
            }
        }
        else
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.red);
            targetAnimator = null;
        }
    }
}
