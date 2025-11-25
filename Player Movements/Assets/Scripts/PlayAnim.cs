using UnityEngine;

public class PlayAnim : MonoBehaviour
{
    public float rayDistance = 5f;
    public KeyCode interactionKey = KeyCode.E;
    public LayerMask interactLayer;

    private Animator playerAnimator;

    void Start()
    {
        // Automatically find the player's animator
        playerAnimator = GetComponentInParent<Animator>();
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool hitSomething = Physics.Raycast(ray, out hit, rayDistance, interactLayer);

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, hitSomething ? Color.green : Color.red);

        if (hitSomething)
        {
            // Press E to play animation
            if (Input.GetKeyDown(interactionKey))
            {
                playerAnimator.SetTrigger("PlayAction");
                Debug.Log("Interaction animation triggered!");
            }
        }
    }
}
