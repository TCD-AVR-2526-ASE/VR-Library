using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Clean-slate network spawn entry point for books.
/// This file is intentionally minimal so the networking flow can be rebuilt cleanly.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class NetworkBookManager : NetworkBehaviour
{
    // Scene-level references injected by BookSystem on startup.
    private BookSystem bookSystem;
    private GameObject bookPrefab;

    public void Configure(BookSystem system, GameObject prefab)
    {
        bookSystem = system;
        bookPrefab = prefab;
    }

    public bool CanNetworkBooks()
    {
        return NetworkManager.Singleton != null &&
               NetworkManager.Singleton.IsConnectedClient &&
               bookPrefab != null &&
               bookPrefab.GetComponent<NetworkObject>() != null;
    }

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
