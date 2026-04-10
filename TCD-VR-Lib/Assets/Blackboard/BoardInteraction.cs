using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Management;

// A compact network-safe record for one blackboard stroke segment.
public struct StrokeData : INetworkSerializable
{
    public Vector2 StartUV;
    public Vector2 EndUV;
    public bool Erasing;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref StartUV);
        serializer.SerializeValue(ref EndUV);
        serializer.SerializeValue(ref Erasing);
    }
}

public class BoardInteraction : NetworkBehaviour
{
    // Server-only history used to replay the board for late joiners.
    private readonly List<StrokeData> strokeHistory = new();

    [Header("Drawing Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private int textureWidth = 2048;
    [SerializeField] private int textureHeight = 2048;
    [SerializeField] private Color drawColor = Color.white;
    [SerializeField] private Color eraseColor = Color.black;
    [SerializeField] private int brushSize = 5;
    [SerializeField] private int eraserSize = 20;

    [Header("Tool Spawning")]
    [SerializeField] private GameObject chalkPrefab;
    [SerializeField] private GameObject eraserPrefab;
    [SerializeField] private string spawnChairName = "SpawnChair";
    [SerializeField] private string chalkSpawn1Name = "ChalkSpawn1";
    [SerializeField] private string chalkSpawn2Name = "ChalkSpawn2";
    [SerializeField] private string eraserSpawnName = "EraserSpawn";
    [SerializeField] private float interactionToleranceDist = 10.0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    [Header("History")]
    [SerializeField] private int maxStrokeHistory = 5000;

    private Texture2D blackboardTexture;
    private Renderer blackboardRenderer;
    private Material blackboardMaterial;

    private bool isMouseDrawing;
    private bool isEraserMode;
    private Vector2? lastUV;
    private bool desktopInteractionEnabled;
    private bool toolsSpawned;
    private bool loggedDesktopBlocked;

    void Start()
    {
        blackboardRenderer = GetComponent<Renderer>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        CreateBlackboardTexture();
        SpawnBoardTools();

        Debug.Log($"[BoardInteraction] Start on '{name}'. Desktop={IsDesktop()}, Camera={(mainCamera != null ? mainCamera.name : "null")}, Renderer={(blackboardRenderer != null ? blackboardRenderer.name : "null")}", this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            Debug.Log("[BoardInteraction] New client joined - requesting board history.");
            RequestBoardHistoryRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            strokeHistory.Clear();
            Debug.Log("[BoardInteraction] Stroke history cleared on network despawn.");
        }
    }

    void CreateBlackboardTexture()
    {
        blackboardTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        blackboardTexture.filterMode = FilterMode.Bilinear;
        blackboardTexture.wrapMode = TextureWrapMode.Clamp;

        FillBlack();

        blackboardMaterial = new Material(Shader.Find("Unlit/Texture"));
        blackboardMaterial.mainTexture = blackboardTexture;
        blackboardRenderer.material = blackboardMaterial;
    }

    void FillBlack()
    {
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;

        blackboardTexture.SetPixels(pixels);
        blackboardTexture.Apply();
    }

    void Update()
    {
        HandleDesktopInput();
    }

    void HandleDesktopInput()
    {
        if (!desktopInteractionEnabled)
        {
            if (Input.GetMouseButtonDown(0) && !loggedDesktopBlocked)
            {
                Debug.Log("[BoardInteraction] Desktop input blocked because interaction zone is not enabled.", this);
                loggedDesktopBlocked = true;
            }

            isMouseDrawing = false;
            lastUV = null;
            return;
        }

        loggedDesktopBlocked = false;

        if (Input.GetMouseButtonDown(0))
        {
            isMouseDrawing = true;
            Debug.Log($"[BoardInteraction] Mouse draw started. EraserMode={isEraserMode}", this);
            DrawAtMousePosition(true);
        }
        else if (Input.GetMouseButton(0) && isMouseDrawing)
        {
            DrawAtMousePosition(false);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMouseDrawing = false;
            lastUV = null;
            Debug.Log("[BoardInteraction] Mouse draw ended.", this);
        }
    }

    public void DrawFromRay(Ray ray, bool isErasing, Vector2? previousUV)
    {
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionToleranceDist)) return;
        if (hit.collider.gameObject != gameObject) return;

        Vector2 uv = hit.textureCoord;
        Vector2 startUV = previousUV ?? uv;

        ApplyStrokeLocally(startUV, uv, isErasing);
        SubmitStrokeRpc(startUV, uv, isErasing);
    }

