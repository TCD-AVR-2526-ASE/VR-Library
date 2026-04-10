using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Highlighter and Notes system for book pages.
/// Desktop: N = toggle highlighter, T = open/close notes panel, Esc = close notes panel, C = clear highlights.
/// VR: Left secondary button (Y) = toggle highlighter, Left trigger = draw.
/// Place on BookManager.
/// </summary>
public class BookAnnotationUI : MonoBehaviour
{
    [Header("Highlighter Settings")]
    public Color highlightColor = new Color(1f, 1f, 0f, 0.35f);
    [Tooltip("Half-width of the highlighter brush in pixels (horizontal spread).")]
    public int highlightHalfWidth = 20;
    [Tooltip("Half-height of the highlighter brush in pixels (vertical spread).")]
    public int highlightHalfHeight = 6;

    [Header("Raycasting")]
    [SerializeField] private LayerMask bookLayer = ~0;

    // Highlighter state
    private bool highlighterMode;
    private Book activeBook;
    private int activePageIndex = -1;
    private Vector2Int lastPixel = new Vector2Int(-1, -1);
    private bool wasDrawing;
    private bool needsRefresh;
    private int refreshCooldown;

    // Notes state
    public bool NotesPanelOpen => notesPanelOpen;
    private bool notesPanelOpen;
    private int notesPageIndex = -1;
    private string notesBookTitle = "";
    private Book notesBook;
    private string currentNoteText = "";

    // Desktop Controls
    private KeyControl dt_enterAnnotationsBind;     // Keyboard.current.leftCtrlKey
    private KeyControl dt_toggleOpenNotesBind;      // Keyboard.current.tabKey
    private KeyControl dt_clearAnnotationsBind;     // keyboard.current.cKey

    // VR input
    private InputAction vrEnterAnnotationMode;
    private readonly string vr_toggleAnnotationModeBinding = "<XRController>{LeftHand}/secondaryButton";
    private InputAction vrDrawAnnotation;
    private readonly string vr_DrawBinding = "<XRController>{RightHand}/triggerPressed";
    private InputAction vrClearAnnotations;
    private readonly string vr_clearAnnotationsBinding = "<XRController>{LeftHand}/primaryButton";
    private VRBookInteractor vrInteractor;

    // References
    private Camera sceneCamera;
    private BookSystem bookSystem;
    private DesktopBookInteractor desktopInteractor;

    // GUI
    private GUIStyle modeStyle;
    private GUIStyle notesLabelStyle;
    private GUIStyle notesSaveButtonStyle;
    private GUIStyle notesTextAreaStyle;
    private GUIStyle notesIndicatorStyle;
    private bool stylesInitialized;


    private Transform cachedControllerTransform;


    private void Start()
    {
        sceneCamera      = Camera.main;
        bookSystem       = FindFirstObjectByType<BookSystem>();
        desktopInteractor = FindFirstObjectByType<DesktopBookInteractor>();

        vrEnterAnnotationMode = new InputAction("AnnotateToggle", InputActionType.Button);
        vrEnterAnnotationMode.AddBinding(vr_toggleAnnotationModeBinding);
        vrEnterAnnotationMode.Enable();

        vrDrawAnnotation = new InputAction("AnnotateDraw", InputActionType.Button);
        vrDrawAnnotation.AddBinding(vr_DrawBinding);
        vrDrawAnnotation.Enable();

        vrClearAnnotations = new("ClearAnnotations", InputActionType.Button);
        vrClearAnnotations.AddBinding(vr_clearAnnotationsBinding);
        vrClearAnnotations.Enable();

        dt_enterAnnotationsBind = Keyboard.current.leftCtrlKey;
        dt_toggleOpenNotesBind = Keyboard.current.tabKey;
        dt_clearAnnotationsBind = Keyboard.current.cKey;


        var interactor = FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor>();
        if (interactor != null)
            cachedControllerTransform = interactor.transform;
    }

    /// <summary>
    /// Returns true if at least one book exists in the scene.
    /// </summary>
    private bool AnyBookInScene()
    {
        return FindFirstObjectByType<BookIdentity>() != null;
    }

