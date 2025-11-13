using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;
    public float sprintSpeed = 4f;
    public float walkSpeed = 2f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;

    private bool isMoving = false;
    private bool isSprinting = false;
    private float yRot;

    private Animator anim;
    private Rigidbody rb;

    void Start()
    {
        playerSpeed = walkSpeed;

        anim = GetComponentInChildren<Animator>();  
        rb = GetComponentInChildren<Rigidbody>();

        rb.freezeRotation = true; // prevents physics rotation problems
    }

    void Update()
    {
        // Reset movement flag
        isMoving = false;

        // ----- Movement -----
        Vector3 moveDir = Vector3.zero;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            moveDir = (transform.right * horizontal + transform.forward * vertical).normalized;
            isMoving = true;
        }

        // Apply movement
        Vector3 newVel = moveDir * playerSpeed;
        newVel.y = rb.velocity.y;        // keep existing Y velocity (gravity)
        rb.velocity = newVel;

        // ----- Jump -----
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // ----- Sprint -----
        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerSpeed = sprintSpeed;
            isSprinting = true;
        }
        else
        {
            playerSpeed = walkSpeed;
            isSprinting = false;
        }

        // ----- Safe Animator Checks -----
        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
            anim.SetBool("isSprinting", isSprinting);
        }
    }
}
