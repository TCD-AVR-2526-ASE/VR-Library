using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ServerRoomStore : IRoomStore
{
    /// <summary>
    /// Loads all rooms associated with the authenticated user from the backend.
    /// Returns an empty list if not logged in or if the request fails.
    /// </summary>
    /// <returns>A task resolving to the list of server-side rooms.</returns>
    public async Task<List<RoomData>> LoadAll()
    {
        // gatekeep loading if not logged in
        if (!AuthSession.IsLoggedIn)
            return new List<RoomData>();

        try
        {
            return await APIManager.Instance.Room.GetAllRooms();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RoomCache] Failed to load rooms from server: {e.Message}");
            return new List<RoomData>();
        }
    }

    /// <summary>
    /// Sends the current room list to the backend for upsert persistence.
    /// No operation occurs if the user is not authenticated.
    /// </summary>
    /// <param name="rooms">The rooms to synchronize with the server.</param>
    public async Task SaveAll(List<RoomData> rooms)
    {
        if (!AuthSession.IsLoggedIn)
            return;

        try
        {
            await APIManager.Instance.Room.AddRooms(rooms);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RoomCache] Failed to save rooms to server: {e.Message}");
        }
    }
}
