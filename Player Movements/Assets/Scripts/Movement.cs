using UnityEngine;

public class Movement : MonoBehaviour
{
    public float playerSpeed;
    public float sprintSpeed = 10f;
    public float walkSpeed = 5f;
    public float mouseSensitivity = 1f;
    public float jumpForce = 5f;

    public Transform cameraPivot;

    private bool isMoving = false;
    private bool isSprinting = false;
    private bool isJumping = false;

    private float xRot = 0f;
    private Animator anim;
    private Rigidbody rb;

    void Start()
    {
        playerSpeed = walkSpeed;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJump();
        HandleSprint();
        HandleAnimations();
    }

    // ---------------- CAMERA LOOK ----------------
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0f, mouseX, 0f);

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRot -= mouseY / 2;
        xRot = Mathf.Clamp(xRot, -80f, 80f);
        cameraPivot.localRotation = Quaternion.Euler(xRot, 0f, 0f);
    }

    // ---------------- MOVEMENT ----------------
    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

        Vector3 moveDir = (transform.right * horizontal + transform.forward * vertical).normalized;

        Vector3 vel = moveDir * playerSpeed;
        vel.y = rb.linearVelocity.y;        // FIXED
        rb.linearVelocity = vel;            // FIXED
    }

    // ---------------- JUMP ----------------
    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Ground"))
            isJumping = false;
    }

    // ---------------- SPRINT ----------------
    void HandleSprint()
    {
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
    }

    // ---------------- ANIMATIONS ----------------
    void HandleAnimations()
    {
        if (anim == null) return;

        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isSprinting", isSprinting);
        anim.SetBool("isJumping", isJumping);
    }
}
