using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;
using UnityEngine.XR.Management;
using XRMultiplayer;

// separate controller class for all things keyboard.
public class DesktopController : MonoBehaviour
{
    /// <summary>
    /// reference to the active control map.
    /// </summary>
    [SerializeField]
    private DesktopControls controller;
    /// <summary>
    /// the crosshair UI game object in scene.
    /// </summary>
    [SerializeField]
    private GameObject crosshair;

    private GameObject keyboard;

    /// <summary>
    /// A reference to the jump movement script provided in the XRI template (for same movement).
    /// </summary>
    private JumpProvider jump;
    /// <summary>
    /// A reference to the player options script provided in the XRI template (for settings menu access).
    /// </summary>
    private PlayerOptions options;
    /// <summary>
    /// A reference to the movement enabler script provided in the XRI template (for same movement).
    /// </summary>
    private DisableXRIInput movement;

    private BookController bookController;

    public Transform cameraOffset;   // XR Camera Offset
    // maybe add a field in the settings to adjust this?
    public float sensitivity = 2f;

    /// <summary>
    /// Mouse look rotation for up-down direction. Capped at +- maxYRotationÂ°
    /// </summary>
    private float pitch = 0f;
    /// <summary>
    /// Maximum rotation offset in up or down direction.
    /// </summary>
    private readonly float maxYRotation = 80.0f;
    /// <summary>
    /// Mouse look rotation for left-right direction. Uncapped.
    /// </summary>
    private float yaw = 0f;

    /// <summary>
    /// A boolean determining whether movement is enabled. Inverts mouse freedom (m_enabled => mouse disabled)
    /// </summary>
    private bool m_enabled = true;
    private bool m_in_book = false;
    /// <summary>
    /// A reference to the last object selected by the player (used for determining UI interaction).
    /// </summary>
    private GameObject lastFocused = null;

    /// <summary>
    /// An event to attach any action that should be done when focusing in on an object
    /// </summary>
    public UnityEvent OnFocus;
    /// <summary>
    /// An event to attach any action that should be done when focusing out into the scene.
    /// </summary>
    public UnityEvent OnUnfocus;
    /// <summary>
    /// A separate bool to track whether the settings menu is open or not (issues with the general input mapping otherwise).
    /// </summary>
    private bool settings_open = false;

    private void Start()
    {
        // create a new input map -> maybe should load a saved version if changed?!
        controller = new();
        controller.Enable();

        // get action script references.
        jump = FindFirstObjectByType<JumpProvider>();
        movement = FindFirstObjectByType<DisableXRIInput>();
        bookController = FindFirstObjectByType<BookController>();

        bookController.OnEnterInteract.AddListener(EnterBookInteract);
        bookController.OnExitInteract.AddListener(ExitBookInteract);

        // assign menu interaction to controller
        if (options == null)
            Debug.LogError("Couldn't find PlayerOptions component for Desktop Movement!");
        else
        {
            options.gameObject.SetActive(false);
            // do the regular menu toggle + custom focus/unfocus action (manual because of issues with regular toggle/untoggle)
            controller.UI_Interaction.ToggleSettings.performed += _ => TryOpenSettings();
        }

        // if the current client isn't a desktop app, disable the script.
        if (!IsDesktop())
        {
            crosshair.transform.parent.gameObject.SetActive(false);
            this.enabled = false;
        }

        // if is a desktop app, assign reactions to corresponding input:
        else if (IsDesktop())
        {
            // assign jump to controller
            if (jump == null)
                Debug.LogError("Couldn't find JumpProvider component for Desktop Movement!");
            else
                controller.Movement.Jump.performed += _ => TryJump();
        }
        cameraOffset.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    // called before the first frame
    private void Awake()
    {
        // lock the cursor to middle and hide from view.
        if (IsDesktop())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        options = FindFirstObjectByType<PlayerOptions>();
    }

    // checks whether the current platform is any supported desktop.
    bool IsDesktop()
    {
        return !(XRGeneralSettings.Instance != null
               && XRGeneralSettings.Instance.Manager != null
               && XRGeneralSettings.Instance.Manager.activeLoader != null);
    }

    #region focus handling
    // stuff done when focusing in on a UI element.
    private void OnActionFocus()
    {
        if (!IsDesktop())
            return;

        // disable movement / free looking
        m_enabled = false;
        movement.DisableMovement();

        // show cursor and free from centre
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // hide the crosshair
        if (crosshair != null)
            crosshair.SetActive(false);

        // call any other related event actions.
        OnFocus.Invoke();
    }

    // when interaction with UI objects ends
    private void OnActionUnfocus()
    {
        if (!IsDesktop())
            return;

        // set the view to look at the mouse
        SnapViewToCursor();

        // enable movement / free looking
        m_enabled = true;
        movement.EnableMovement();
        // hide the cursor and put cursor in the middle of the window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // show the crosshair
        if (crosshair != null && IsDesktop())
            crosshair.SetActive(true);

        // call any other related event actions.
        OnUnfocus.Invoke();
    }

    //comparison between previous and current selected objects
    //to determine transition between focused and unfocused
    private void CheckForFocusMode()
    {
        var current = EventSystem.current.currentSelectedGameObject;

        if (current != lastFocused)
        {
            if (lastFocused != null && current == null)
            {
                OnActionUnfocus();
            }

            if (current != null)
            {
                OnActionFocus();
            }

            lastFocused = current;
        }
    }

    private void ManualToggleFocus(bool focus)
    {
        // unfocus when settings open
        if (focus)
        {
            OnActionUnfocus();
            settings_open = false;
        }
        else
        {
            OnActionFocus();
            settings_open = true;
        }
    }
    #endregion

    #region input mapping
    // change the players view to follow the mouse
    private void Gaze()
    {
        var input = controller.Gaze.Look.ReadValue<Vector2>();
        if (input != Vector2.zero)
        {
            pitch -= input.y * sensitivity;
            pitch = Mathf.Clamp(pitch, -maxYRotation, maxYRotation);

            yaw -= -input.x * sensitivity;

            cameraOffset.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    private void SnapViewToCursor()
    {
        if (!IsDesktop())
            return;

        //Debug.Log("This is a desktop client!");

        // Get cursor position relative to screen centre
        Vector2 screenCentre = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 cursorPos = Mouse.current.position.ReadValue();
        Vector2 delta = cursorPos - screenCentre;

        // Normalise by screen height so sensitivity is consistent regardless of resolution
        delta /= Screen.height;

        // Apply sensitivity scaling — multiply by a factor to make the snap feel responsive
        // You can tune this constant or expose it as a field
        float snapScale = 85f;
        pitch -= delta.y * snapScale;
        pitch = Mathf.Clamp(pitch, -maxYRotation, maxYRotation);
        yaw += delta.x * snapScale;

        cameraOffset.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void TryJump()
    {
        if (m_enabled && !m_in_book)
            jump.Jump();
    }
    #endregion

    void Update()
    {
        CheckForFocusMode();

        // do nothing if interaction disabled.
        if (!m_enabled)
            return;

        Gaze();
    }

    void TryOpenSettings()
    {
        if (m_in_book)
            return;

        options.ToggleMenu();
        ManualToggleFocus(settings_open);
    }

    void EnterBookInteract()
    {
        m_in_book = true;
        OnActionFocus();
    }

    void ExitBookInteract()
    {
        m_in_book = false;
        OnActionUnfocus();
    }
}