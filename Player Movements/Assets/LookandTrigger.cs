using UnityEngine;

public class LookAndTrigger : MonoBehaviour
{
    [Header("Settings")]
    public float rayDistance = 5f;
    public KeyCode triggerKey = KeyCode.E;

    [Header("Highlight / Debug")]
    public LayerMask interactLayer;

    private Animator targetAnimator;

    void Update()
    {
        RaycastHit hit;

        // Shoot ray from camera forward
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayDistance, interactLayer))
        {
            // Check if hit object has animator
            targetAnimator = hit.collider.GetComponent<Animator>();

            // Show debug ray so you can see what's happening
            Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.green);

            // If pressing key while looking
            if (Input.GetMouseButtonDown(1))
            {
                if (targetAnimator != null)
                {
                    targetAnimator.SetTrigger("PlayAnim");
                    Debug.Log("Animation Triggered!");
                }
            }
        }
        else
        {
            // Not looking at anything
            targetAnimator = null;
            Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.red);
        }
    }
}
