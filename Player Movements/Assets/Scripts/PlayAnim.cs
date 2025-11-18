using UnityEngine;

public class PlayAnim : StateMachineBehaviour
{
    [Header("Settings")]
    public float rayDistance = 5f;
    public KeyCode triggerKey = KeyCode.E;

    [Header("Highlight / Debug")]
    public LayerMask interactLayer;

    private Animator targetAnimator;

    // Update is NOT called automatically in StateMachineBehaviour.
    // You must use OnStateUpdate instead.
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        RaycastHit hit;

        // Use animator.transform instead of transform
        Transform t = animator.transform;

        if (Physics.Raycast(t.position, t.forward, out hit, rayDistance, interactLayer))
        {
            targetAnimator = hit.collider.GetComponent<Animator>();

            Debug.DrawRay(t.position, t.forward * rayDistance, Color.green);

            if (Input.GetMouseButtonDown(0))
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
            targetAnimator = null;
            Debug.DrawRay(t.position, t.forward * rayDistance, Color.red);
        }
    }
}