    private void Update()
    {
        // When notes panel is open, let the same notes key or Escape close it cleanly.
        if (notesPanelOpen)
        {
            if (Keyboard.current != null &&
                (dt_toggleOpenNotesBind.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
                ToggleNotesPanel();
            return;
        }

        // Don't respond to any annotation keys until a book is spawned
        if (!AnyBookInScene()) return;

        // handle VR actions

        if (vrEnterAnnotationMode != null && vrEnterAnnotationMode.WasPerformedThisFrame())
        {
            ToggleHighlighterMode();
            VRBookInteractor.SetAllAnnotationModifiers(highlighterMode);
            Debug.Log("Disabled book interactions: " + highlighterMode);
        }

        if (vrClearAnnotations != null && vrClearAnnotations.WasPerformedThisFrame())
            ClearCurrentAnnotations();


        // handle keyboard actions
        if (Keyboard.current != null)
        {
            if (dt_enterAnnotationsBind.wasPressedThisFrame)
                ToggleHighlighterMode();

            if (dt_toggleOpenNotesBind.wasPressedThisFrame)
                ToggleNotesPanel();

            if (highlighterMode && dt_clearAnnotationsBind.wasPressedThisFrame)
                ClearCurrentAnnotations();
        }

        if (highlighterMode)
        {
            if (sceneCamera == null) sceneCamera = Camera.main;
            if (Mouse.current != null && sceneCamera != null)
            {
                HandleDesktopDraw();
                HandleVRDraw();
            }
        }

        if (needsRefresh)
        {
            refreshCooldown--;
            if (refreshCooldown <= 0)
            {
                if (activeBook != null)
                    bookSystem.AddRenderRequest(activeBook);
                needsRefresh = false;
                refreshCooldown = 1;
            }
        }
    }

    // --- Highlighter ---

    private void ToggleHighlighterMode()
    {
        highlighterMode = !highlighterMode;
        lastPixel = new Vector2Int(-1, -1);
        wasDrawing = false;

        if (!highlighterMode)
            SaveCurrentAnnotations();

        Debug.Log(highlighterMode ? "[Highlighter] ON: Draw to highlight. C to clear." : "[Highlighter] OFF");
    }

    private void HandleDesktopDraw()
    {
        bool isDrawing = Mouse.current.leftButton.isPressed;

        if (isDrawing)
        {
            var ray = sceneCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (TryGetPagePixel(ray, out Book book, out int pageIndex, out Vector2Int pixel))
            {
                var tex = BookAnnotationStore.GetTexture(book.title, pageIndex);

                if (activePageIndex != pageIndex)
                    lastPixel = new Vector2Int(-1, -1);

                if (wasDrawing && lastPixel.x >= 0)
                    BookAnnotationStore.DrawHighlighterLine(tex, lastPixel, pixel, highlightHalfWidth, highlightHalfHeight, highlightColor);
                else
                    BookAnnotationStore.DrawHighlighterLine(tex, pixel, pixel, highlightHalfWidth, highlightHalfHeight, highlightColor);

                GetNetworkState(book)?.BroadcastHighlightLine(
                    pageIndex,
                    wasDrawing && lastPixel.x >= 0 ? lastPixel : pixel,
                    pixel,
                    highlightHalfWidth,
                    highlightHalfHeight,
                    highlightColor);

                lastPixel = pixel;
                activeBook = book;
                activePageIndex = pageIndex;
                needsRefresh = true;
                refreshCooldown = 2;
            }
        }
        else
        {
            if (wasDrawing)
            {
                lastPixel = new Vector2Int(-1, -1);
                if (activeBook != null)
                    bookSystem.AddRenderRequest(activeBook);
            }
        }

        wasDrawing = isDrawing;
    }


    private void HandleVRDraw()
    {
        if (vrDrawAnnotation == null || !highlighterMode) return;

        bool isDrawing = vrDrawAnnotation.IsPressed();

        if (isDrawing)
        {
            // Find XR controllers directly — works with any interactor type
            var controllers = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor>(FindObjectsSortMode.None);
            //Debug.Log($"[HandleVRDraw] Found {controllers.Length} XRBaseInputInteractors");

            foreach (var controller in controllers)
            {
                Ray ray = new Ray(controller.transform.position, controller.transform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, 100f, bookLayer))
                {
                    //Debug.Log($"[HandleVRDraw] Raycast HIT: {hit.collider.gameObject.name}");

                    var identity = hit.collider.GetComponentInParent<BookIdentity>();
                    if (identity == null || identity.Book == null) continue;

                    Book book = identity.Book;
                    if (TryHitToPixel(hit, book, out int pageIndex, out Vector2Int pixel))
                    {
                        //Debug.Log($"[HandleVRDraw] Drawing on page {pageIndex} at pixel {pixel}");

                        var tex = BookAnnotationStore.GetTexture(book.title, pageIndex);

                        if (activePageIndex != pageIndex)
                            lastPixel = new Vector2Int(-1, -1);

                        if (wasDrawing && lastPixel.x >= 0)
                            BookAnnotationStore.DrawHighlighterLine(tex, lastPixel, pixel, highlightHalfWidth, highlightHalfHeight, highlightColor);
                        else
                            BookAnnotationStore.DrawHighlighterLine(tex, pixel, pixel, highlightHalfWidth, highlightHalfHeight, highlightColor);

                        lastPixel = pixel;
                        activeBook = book;
                        activePageIndex = pageIndex;
                        needsRefresh = true;
                        refreshCooldown = 2;
                        break;
                    }
                }
            }

            // Fallback: if no XRBaseInputInteractor found either, try ActionBasedController
            if (controllers.Length == 0)
            {
                var actionControllers = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>(FindObjectsSortMode.None);
                Debug.Log($"[HandleVRDraw] Fallback — Found {actionControllers.Length} ActionBasedControllers");

                foreach (var ctrl in actionControllers)
                {
                    Ray ray = new Ray(ctrl.transform.position, ctrl.transform.forward);

                    if (Physics.Raycast(ray, out RaycastHit hit, 100f, bookLayer))
                    {
                        var identity = hit.collider.GetComponentInParent<BookIdentity>();
                        if (identity == null || identity.Book == null) continue;

                        Book book = identity.Book;
                        if (TryHitToPixel(hit, book, out int pageIndex, out Vector2Int pixel))
                        {
                            var tex = BookAnnotationStore.GetTexture(book.title, pageIndex);

                            if (activePageIndex != pageIndex)
                                lastPixel = new Vector2Int(-1, -1);

                            if (wasDrawing && lastPixel.x >= 0)
                                BookAnnotationStore.DrawHighlighterLine(tex, lastPixel, pixel, highlightHalfWidth, highlightHalfHeight, highlightColor);
                            else
                                BookAnnotationStore.DrawHighlighterLine(tex, pixel, pixel, highlightHalfWidth, highlightHalfHeight, highlightColor);

                            lastPixel = pixel;
                            activeBook = book;
                            activePageIndex = pageIndex;
                            needsRefresh = true;
                            refreshCooldown = 2;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (wasDrawing)
            {
                lastPixel = new Vector2Int(-1, -1);
                if (activeBook != null)
                    bookSystem.AddRenderRequest(activeBook);
            }
        }

        wasDrawing = isDrawing;
    }

    // --- Raycasting & Coordinate Mapping ---

    private bool TryGetPagePixel(Ray ray, out Book book, out int pageIndex, out Vector2Int pixel)
    {
        book = null;
        pageIndex = 0;
        pixel = Vector2Int.zero;

        // Only hit BoxColliders — skip any MeshColliders that may exist on children
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, bookLayer);
        foreach (var hit in hits)
        {
            if (!(hit.collider is BoxCollider)) continue;

            var identity = hit.collider.GetComponentInParent<BookIdentity>();
            if (identity == null || identity.Book == null) continue;

            book = identity.Book;
            return TryHitToPixel(hit, book, out pageIndex, out pixel);
        }

        return false;
    }

    /// <summary>
    /// BoxCollider local-space coordinate mapping.
    /// X axis kept as-is, Y axis inverted.
    /// </summary>
    private bool TryHitToPixel(RaycastHit hit, Book book, out int pageIndex, out Vector2Int pixel)
    {
        pageIndex = book.activePage;
        pixel = Vector2Int.zero;

        if (book.BookInstance == null) return false;

        var boxCollider = hit.collider as BoxCollider;
        if (boxCollider == null) return false;

        Transform bookT = boxCollider.transform;
        Vector3 localHit = bookT.InverseTransformPoint(hit.point);

        float halfX = boxCollider.size.x / 2f;
        float halfZ = boxCollider.size.z / 2f;

        float normalizedX = (localHit.x - boxCollider.center.x + halfX) / boxCollider.size.x;
        float normalizedZ = (localHit.z - boxCollider.center.z + halfZ) / boxCollider.size.z;

        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedZ = Mathf.Clamp01(normalizedZ);

        int texSize = BookAnnotationStore.TextureSize;

        float pixelY = normalizedZ * (texSize - 1);

        if (normalizedX < 0.5f)
        {
            pageIndex = book.activePage;
            float pageX = normalizedX * 2f;
            pixel = new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(pageX * (texSize - 1)), 0, texSize - 1),
                Mathf.Clamp(Mathf.RoundToInt(pixelY), 0, texSize - 1)
            );
        }
        else
        {
            pageIndex = book.activePage + 1;
            float pageX = (normalizedX - 0.5f) * 2f;
            pixel = new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(pageX * (texSize - 1)), 0, texSize - 1),
                Mathf.Clamp(Mathf.RoundToInt(pixelY), 0, texSize - 1)
            );
        }

        return true;
    }

