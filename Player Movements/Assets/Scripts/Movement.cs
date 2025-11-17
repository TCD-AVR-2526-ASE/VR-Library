using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;
    public float sprintSpeed = 4f;
    public float walkSpeed = 2f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;

    public Transform cameraPivot;  // Camera parent for vertical rotation

    private bool isMoving = false;
    private bool isSprinting = false;
    private float yRot;
    private float xRot;

    private Animator anim;
    private Rigidbody rb;

    void Start()
    {
        playerSpeed = walkSpeed;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ----- Mouse Look -----
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yRot += mouseX;
        transform.rotation = Quaternion.Euler(0f, yRot, 0f);

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);
        cameraPivot.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        isMoving = false;

        // ----- Movement -----
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = Vector3.zero;

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            moveDir = (transform.right * horizontal + transform.forward * vertical).normalized;
            isMoving = true;
        }

        Vector3 newVel = moveDir * playerSpeed;
        newVel.y = rb.linearVelocity.y;
        rb.linearVelocity = newVel;

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

        // ----- Anim -----
        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
            anim.SetBool("isSprinting", isSprinting);
        }
    }
}