    void DrawAtMousePosition(bool verboseLog)
    {
        if (mainCamera == null)
        {
            if (verboseLog)
                Debug.LogWarning("[BoardInteraction] DrawAtMousePosition aborted because mainCamera is null.", this);
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            if (verboseLog)
                Debug.Log("[BoardInteraction] Mouse raycast did not hit anything.", this);
            return;
        }

        if (hit.collider.gameObject != gameObject)
        {
            if (verboseLog)
                Debug.Log($"[BoardInteraction] Mouse raycast hit '{hit.collider.gameObject.name}' instead of board '{gameObject.name}'.", this);
            return;
        }

        Vector2 uv = hit.textureCoord;

        if (verboseLog)
            Debug.Log($"[BoardInteraction] Mouse raycast hit board at UV {uv}.", this);

        Vector2 startUV = lastUV ?? uv;
        ApplyStrokeLocally(startUV, uv, isEraserMode);
        SubmitStrokeRpc(startUV, uv, isEraserMode);

        lastUV = uv;
    }

    void ApplyStrokeLocally(Vector2 startUV, Vector2 endUV, bool erasing)
    {
        DrawStroke(startUV, endUV, erasing);
        blackboardTexture.Apply();
    }

    void DrawStroke(Vector2? previousUV, Vector2 currentUV, bool erasing)
    {
        if (previousUV.HasValue)
        {
            float dist = Vector2.Distance(previousUV.Value, currentUV);
            int steps = Mathf.Max(1, Mathf.CeilToInt(dist * textureWidth));

            for (int i = 0; i <= steps; i++)
            {
                Vector2 lerpedUV = Vector2.Lerp(previousUV.Value, currentUV, i / (float)steps);
                DrawAtUV(lerpedUV, erasing);
            }
        }
        else
        {
            DrawAtUV(currentUV, erasing);
        }
    }

    void DrawStroke(Vector2 startUV, Vector2 endUV, bool erasing)
    {
        DrawStroke((Vector2?)startUV, endUV, erasing);
    }

    void DrawAtUV(Vector2 uv, bool erasing)
    {
        int x = Mathf.Clamp((int)(uv.x * textureWidth), 0, textureWidth - 1);
        int y = Mathf.Clamp((int)(uv.y * textureHeight), 0, textureHeight - 1);
        DrawCircle(x, y, erasing);
    }

    void DrawCircle(int centerX, int centerY, bool erasing)
    {
        int currentSize = erasing ? eraserSize : brushSize;
        Color currentColor = erasing ? eraseColor : drawColor;

        for (int x = -currentSize; x <= currentSize; x++)
        {
            for (int y = -currentSize; y <= currentSize; y++)
            {
                if (x * x + y * y <= currentSize * currentSize)
                {
                    int drawX = centerX + x;
                    int drawY = centerY + y;

                    if (drawX >= 0 && drawX < textureWidth && drawY >= 0 && drawY < textureHeight)
                        blackboardTexture.SetPixel(drawX, drawY, currentColor);
                }
            }
        }
    }

    public void EnableEraser()
    {
        if (!CanUseBoardControls())
        {
            Debug.Log("[BoardInteraction] EnableEraser blocked because desktop interaction is disabled.", this);
            return;
        }

        isEraserMode = true;
        Debug.Log("Eraser mode enabled");
    }

    public void EnableDrawing()
    {
        if (!CanUseBoardControls())
        {
            Debug.Log("[BoardInteraction] EnableDrawing blocked because desktop interaction is disabled.", this);
            return;
        }

        isEraserMode = false;
        Debug.Log("Drawing mode enabled");
    }

    public void ClearBoard()
    {
        if (!CanUseBoardControls())
            return;

        ClearBoardRpc();
        Debug.Log($"[BoardInteraction] ClearBoard called. IsServer={IsServer}");
    }

    public void SetDrawColor(Color color)
    {
        drawColor = color;
    }

    public void SetBrushSize(int size)
    {
        brushSize = Mathf.Clamp(size, 1, 50);
    }

    public void SetEraserSize(int size)
    {
        eraserSize = Mathf.Clamp(size, 1, 100);
    }

    public void SetDesktopInteractionEnabled(bool enabled)
    {
        desktopInteractionEnabled = enabled;
        Debug.Log($"[BoardInteraction] Desktop interaction set to {enabled}.", this);

        if (!enabled)
        {
            isMouseDrawing = false;
            lastUV = null;
        }
    }

    bool CanUseBoardControls()
    {
        if (!IsDesktop())
            return true;

        return desktopInteractionEnabled;
    }