    // --- Notes Panel ---

    private void ToggleNotesPanel()
    {
        if (notesPanelOpen)
        {
            if (!string.IsNullOrEmpty(notesBookTitle))
            {
                BookAnnotationStore.SaveNote(notesBookTitle, notesPageIndex, currentNoteText);
                GetNetworkState(notesBook)?.BroadcastNote(notesPageIndex, currentNoteText);
            }
            notesPanelOpen = false;
            Debug.Log("[Notes] Panel closed. Notes saved.");
            return;
        }

        Book book = GetHoveredBook();
        if (book == null)
        {
            Debug.Log("[Notes] Point at a book page first, then press T.");
            return;
        }

        int page = GetHoveredPageIndex(book);

        notesBookTitle = book.title;
        notesBook = book;
        notesPageIndex = page;
        currentNoteText = BookAnnotationStore.GetNote(book.title, page);
        notesPanelOpen = true;
        Debug.Log($"[Notes] Panel opened for '{book.title}' page {page + 1}");
    }

    private Book GetHoveredBook()
    {
        if (desktopInteractor == null)
            desktopInteractor = FindFirstObjectByType<DesktopBookInteractor>();
        if (desktopInteractor != null && desktopInteractor.ActiveBook != null)
            return desktopInteractor.ActiveBook;
        return activeBook;
    }

