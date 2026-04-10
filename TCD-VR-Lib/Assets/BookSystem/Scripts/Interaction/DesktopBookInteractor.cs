using UnityEngine;
using UnityEngine.InputSystem;
using echo17.EndlessBook;

/// <summary>
/// Desktop input implementation for book interaction.
/// Raycasts from screen center (crosshair) to detect books.
/// E = turn forward, Q = turn backward, Middle click = open/close.
/// F = toggle focus mode (zoom in to read text).
/// Integrates with Testing scene's DesktopController + DisableXRIInput
/// to disable movement while a book is open.
/// Place on BookManager (added automatically by BookSystem in desktop mode).
/// </summary>
public class DesktopBookInteractor : MonoBehaviour, IBookInteractor
{
    [Header("Raycasting")]
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private LayerMask bookLayer = ~0;

    [Header("Focus Mode")]
    [Tooltip("How close the camera moves to the book in focus mode.")]
    [SerializeField] private float focusDistance = 0.6f;
    [Tooltip("How fast the camera lerps into/out of focus.")]
    [SerializeField] private float focusLerpSpeed = 8f;

    public Book ActiveBook { get; private set; }

    public bool TurnPageForward  => turnForwardThisFrame;
    public bool TurnPageBackward => turnBackwardThisFrame;
    public bool ToggleOpen       => toggleOpenThisFrame;

    private bool turnForwardThisFrame;
    private bool turnBackwardThisFrame;
    private bool toggleOpenThisFrame;

    private Camera sceneCamera;
    // Use MonoBehaviour references to avoid cross-assembly dependency
    // (BookSystem assembly can't directly reference Assembly-CSharp types)
    private MonoBehaviour movementController;  // DisableXRIInput
    private MonoBehaviour desktopController;   // DesktopController
    private GameObject crosshairObj;
    private bool movementDisabled;

    // Focus mode state
    private bool focusMode;
    private Book focusedBook;
    private Vector3 preFocusPosition;
    private Quaternion preFocusRotation;
    private Transform cameraRig; // the parent we actually move (e.g. XR Origin)

    private void Start()
    {
        sceneCamera = Camera.main;
        FindSceneControllers();
        Debug.Log($"[DesktopBookInteractor] Started. Camera={sceneCamera != null}, DesktopCtrl={desktopController != null}, MovementCtrl={movementController != null}");
    }

    private void FindSceneControllers()
    {
        // Find by type name to avoid cross-assembly reference
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            string typeName = mb.GetType().Name;
            if (typeName == "DisableXRIInput" && movementController == null)
                movementController = mb;
            else if (typeName == "DesktopController" && desktopController == null)
                desktopController = mb;

            if (movementController != null && desktopController != null)
                break;
        }

        // Find the crosshair by tag or by searching children of DesktopController
        if (crosshairObj == null && desktopController != null)
        {
            // DesktopController has a serialized crosshair field — find it via the scene
            var allImages = FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None);
            foreach (var img in allImages)
            {
                if (img.gameObject.name.ToLower().Contains("crosshair") ||
                    img.gameObject.name.ToLower().Contains("cross_hair") ||
                    img.gameObject.name.ToLower().Contains("reticle"))
                {
                    crosshairObj = img.gameObject;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        turnForwardThisFrame = false;
        turnBackwardThisFrame = false;
        toggleOpenThisFrame = false;

        if (sceneCamera == null)
            sceneCamera = Camera.main;
        if (sceneCamera == null) return;

        // Handle focus mode lerp
        if (focusMode && focusedBook != null && focusedBook.BookInstance != null)
        {
            Transform bookT = focusedBook.BookInstance.transform;
            Vector3 targetPos = bookT.position + bookT.up * focusDistance;
            Quaternion targetRot = Quaternion.LookRotation(-bookT.up, bookT.forward);

            if (cameraRig != null)
            {
                cameraRig.position = Vector3.Lerp(cameraRig.position, targetPos, Time.deltaTime * focusLerpSpeed);
                cameraRig.rotation = Quaternion.Slerp(cameraRig.rotation, targetRot, Time.deltaTime * focusLerpSpeed);
            }
        }

        // When in book interaction mode, raycast from mouse position (cursor is free)
        // When not, raycast from screen center (crosshair)
        Ray ray;
        if (movementDisabled && Mouse.current != null)
            ray = sceneCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        else
            ray = sceneCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        Book hitBook = null;

        // Use RaycastAll to check through furniture/scene objects and find books behind them
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, bookLayer);
        float closestBookDist = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            var identity = hits[i].collider.GetComponentInParent<BookIdentity>();
            if (identity != null && hits[i].distance < closestBookDist)
            {
                hitBook = identity.Book;
                closestBookDist = hits[i].distance;
            }
        }