    bool IsDesktop()
    {
        return !(XRGeneralSettings.Instance != null
            && XRGeneralSettings.Instance.Manager != null
            && XRGeneralSettings.Instance.Manager.activeLoader != null);
    }

    void OnDestroy()
    {
        if (blackboardTexture != null)
            Destroy(blackboardTexture);
        if (blackboardMaterial != null)
            Destroy(blackboardMaterial);
    }

    void SpawnBoardTools()
    {
        if (toolsSpawned)
            return;

        if (chalkPrefab == null || eraserPrefab == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("Board tool prefabs are not assigned.", this);
            return;
        }

        Transform spawnChair = transform.Find(spawnChairName);
        if (spawnChair == null)
        {
            if (showDebugInfo)
                Debug.LogWarning($"Could not find '{spawnChairName}' under board.", this);
            return;
        }

        SpawnToolAt(spawnChair.Find(chalkSpawn1Name), chalkPrefab);
        SpawnToolAt(spawnChair.Find(chalkSpawn2Name), chalkPrefab);
        SpawnToolAt(spawnChair.Find(eraserSpawnName), eraserPrefab);

        toolsSpawned = true;
    }

    void SpawnToolAt(Transform spawnPoint, GameObject toolPrefab)
    {
        if (spawnPoint == null || toolPrefab == null)
            return;

        GameObject toolInstance = Instantiate(toolPrefab, spawnPoint.position, spawnPoint.rotation);
        toolInstance.name = toolPrefab.name;

        if (toolInstance.TryGetComponent(out BoardTool boardTool))
        {
            boardTool.SetBoard(this);
            boardTool.interactionToleranceDist = interactionToleranceDist;
        }
    }

    // Clients submit strokes to the authoritative server copy.
    [Rpc(SendTo.Server)]
    void SubmitStrokeRpc(Vector2 startUV, Vector2 endUV, bool erasing)
    {
        strokeHistory.Add(new StrokeData
        {
            StartUV = startUV,
            EndUV = endUV,
            Erasing = erasing
        });

        if (strokeHistory.Count > maxStrokeHistory)
            strokeHistory.RemoveRange(0, Mathf.Min(10, strokeHistory.Count));

        ApplyStrokeLocally(startUV, endUV, erasing);
        BroadcastStrokeRpc(startUV, endUV, erasing);
    }

    // Everyone else replays the same stroke without touching history.
    [Rpc(SendTo.NotMe)]
    void BroadcastStrokeRpc(Vector2 startUV, Vector2 endUV, bool erasing)
    {
        ApplyStrokeLocally(startUV, endUV, erasing);
    }

    // Late joiners ask the server copy for the current board state.
    [Rpc(SendTo.Server)]
    void RequestBoardHistoryRpc(RpcParams rpcParams = default)
    {
        ulong requestingClient = rpcParams.Receive.SenderClientId;
        int batchSize = 50;
        int total = strokeHistory.Count;

        for (int i = 0; i < total; i += batchSize)
        {
            int count = Mathf.Min(batchSize, total - i);
            StrokeData[] batch = new StrokeData[count];
            for (int j = 0; j < count; j++)
                batch[j] = strokeHistory[i + j];

            bool isLastBatch = (i + batchSize) >= total;
            ReceiveBoardHistoryBatchRpc(batch, isLastBatch, RpcTarget.Single(requestingClient, RpcTargetUse.Temp));
        }

        if (total == 0)
            ReceiveBoardHistoryBatchRpc(new StrokeData[0], true, RpcTarget.Single(requestingClient, RpcTargetUse.Temp));
    }

    // Only the requesting client replays the server history.
    [Rpc(SendTo.SpecifiedInParams)]
    void ReceiveBoardHistoryBatchRpc(StrokeData[] batch, bool isLastBatch, RpcParams rpcParams = default)
    {
        foreach (StrokeData stroke in batch)
            DrawStroke(stroke.StartUV, stroke.EndUV, stroke.Erasing);

        if (isLastBatch)
        {
            blackboardTexture.Apply();
            Debug.Log("[BoardInteraction] Board history fully replayed.");
        }
    }

    // Clearing resets both the visible board and the authoritative history.
    [Rpc(SendTo.Server)]
    void ClearBoardRpc()
    {
        strokeHistory.Clear();
        FillBlack();
        BroadcastClearBoardRpc();
        Debug.Log("[BoardInteraction] Stroke history fully cleared on server.");
    }

    [Rpc(SendTo.NotMe)]
    void BroadcastClearBoardRpc()
    {
        FillBlack();
        Debug.Log("[BoardInteraction] Board fully cleared.");
    }
}
