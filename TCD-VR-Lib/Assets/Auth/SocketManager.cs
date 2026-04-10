using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Concurrent;
using Unity.Netcode;

public class WebSocketManager : MonoBehaviour
{
    private static WebSocketManager _instance;

    /// <summary>
    /// Static instance for global access. Auto-generates a GameObject if missing in the scene.
    /// </summary>
    public static WebSocketManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WebSocketManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("_WebSocketSystem_Auto");
                    _instance = go.AddComponent<WebSocketManager>();
                    Debug.Log("[WebSocketManager] Auto-generating instance GameObject.");
                }
            }
            return _instance;
        }
    }

    private ClientWebSocket wsClient;
    private CancellationTokenSource cancellationTokenSource;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    // Connection health monitoring variables
    private float lastServerHeartbeatTime;
    private readonly int clientHeartbeatInterval = 8000; // 8 seconds
    private readonly int serverTimeoutThreshold = 15;    // 15 seconds

    // Watchdog activation flag to prevent timing conflicts during initialization
    private bool isWatchdogActive = false;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>. Initializes the singleton and persists across scenes.
    /// </summary>
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>. Handles message dispatching and connection monitoring on the main thread.
    /// </summary>
    void Update()
    {
        // Dispatch incoming messages to the main thread
        while (messageQueue.TryDequeue(out string message))
        {
            ProcessMessage(message);
        }

        // Watchdog: Monitor server connection timeout ONLY when active
        if (isWatchdogActive && wsClient != null && wsClient.State == WebSocketState.Open)
        {
            if (Time.time - lastServerHeartbeatTime > serverTimeoutThreshold)
            {
                Debug.LogWarning("[WebSocketManager] No heartbeat received from the server for an extended period. Preparing to disconnect.");
                Disconnect();
            }
        }
    }

    /// <summary>
    /// Connects to the WebSocket server at the specified URI based on the token.
    /// This method initializes the client and starts both the receive and heartbeat loops.
    /// </summary>
    /// <param name="tokenData">The authentication token information.</param>
    public async Task Connect(TokenVo tokenData)
    {
        // Assuming RequestUtils._baseIP and TokenVo structure are defined elsewhere in your project
        string token = $"{tokenData.tokenHead}{tokenData.token}";
        string uri = $"ws://{RequestUtils._baseIP}:8877/im?Authorization={token}";

        wsClient = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await wsClient.ConnectAsync(new Uri(uri), cancellationTokenSource.Token);
            Debug.Log("[WebSocketManager] WebSocket connected successfully!");

            lastServerHeartbeatTime = Time.time;
            isWatchdogActive = true;

            // Start async background loops
            _ = ReceiveLoop();
            _ = ClientHeartbeatLoop();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[WebSocketManager] WebSocket connection error: {ex.Message}");
            isWatchdogActive = false; // Ensure watchdog stays off if connection fails
        }
    }

    /// <summary>
    /// Sends a kickoff command to the server to disconnect a specific user (Admin privilege required).
    /// </summary>
    /// <param name="targetUserId">The ID of the user to be kicked off.</param>
    public async Task SendKickoffMessage(long targetUserId)
    {
        if (wsClient == null || wsClient.State != WebSocketState.Open)
        {
            Debug.LogWarning("[WebSocketManager] Cannot send kickoff command: WebSocket is not connected.");
            return;
        }

        try
        {
            KickoffMessage kickMsg = new KickoffMessage { type = "kickoff", userId = targetUserId };
            string jsonMsg = JsonUtility.ToJson(kickMsg);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonMsg);
            ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);

            await wsClient.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationTokenSource.Token);
            Debug.Log($"[WebSocketManager] Kickoff command sent successfully for Target User ID: {targetUserId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[WebSocketManager] Failed to send kickoff command: {ex.Message}");
        }
    }

    /// <summary>
    /// Client heartbeat loop that periodically sends a ping to the WebSocket server to maintain the connection.
    /// </summary>
    private async Task ClientHeartbeatLoop()
    {
        HeartbeatMessage hbMsg = new HeartbeatMessage();
        string jsonMsg = JsonUtility.ToJson(hbMsg);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonMsg);

        while (wsClient != null && wsClient.State == WebSocketState.Open)
        {
            try
            {
                await Task.Delay(clientHeartbeatInterval, cancellationTokenSource.Token);

                ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);
                await wsClient.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationTokenSource.Token);

                Debug.Log("[WebSocketManager] Heartbeat packet sent to the server.");
            }
            catch (TaskCanceledException)
            {
                break; // Exit gracefully on cancellation
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] Heartbeat send operation failed: {ex.Message}");
                break;
            }
        }
    }

    /// <summary>
    /// Background loop that continuously listens for incoming messages from the server.
    /// </summary>
    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[4096];

        while (wsClient != null && wsClient.State == WebSocketState.Open)
        {
            try
            {
                var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("[WebSocketManager] Connection was closed gracefully by the server.");
                    Disconnect();
                    break;
                }

                string receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                messageQueue.Enqueue(receivedText);
            }
            catch (TaskCanceledException)
            {
                break; // Exit gracefully on cancellation
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebSocketManager] Data receive operation failed: {ex.Message}");
                Disconnect();
                break;
            }
        }
    }

    /// <summary>
    /// Message router: Parses incoming JSON and dispatches it to the corresponding business logic based on the 'type' field.
    /// </summary>
    /// <param name="jsonMessage">The raw JSON string received from the server.</param>
    private void ProcessMessage(string jsonMessage)
    {
        try
        {
            BaseMessage baseMsg = JsonUtility.FromJson<BaseMessage>(jsonMessage);

            if (baseMsg == null || string.IsNullOrEmpty(baseMsg.type))
            {
                Debug.LogWarning($"[WebSocketManager] Received malformed message format: {jsonMessage}");
                return;
            }

            switch (baseMsg.type)
            {
                case "heartbeat":
                    lastServerHeartbeatTime = Time.time;
                    break;

                case "kickoff":
                    KickoffMessage kickMsg = JsonUtility.FromJson<KickoffMessage>(jsonMessage);
                    HandleKickedOff(kickMsg);
                    break;

                default:
                    Debug.Log($"[WebSocketManager] Unhandled business data received [Type: {baseMsg.type}]: {jsonMessage}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[WebSocketManager] Failed to parse incoming message: {jsonMessage}. Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the business logic when the current client is kicked off by the server.
    /// </summary>
    /// <param name="kickMsg">The deserialized kickoff message data.</param>
    private void HandleKickedOff(KickoffMessage kickMsg)
    {
        Debug.LogWarning($"[WebSocketManager] Received kickoff notification from server! Associated User ID: {kickMsg.userId}");
        try
        {
            //dispatch NetworkManager kickoff event here to trigger client-side logout and cleanup
            NetworkManager.Singleton.Shutdown();

        }
        catch (Exception ex)
        {
            Debug.LogError($"[WebSocketManager] Error during client shutdown after kickoff: {ex.Message}");
        }
    }

    /// <summary>
    /// Disconnects from the WebSocket server and safely disposes of unmanaged resources.
    /// </summary>
    public void Disconnect()
    {
        // Deactivate the watchdog IMMEDIATELY to prevent it from triggering during cleanup
        isWatchdogActive = false;

        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        if (wsClient != null)
        {
            if (wsClient.State == WebSocketState.Open)
            {
                // Asynchronous close with a timeout to avoid blocking the main thread indefinitely
                wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Disconnecting", CancellationToken.None).Wait(1000);
            }
            wsClient.Dispose();
            wsClient = null;
        }

        Debug.Log("[WebSocketManager] WebSocket successfully disconnected and resources cleaned up.");
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>. Ensures the connection is terminated when the application closes.
    /// </summary>
    void OnApplicationQuit()
    {
        Disconnect();
    }

    // --- Serializable Data Structures ---

    [System.Serializable]
    public class BaseMessage
    {
        public string type;
    }

    [System.Serializable]
    public class HeartbeatMessage
    {
        public string type = "heartbeat";
    }

    [System.Serializable]
    public class KickoffMessage
    {
        public string type = "kickoff";
        public long userId;
    }
}