    private int GetHoveredPageIndex(Book book)
    {
        if (Mouse.current == null || sceneCamera == null) return book.activePage;

        var ray = sceneCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, bookLayer);
        foreach (var hit in hits)
        {
            if (!(hit.collider is BoxCollider)) continue;
            var boxCollider = hit.collider as BoxCollider;
            Vector3 localHit = boxCollider.transform.InverseTransformPoint(hit.point);
            float halfX = boxCollider.size.x / 2f;
            float normalizedX = (localHit.x - boxCollider.center.x + halfX) / boxCollider.size.x;
            return normalizedX < 0.5f ? book.activePage : book.activePage + 1;
        }

        return book.activePage;
    }

    // --- Persistence ---

    private void ClearCurrentAnnotations()
    {
        Book book = GetHoveredBook();
        if (book == null) return;

        BookAnnotationStore.Clear(book.title, book.activePage);
        BookAnnotationStore.Clear(book.title, book.activePage + 1);
        GetNetworkState(book)?.BroadcastClearPage(book.activePage);
        GetNetworkState(book)?.BroadcastClearPage(book.activePage + 1);
        if (bookSystem != null) bookSystem.AddRenderRequest(book);
        Debug.Log($"[Highlighter] Cleared highlights on pages {book.activePage + 1}-{book.activePage + 2}");
    }

    private void SaveCurrentAnnotations()
    {
        if (activeBook == null) return;
        BookAnnotationStore.Save(activeBook.title, activeBook.activePage);
        BookAnnotationStore.Save(activeBook.title, activeBook.activePage + 1);
    }

    // --- GUI ---

    private void InitStyles()
    {
        if (stylesInitialized) return;
        stylesInitialized = true;

        modeStyle = new GUIStyle(GUI.skin.box);
        modeStyle.fontSize = 16;
        modeStyle.normal.textColor = Color.white;
        modeStyle.alignment = TextAnchor.MiddleCenter;

        notesLabelStyle = new GUIStyle(GUI.skin.label);
        notesLabelStyle.fontSize = 64;
        notesLabelStyle.fontStyle = FontStyle.Bold;
        notesLabelStyle.normal.textColor = Color.white;

        notesSaveButtonStyle = new GUIStyle(GUI.skin.button);
        notesSaveButtonStyle.fontSize = 56;

        notesTextAreaStyle = new GUIStyle(GUI.skin.textArea);
        notesTextAreaStyle.fontSize = 56;
        notesTextAreaStyle.wordWrap = true;

        notesIndicatorStyle = new GUIStyle(GUI.skin.box);
        notesIndicatorStyle.fontSize = 12;
        notesIndicatorStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);
        notesIndicatorStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void OnGUI()
    {
        InitStyles();

        if (highlighterMode)
            GUI.Box(new Rect(Screen.width / 2 - 200, 10, 400, 30), "HIGHLIGHTER MODE (N=exit, C=clear)", modeStyle);

        if (!notesPanelOpen)
        {
            Book hovered = GetHoveredBook();
            if (hovered != null)
            {
                bool leftHasNote = BookAnnotationStore.HasNote(hovered.title, hovered.activePage);
                bool rightHasNote = BookAnnotationStore.HasNote(hovered.title, hovered.activePage + 1);
                if (leftHasNote || rightHasNote)
                    GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height - 40, 200, 30), "Notes available (T to view)", notesIndicatorStyle);
            }
        }

        if (notesPanelOpen)
            DrawNotesPanel();
    }

    private void DrawNotesPanel()
    {
        float panelWidth = Screen.width * 0.45f;
        float panelHeight = Screen.height * 0.85f;
        float panelX = Screen.width - panelWidth - 20;
        float panelY = 30;

        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "");
        GUI.backgroundColor = prev;

        GUILayout.BeginArea(new Rect(panelX + 20, panelY + 20, panelWidth - 40, panelHeight - 40));

        GUILayout.Label($"Notes — Page {notesPageIndex + 1}", notesLabelStyle);
        GUILayout.Label($"Book: {notesBookTitle}", GUI.skin.label);
        GUILayout.Space(10);

        currentNoteText = GUILayout.TextArea(currentNoteText, notesTextAreaStyle, GUILayout.Height(panelHeight - 160));

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save & Close", notesSaveButtonStyle, GUILayout.Height(55)))
        {
            BookAnnotationStore.SaveNote(notesBookTitle, notesPageIndex, currentNoteText);
            GetNetworkState(notesBook)?.BroadcastNote(notesPageIndex, currentNoteText);
            notesPanelOpen = false;
            Debug.Log($"[Notes] Saved for '{notesBookTitle}' page {notesPageIndex + 1}");
        }
        if (GUILayout.Button("Clear", notesSaveButtonStyle, GUILayout.Height(55)))
        {
            currentNoteText = "";
        }
        if (GUILayout.Button("Cancel", notesSaveButtonStyle, GUILayout.Height(55)))
        {
            notesPanelOpen = false;
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        if (notesPanelOpen && !string.IsNullOrEmpty(notesBookTitle))
        {
            BookAnnotationStore.SaveNote(notesBookTitle, notesPageIndex, currentNoteText);
            GetNetworkState(notesBook)?.BroadcastNote(notesPageIndex, currentNoteText);
        }

        vrEnterAnnotationMode?.Disable();
        vrEnterAnnotationMode?.Dispose();
        vrDrawAnnotation?.Disable();
        vrDrawAnnotation?.Dispose();
    }

    private NetworkBookState GetNetworkState(Book book)
    {
        if (book == null || book.BookInstance == null)
            return null;

        return book.BookInstance.GetComponent<NetworkBookState>();
    }
}
