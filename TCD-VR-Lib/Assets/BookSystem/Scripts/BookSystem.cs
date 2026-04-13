using echo17.EndlessBook;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Management;

/// <summary>
/// Central orchestrator for the book system.
/// Manages book request and render queues, instantiates book prefabs,
/// and wires up the controller + renderer.
/// Table occupancy is tracked locally and requests auto-claim the next free table.
/// </summary>
public class BookSystem : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Max queue items processed per frame.")]
    [SerializeField] private int maxRequestsPerFrame = 30;

    [Header("Prefab")]
    [SerializeField] private GameObject bookPrefab;

    [Header("Spawn Settings")]
    [Tooltip("The transform above which the book will spawn (e.g. a cube). If null, falls back to camera-based positioning.")]
    [SerializeField] private Transform spawnAnchor;
    [SerializeField] private float spawnHeightOffset = 0.5f;
    [SerializeField] private float bookScale = 1.0f;

    [Header("Spawn Feedback")]
    [SerializeField] private float tablePulseDuration = 10f;
    [SerializeField] private Color tablePulseColor = new Color(1f, 0.85f, 0.4f, 1f);
    [SerializeField] private float tablePulseEmission = 1.6f;

    [Header("Table Spawn Points")]
    [Tooltip("Drag table transforms here. Requests will use the next free one automatically.")]
    public Transform[] tableSpawnPoints = new Transform[5];

    [Tooltip("Optional parent whose direct children will be used as table spawn points. This overrides the manual array.")]
    [SerializeField] private Transform tableSpawnRoot;

    /// <summary>
    /// Table occupancy tracking.
    /// </summary>
    private bool[] tableOccupied;

    // Queues — store table index alongside title
    private Queue<(string title, int tableIndex)> requestQueue;
    private Queue<Book> renderQueue;

    // Track which table each pending request is for (supports async downloads and duplicate titles)
    private Dictionary<string, Queue<int>> pendingTableMap = new Dictionary<string, Queue<int>>();

    // Subsystems
    private BookRepository bookRepo;
    private BookController bookController;
    private BookRenderer bookRenderer;
    private NetworkBookManager networkBookManager;
    private readonly Dictionary<Renderer, Coroutine> activeTablePulses = new Dictionary<Renderer, Coroutine>();

    private void Awake()
    {
        bookRepo = gameObject.AddComponent<BookRepository>();

        if (bookPrefab == null)
            bookPrefab = Resources.Load<GameObject>("BookSystem/Prefabs/Book");

        networkBookManager = GetComponent<NetworkBookManager>();
        if (networkBookManager != null)
            networkBookManager.Configure(this, bookPrefab);

        RefreshTableSpawnPoints();
        tableOccupied = new bool[tableSpawnPoints != null ? tableSpawnPoints.Length : 0];
        requestQueue = new Queue<(string, int)>();
        renderQueue = new Queue<Book>();
    }

    private bool IsDesktop()
    {
        return !(XRGeneralSettings.Instance != null
               && XRGeneralSettings.Instance.Manager != null
               && XRGeneralSettings.Instance.Manager.activeLoader != null);
    }

    private void Start()
    {
        bookController = FindFirstObjectByType<BookController>();
        bookRenderer = FindFirstObjectByType<BookRenderer>();

        if (bookRenderer == null)
        {
            bookRenderer = gameObject.AddComponent<BookRenderer>();
            Debug.Log("[BookSystem] BookRenderer not found — added automatically.");
        }

        // Ensure BookAnnotationUI exists for highlighter + notes
        if (GetComponent<BookAnnotationUI>() == null)
        {
            gameObject.AddComponent<BookAnnotationUI>();
            Debug.Log("[BookSystem] BookAnnotationUI not found — added automatically.");
        }

        // In desktop mode, ensure DesktopBookInteractor exists on BookManager
        if (IsDesktop())
        {
            if (GetComponent<DesktopBookInteractor>() == null)
            {
                gameObject.AddComponent<DesktopBookInteractor>();
                Debug.Log("[BookSystem] Desktop mode — added DesktopBookInteractor.");
            }
        }
        else
        {
            if (GetComponent<VRBookInteractor>() == null)
            {
                gameObject.AddComponent<VRBookInteractor>();
                Debug.Log("[BookSystem] VR mode - added VRBookInteractor");
            }
        }
    }

    private void RefreshTableSpawnPoints()
    {
        if (tableSpawnRoot == null)
        {
            tableSpawnRoot = FindTableSpawnRoot();
        }

        if (tableSpawnRoot == null)
            return;

        var discoveredSpawnPoints = new List<Transform>(tableSpawnRoot.childCount);
        var fallbackSpawnPoints = new List<Transform>(tableSpawnRoot.childCount);
        for (int i = 0; i < tableSpawnRoot.childCount; i++)
        {
            Transform child = tableSpawnRoot.GetChild(i);
            if (child != null)
            {
                fallbackSpawnPoints.Add(child);

                if (child.name.StartsWith("Table_Right_", System.StringComparison.Ordinal) ||
                    child.name.StartsWith("Table_Left_", System.StringComparison.Ordinal))
                    discoveredSpawnPoints.Add(child);
            }
        }

        if (discoveredSpawnPoints.Count == 0)
        {
            discoveredSpawnPoints = fallbackSpawnPoints;
        }
        else
        {
            discoveredSpawnPoints.Sort(CompareNamedTableSpawnPoints);
        }

        if (discoveredSpawnPoints.Count > 0)
        {
            tableSpawnPoints = discoveredSpawnPoints.ToArray();
            Debug.Log($"[BookSystem] Using {tableSpawnPoints.Length} table spawn points from '{tableSpawnRoot.name}'.");
            Debug.Log($"[BookSystem] Table spawn order: {string.Join(", ", System.Array.ConvertAll(tableSpawnPoints, t => t != null ? t.name : "<null>"))}");
        }
    }

    private Transform FindTableSpawnRoot()
    {
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform candidate in allTransforms)
        {
            if (candidate != null && candidate.name == "tables")
                return candidate;
        }

        return null;
    }

    private static int CompareNamedTableSpawnPoints(Transform a, Transform b)
    {
        ParseNamedTableSpawnPoint(a != null ? a.name : string.Empty, out int rowA, out int tableA, out bool isLeftA);
        ParseNamedTableSpawnPoint(b != null ? b.name : string.Empty, out int rowB, out int tableB, out bool isLeftB);

        int rowCompare = rowA.CompareTo(rowB);
        if (rowCompare != 0) return rowCompare;

        if (isLeftA != isLeftB)
            return isLeftA ? 1 : -1; // Right side first, then left side, within each row.

        return tableA.CompareTo(tableB);
    }

    private static void ParseNamedTableSpawnPoint(string name, out int row, out int table, out bool isLeft)
    {
        row = int.MaxValue;
        table = int.MaxValue;
        isLeft = false;

        if (string.IsNullOrEmpty(name))
            return;

        isLeft = name.StartsWith("Table_Left_", System.StringComparison.Ordinal);

        int rowIndex = name.LastIndexOf("_R", System.StringComparison.Ordinal);
        int tableIndex = name.LastIndexOf("_T", System.StringComparison.Ordinal);

        if (rowIndex >= 0 && tableIndex > rowIndex + 2)
            int.TryParse(name.Substring(rowIndex + 2, tableIndex - (rowIndex + 2)), out row);

        if (tableIndex >= 0 && tableIndex + 2 <= name.Length - 1)
            int.TryParse(name.Substring(tableIndex + 2), out table);
    }

    private void Update()
    {
        if (requestQueue != null && requestQueue.Count > 0)
        {
            int count = Mathf.Min(maxRequestsPerFrame, requestQueue.Count);
            for (int i = 0; i < count; i++)
            {
                var (title, tableIndex) = requestQueue.Dequeue();
                string key = title.ToLower();
                if (!pendingTableMap.TryGetValue(key, out Queue<int> pendingTables))
                {
                    pendingTables = new Queue<int>();
                    pendingTableMap[key] = pendingTables;
                }

                pendingTables.Enqueue(tableIndex);
                bookRepo.RequestBook(title);
            }
        }

        if (bookRenderer != null && renderQueue != null)
            ProcessQueue(renderQueue, maxRequestsPerFrame, book => bookRenderer.DisplayCurrent(book));
    }

    private void ProcessQueue<T>(Queue<T> queue, int maxPerFrame, System.Action<T> action)
    {
        int count = Mathf.Min(maxPerFrame, queue.Count);
        for (int i = 0; i < count; i++)
            action(queue.Dequeue());
    }

    public void AddRenderRequest(Book book)
    {
        renderQueue.Enqueue(book);
    }

    /// <summary>
    /// Called from BookRowUI via SendMessage.
    /// Any legacy "title|tableIndex" payload will be normalized back to just the title.
    /// </summary>
    public void AddBookRequest(string bookRequest)
    {
        Debug.Log($"[BookSystem] AddBookRequest received: '{bookRequest}'");

        string title = bookRequest;
        int separatorIndex = bookRequest.LastIndexOf('|');
        if (separatorIndex >= 0)
        {
            title = bookRequest.Substring(0, separatorIndex);
        }

        title = title.Trim();

        if (networkBookManager != null && networkBookManager.CanNetworkBooks())
        {
            networkBookManager.RequestSharedBook(title);
            Debug.Log($"[BookSystem] Forwarded shared book request for '{title}' to NetworkBookManager.");
            return;
        }

        int tableIndex = GetNextAvailableTableIndex();

        if (tableIndex >= 0)
        {
            tableOccupied[tableIndex] = true;
            requestQueue.Enqueue((title, tableIndex));
            Debug.Log($"[BookSystem] Auto-assigned Table {tableIndex + 1} for '{title}'");
        }
        else
        {
            requestQueue.Enqueue((title, -1));
            Debug.LogWarning($"[BookSystem] No free table found for '{title}'. Using fallback spawn.");
        }
    }

    private int GetNextAvailableTableIndex()
    {
        if (tableSpawnPoints == null || tableOccupied == null)
            return -1;

        int count = Mathf.Min(tableSpawnPoints.Length, tableOccupied.Length);
        for (int i = 0; i < count; i++)
        {
            if (tableSpawnPoints[i] != null && !tableOccupied[i])
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Returns a short UI-friendly label for the next table that would be used by an auto-routed request.
    /// </summary>
    public string GetNextAvailableTableLabel()
    {
        int tableIndex = GetNextAvailableTableIndex();
        return tableIndex >= 0 ? $"Table {tableIndex + 1}" : "Fallback Spawn";
    }

    public bool TryReserveNextAvailableTable(out int tableIndex)
    {
        tableIndex = GetNextAvailableTableIndex();
        if (tableIndex < 0)
            return false;

        tableOccupied[tableIndex] = true;
        Debug.Log($"[BookSystem] Reserved Table {tableIndex + 1}.");
        return true;
    }

    public void MarkTableOccupied(int index)
    {
        if (index < 0 || tableOccupied == null || index >= tableOccupied.Length)
            return;

        tableOccupied[index] = true;
    }

    /// <summary>
    /// Returns whether a specific table is occupied (for UI button state).
    /// </summary>
    public bool IsTableOccupied(int index)
    {
        if (index < 0 || index >= tableOccupied.Length) return false;
        return tableOccupied[index];
    }

    public void ClearLocalCache()
    {
        if (bookRepo != null)
            bookRepo.ClearLocalCache();
    }

    public void ClearTableOccupancy(int index)
    {
        if (index < 0 || tableOccupied == null || index >= tableOccupied.Length)
            return;
        tableOccupied[index] = false;
        Debug.Log($"[BookSystem] Table {index + 1} freed.");
    }

    /// <summary>
    /// Instantiate a 3D book, attach interactor components, and wire everything up.
    /// Spawns at the pending table's spawn point if set, otherwise uses spawnAnchor or camera.
    /// </summary>
    public void ProcessBookRequest(Book book)
    {
        int tableIndex = -1;
        string key = book.title != null ? book.title.ToLower() : "";
        if (pendingTableMap.TryGetValue(key, out Queue<int> pendingTables) && pendingTables.Count > 0)
        {
            tableIndex = pendingTables.Dequeue();
            if (pendingTables.Count == 0)
                pendingTableMap.Remove(key);
        }

        if (networkBookManager != null && networkBookManager.CanNetworkBooks())
        {
            networkBookManager.SpawnSharedBook(book.title, tableIndex, book.id);
            return;
        }

        SpawnLocalBook(book, tableIndex);
    }

    private void SpawnLocalBook(Book book, int tableIndex)
    {
        ResolveSpawnPose(tableIndex, out Vector3 spawnPos, out Quaternion spawnRot);

        if (tableIndex >= 0)
        {
            Debug.Log($"[BookSystem] Spawning book on Table {tableIndex + 1}");
            PulseTableHighlightForIndex(tableIndex);
        }

        GameObject bookObj = Instantiate(bookPrefab, spawnPos, spawnRot);
        bookObj.transform.localScale = Vector3.one * bookScale;
        BindBookToSpawnedObject(book, bookObj);
        SetupPhysics(bookObj);
        AttachInteractor(bookObj);
    }

    public void BindBookToSpawnedObject(Book book, GameObject bookObj)
    {
        if (book == null || bookObj == null)
            return;

        EndlessBook endlessBook = bookObj.GetComponent<EndlessBook>();
        book.SetBookInstance(endlessBook);

        var identity = bookObj.GetComponent<BookIdentity>();
        if (identity == null)
            identity = bookObj.AddComponent<BookIdentity>();
        identity.Init(book);

        if (bookController != null)
            bookController.SetBook(book);
        if (bookRenderer != null)
            bookRenderer.DisplayCurrent(book);
    }

    public void PrepareBookObjectForNetworking(GameObject bookObj)
    {
        SetupPhysics(bookObj);

        var grab = bookObj.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            grab.throwOnDetach = false;
        }
        else
        {
            Debug.LogWarning("[BookSystem] Networked book prefab is missing XRGrabInteractable.");
        }

        if (bookObj.GetComponent<VRBookInteractor>() == null)
            Debug.LogWarning("[BookSystem] Networked book prefab is missing VRBookInteractor.");
    }

    public void ResolveSpawnPose(int tableIndex, out Vector3 spawnPos, out Quaternion spawnRot)
    {
        spawnRot = Quaternion.identity;

        if (tableIndex >= 0 && tableSpawnPoints != null && tableIndex < tableSpawnPoints.Length && tableSpawnPoints[tableIndex] != null)
        {
            spawnPos = tableSpawnPoints[tableIndex].position + Vector3.up * spawnHeightOffset;
            spawnRot = tableSpawnPoints[tableIndex].rotation;
            return;
        }

        if (spawnAnchor != null)
        {
            spawnPos = spawnAnchor.position + Vector3.up * spawnHeightOffset;
            return;
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            spawnPos = cam.transform.position + cam.transform.forward * 2f;
            spawnPos.y += spawnHeightOffset;
            Vector3 lookDir = cam.transform.position - spawnPos;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                spawnRot = Quaternion.LookRotation(lookDir);
        }
        else
        {
            spawnPos = Vector3.up * spawnHeightOffset;
        }
    }

    public void PulseTableHighlightForIndex(int tableIndex)
    {
        if (tableSpawnPoints == null || tableIndex < 0 || tableIndex >= tableSpawnPoints.Length)
            return;

        PulseTableHighlight(tableSpawnPoints[tableIndex]);
    }

    private void PulseTableHighlight(Transform tableSlot)
    {
        Renderer targetRenderer = FindTableRenderer(tableSlot);
        if (targetRenderer == null)
            return;

        if (activeTablePulses.TryGetValue(targetRenderer, out Coroutine runningPulse) && runningPulse != null)
        {
            StopCoroutine(runningPulse);
        }

        activeTablePulses[targetRenderer] = StartCoroutine(PulseTableHighlightRoutine(targetRenderer));
    }

    private Renderer FindTableRenderer(Transform tableSlot)
    {
        if (tableSlot == null)
            return null;

        for (int i = 0; i < tableSlot.childCount; i++)
        {
            Renderer childRenderer = tableSlot.GetChild(i).GetComponent<Renderer>();
            if (childRenderer != null)
                return childRenderer;
        }

        return tableSlot.GetComponentInChildren<Renderer>(true);
    }

    private System.Collections.IEnumerator PulseTableHighlightRoutine(Renderer targetRenderer)
    {
        if (targetRenderer == null)
            yield break;

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(propertyBlock);

        Material sharedMaterial = targetRenderer.sharedMaterial;
        bool hasBaseColor = sharedMaterial != null && sharedMaterial.HasProperty("_BaseColor");
        bool hasEmissionColor = sharedMaterial != null && sharedMaterial.HasProperty("_EmissionColor");

        Color originalBaseColor = hasBaseColor ? propertyBlock.GetColor("_BaseColor") : Color.white;
        if (hasBaseColor && originalBaseColor == default)
            originalBaseColor = sharedMaterial.GetColor("_BaseColor");

        Color originalEmissionColor = hasEmissionColor ? propertyBlock.GetColor("_EmissionColor") : Color.black;
        if (hasEmissionColor && originalEmissionColor == default)
            originalEmissionColor = sharedMaterial.GetColor("_EmissionColor");

        float elapsed = 0f;
        while (elapsed < tablePulseDuration)
        {
            float t = elapsed / tablePulseDuration;
            float pulse = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 6f);

            targetRenderer.GetPropertyBlock(propertyBlock);

            if (hasBaseColor)
            {
                Color pulseBaseColor = Color.Lerp(originalBaseColor, tablePulseColor, 0.35f + (pulse * 0.35f));
                propertyBlock.SetColor("_BaseColor", pulseBaseColor);
            }

            if (hasEmissionColor)
            {
                Color pulseEmissionColor = tablePulseColor * (pulse * tablePulseEmission);
                propertyBlock.SetColor("_EmissionColor", originalEmissionColor + pulseEmissionColor);
            }

            targetRenderer.SetPropertyBlock(propertyBlock);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetRenderer.GetPropertyBlock(propertyBlock);
        if (hasBaseColor)
            propertyBlock.SetColor("_BaseColor", originalBaseColor);
        if (hasEmissionColor)
            propertyBlock.SetColor("_EmissionColor", originalEmissionColor);

        targetRenderer.SetPropertyBlock(propertyBlock);
        activeTablePulses.Remove(targetRenderer);
    }

    /// <summary>
    /// Ensures the book has a Rigidbody and collider for interaction.
    /// </summary>
    private void SetupPhysics(GameObject bookObj)
    {
        var rb = bookObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        else
        {
            Debug.LogWarning("[BookSystem] Book prefab is missing Rigidbody.");
        }

        var box = bookObj.GetComponent<BoxCollider>();
        if (box != null)
        {
            FitBookCollider(bookObj, box);
            return;
        }

        Debug.LogWarning("[BookSystem] Book prefab is missing BoxCollider.");
    }

    private void FitBookCollider(GameObject bookObj, BoxCollider box)
    {
        Renderer standinRenderer = null;
        var allRenderers = bookObj.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in allRenderers)
        {
            if (r.gameObject.name.Contains("BookStandinOpenMiddle"))
            {
                standinRenderer = r;
                break;
            }
        }

        if (standinRenderer != null)
        {
            bool wasActive = standinRenderer.gameObject.activeSelf;
            standinRenderer.gameObject.SetActive(true);

            Bounds bounds = standinRenderer.bounds;
            box.center = bookObj.transform.InverseTransformPoint(bounds.center);
            Vector3 size = bookObj.transform.InverseTransformVector(bounds.size);
            float ySize = Mathf.Max(Mathf.Abs(size.y), 0.15f);
            box.size = new Vector3(Mathf.Abs(size.x), ySize, Mathf.Abs(size.z));

            standinRenderer.gameObject.SetActive(wasActive);
        }
        else
        {
            box.center = new Vector3(0f, box.center.y, box.center.z);
            box.size = new Vector3(box.size.x * 2f, box.size.y, box.size.z);
        }
    }

    /// <summary>
    /// Attaches VRBookInteractor if XR is running, otherwise DesktopBookInteractor.
    /// </summary>
    private void AttachInteractor(GameObject bookObj)
    {
        bool xrActive = UnityEngine.XR.XRSettings.isDeviceActive;

        if (xrActive)
        {
            var grab = bookObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null)
            {
                grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;
                grab.throwOnDetach = false;
            }
            else
            {
                Debug.LogWarning("[BookSystem] Book prefab is missing XRGrabInteractable.");
            }

            if (bookObj.GetComponent<VRBookInteractor>() == null)
                Debug.LogWarning("[BookSystem] Book prefab is missing VRBookInteractor.");
        }
    }

    public Book FindBookByID(int id)
    {
        if (id < 0 || bookRepo == null)
            return null;

        return bookRepo.GetBook(id);
    }

    public float BookScale => bookScale;
}