        ActiveBook = hitBook;

        if (Keyboard.current == null) return;

        // Middle click = open/close
        if (Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame && ActiveBook != null)
        {
            toggleOpenThisFrame = true;

            if (ActiveBook.BookInstance != null)
            {
                bool isCurrentlyOpen = ActiveBook.BookInstance.CurrentState == EndlessBook.StateEnum.OpenMiddle;
                if (isCurrentlyOpen)
                {
                    ExitFocusMode();
                    RestoreSceneMovement();
                }
                else
                {
                    DisableSceneMovement();
                }
            }
        }

        // Only allow page turns / focus when looking at an open book
        if (ActiveBook == null || ActiveBook.BookInstance == null) return;
        if (ActiveBook.BookInstance.CurrentState != EndlessBook.StateEnum.OpenMiddle) return;

        if (Keyboard.current.eKey.wasPressedThisFrame) { 
            turnForwardThisFrame = true;
            Debug.Log(turnForwardThisFrame);
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
            turnBackwardThisFrame = true;

        // F = toggle focus mode
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (focusMode)
                ExitFocusMode();
            else
                EnterFocusMode(ActiveBook);
        }
    }

    // --- Focus Mode ---

    private void EnterFocusMode(Book book)
    {
        if (focusMode || book == null || book.BookInstance == null) return;

        // Find the rig to move (camera's root parent, e.g. XR Origin)
        cameraRig = sceneCamera.transform;
        // Walk up to find the top-level rig (stop before scene root)
        while (cameraRig.parent != null && cameraRig.parent.parent != null)
            cameraRig = cameraRig.parent;

        preFocusPosition = cameraRig.position;
        preFocusRotation = cameraRig.rotation;
        focusedBook = book;
        focusMode = true;

        Debug.Log("[BookSystem] Focus mode ON — press F to exit.");
    }

    private void ExitFocusMode()
    {
        if (!focusMode) return;

        if (cameraRig != null)
        {
            cameraRig.position = preFocusPosition;
            cameraRig.rotation = preFocusRotation;
        }

        focusMode = false;
        focusedBook = null;
        Debug.Log("[BookSystem] Focus mode OFF.");
    }

    // --- Movement Control ---

    private void DisableSceneMovement()
    {
        if (movementDisabled) return;

        if (movementController == null || desktopController == null)
            FindSceneControllers();

        if (movementController != null)
            movementController.SendMessage("DisableMovement", SendMessageOptions.DontRequireReceiver);
        if (desktopController != null)
            desktopController.enabled = false;

        // Unlock cursor so mouse can be used for annotation/interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide crosshair while in book mode
        if (crosshairObj != null)
            crosshairObj.SetActive(false);

        movementDisabled = true;
    }

    private void RestoreSceneMovement()
    {
        if (!movementDisabled) return;

        if (movementController != null)
            movementController.SendMessage("EnableMovement", SendMessageOptions.DontRequireReceiver);
        if (desktopController != null)
            desktopController.enabled = true;

        // Re-lock cursor for FPS-style movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Restore crosshair
        if (crosshairObj != null)
            crosshairObj.SetActive(true);

        movementDisabled = false;
    }

    private void OnDisable()
    {
        ExitFocusMode();
        RestoreSceneMovement();
    }

    private void OnDestroy()
    {
        ExitFocusMode();
        RestoreSceneMovement();
    }
}
