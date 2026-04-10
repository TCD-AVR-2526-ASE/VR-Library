using UnityEngine;
using System;
using System.Threading.Tasks;

public class RoomCacheProvider : MonoBehaviour
{
    public static RoomCacheService CacheService { get; private set; }

    void Awake()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (CacheService != null)
        {
            Destroy(this);
            return;
        }

        AuthSession.LoggedIn += OnLoggedIn;
        AuthSession.LoggedOut += OnLoggedOut;

        if (AuthSession.IsLoggedIn)
        {
            await InitializeServer();
        }
        else
        {
            await InitializeLocalCache();
        }
    }


    /// <summary>
    /// Initializes the cache service using the local JSON-backed store.
    /// Retained for reference but not used in the server-backed configuration.
    /// </summary>
    /// <returns>A task that completes once the local cache is initialized.</returns>
    private async Task InitializeLocalCache()
    {
        CacheService = await RoomCacheService.CreateAsync(new RoomCache());
        Debug.Log("RoomCacheService initialized (local)");
    }

    /// <summary>
    /// Initializes the cache service using the server-backed store.
    /// Loads room metadata from the backend after authentication.
    /// </summary>
    /// <returns>A task that completes once the server cache is initialized.</returns>
    private async Task InitializeServer()
    {
        CacheService = await RoomCacheService.CreateAsync(new ServerRoomStore());
        Debug.Log("RoomCacheService initialized (server)");
    }

    /// <summary>
    /// Triggered when authentication succeeds.
    /// Initializes the server-backed cache service.
    /// </summary>
    /// <param name="tvo">The authenticated token information.</param>
    private void OnLoggedIn(TokenVo tvo)
    {
        _ = InitializeServer();
    }

    /// <summary>
    /// Clears the active cache service when the user logs out.
    /// Prevents further cache operations until re-authenticated.
    /// </summary>
    private void OnLoggedOut()
    {
        CacheService = null;
    }

    private void OnDestroy()
    {
        AuthSession.LoggedIn -= OnLoggedIn;
        AuthSession.LoggedOut -= OnLoggedOut;
    }
}