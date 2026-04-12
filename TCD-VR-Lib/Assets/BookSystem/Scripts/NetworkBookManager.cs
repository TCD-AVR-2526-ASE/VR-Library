using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Coordinates authoritative spawning of shared books across the network.
/// Accepts client requests, reserves a table on the server, and spawns the networked book prefab.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class NetworkBookManager : NetworkBehaviour
{
    // Scene-level references injected by BookSystem on startup.
    private BookSystem bookSystem;
    private GameObject bookPrefab;

    /// <summary>
    /// Injects the scene-level references required to spawn and position shared books.
    /// </summary>
    /// <param name="system">The active <see cref="BookSystem"/> in the scene.</param>
    /// <param name="prefab">The network-enabled book prefab to spawn.</param>
    public void Configure(BookSystem system, GameObject prefab)
    {
        bookSystem = system;
        bookPrefab = prefab;
    }

    /// <summary>
    /// Returns whether networked shared-book spawning is currently available.
    /// </summary>
    /// <returns><c>true</c> when the client is connected and the prefab is network-ready.</returns>
    public bool CanNetworkBooks()
    {
        return NetworkManager.Singleton != null &&
               NetworkManager.Singleton.IsConnectedClient &&
               bookPrefab != null &&
               bookPrefab.GetComponent<NetworkObject>() != null;
    }

    /// <summary>
    /// Requests a shared book by title using the authoritative server-side table allocation path.
    /// </summary>
    /// <param name="title">The requested book title.</param>
    public void RequestSharedBook(string title)
    {
        if (!CanNetworkBooks())
            return;

        string requestedTitle = title != null ? title.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(requestedTitle))
            return;

        if (IsServer)
        {
            HandleSharedBookRequest(requestedTitle);
            return;
        }

        RequestSharedBookServerRpc(new FixedString512Bytes(requestedTitle));
    }

    [Rpc(SendTo.Server)]
    private void RequestSharedBookServerRpc(FixedString512Bytes title)
    {
        HandleSharedBookRequest(title.ToString());
    }

    /// <summary>
    /// Spawns a shared book at an explicit table index.
    /// Intended for code paths that have already resolved a spawn slot.
    /// </summary>
    /// <param name="title">The book title to replicate.</param>
    /// <param name="tableIndex">The target table index, or <c>-1</c> for fallback placement.</param>
    /// <param name="preloadedBook">An optional preloaded book id to seed onto the spawned object.</param>
    public void SpawnSharedBook(string title, int tableIndex, int preloadedBook = -1)
    {
        if (!CanNetworkBooks())
            return;

        // In distributed authority, non-owners ask the authoritative peer to spawn.
        if (IsServer)
            SpawnBookOnServer(title, tableIndex, preloadedBook);
        else
            RequestSpawnServerRpc(new FixedString512Bytes(title ?? string.Empty), tableIndex, preloadedBook);
    }

    private void HandleSharedBookRequest(string title)
    {
        if (bookSystem == null)
        {
            Debug.LogWarning("[NetworkBookManager] Missing BookSystem during shared request.");
            return;
        }

        int tableIndex = -1;
        if (!bookSystem.TryReserveNextAvailableTable(out tableIndex))
            Debug.LogWarning($"[NetworkBookManager] No free table found for shared book '{title}'. Using fallback spawn.");

        SpawnBookOnServer(title, tableIndex, -1);
    }

    [Rpc(SendTo.Server)]
    private void RequestSpawnServerRpc(FixedString512Bytes title, int tableIndex, int preloadedBook)
    {
        SpawnBookOnServer(title.ToString(), tableIndex, preloadedBook);
    }

    private void SpawnBookOnServer(string title, int tableIndex, int preloadedBook)
    {
        if (bookSystem == null || bookPrefab == null)
        {
            Debug.LogWarning("[NetworkBookManager] Missing BookSystem or book prefab during spawn.");
            return;
        }

        bookSystem.ResolveSpawnPose(tableIndex, out Vector3 spawnPos, out Quaternion spawnRot);
        Debug.Log($"[NetworkBookManager] Spawning shared book '{title}' (id={preloadedBook}) at table {tableIndex}.");

        GameObject bookObj = Instantiate(bookPrefab, spawnPos, spawnRot);
        bookObj.transform.localScale = Vector3.one * bookSystem.BookScale;
        bookSystem.PrepareBookObjectForNetworking(bookObj);

        // Seed the spawned book with the identity every client will resolve locally.
        var networkState = bookObj.GetComponent<NetworkBookState>();
        if (networkState != null)
        {
            networkState.SetInitialState(title, preloadedBook, 0, false, tableIndex);
        }
        else
        {
            Debug.LogWarning("[NetworkBookManager] Spawned book is missing NetworkBookState.");
        }

        var networkObject = bookObj.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("[NetworkBookManager] Book prefab is missing NetworkObject.");
            Destroy(bookObj);
            return;
        }

        networkObject.Spawn();
        Debug.Log($"[NetworkBookManager] Spawned NetworkObjectId={networkObject.NetworkObjectId} for '{title}'.");

        if (bookSystem != null)
            bookSystem.PulseTableHighlightForIndex(tableIndex);
    }
}
