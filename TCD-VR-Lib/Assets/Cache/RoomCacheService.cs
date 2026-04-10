using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class RoomCacheService
{
    private readonly IRoomStore _store;
    private List<RoomData> _rooms;

    private RoomCacheService(IRoomStore store, List<RoomData> rooms)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _rooms = rooms ?? new List<RoomData>();
        MarkAllDormant();
        CleanupExpiredRooms();
        Debug.Log($"[RoomCacheService] Loaded rooms: {_rooms.Count}");
    }

    /// <summary>
    /// Creates and initializes a new <see cref="RoomCacheService"/> instance
    /// using the provided storage implementation.
    /// </summary>
    /// <param name="store">The storage backend used for persistence.</param>
    /// <returns>A fully initialized cache service.</returns>
    public static async Task<RoomCacheService> CreateAsync(IRoomStore store)
    {
        var rooms = await store.LoadAll();
        return new RoomCacheService(store, rooms);
    }

    private void Touch(RoomData room) =>
        room.LastUpdatedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    /// Inserts a new room or updates an existing room by GUID.
    /// Persists the updated room list to the configured store.
    /// </summary>
    /// <param name="room">The room to insert or update.</param>
    /// <returns>The inserted or updated <see cref="RoomData"/>.</returns>
    public async Task<RoomData> Upsert(RoomData room)
    {
        var existingRoom = _rooms.FirstOrDefault(r => r.GUID == room.GUID);
        if (existingRoom != null)
        {
            // Update Room Info
            existingRoom.RoomName = room.RoomName;
            existingRoom.SceneName = room.SceneName;
            existingRoom.MaxPlayers = room.MaxPlayers;
            existingRoom.StatusEnum = room.StatusEnum;
            existingRoom.Endpoint = room.Endpoint;

            // session-related fields
            existingRoom.SessionID = room.SessionID;
            existingRoom.JoinCode = room.JoinCode;

            Touch(existingRoom);
            await _store.SaveAll(_rooms);
            return existingRoom;
        }
        // Insert new room
        Touch(room);
        _rooms.Add(room);
        await _store.SaveAll(_rooms);
        return room;
    }

    /// <summary>
    /// Updates the status of a room identified by GUID
    /// and persists the change to the store.
    /// </summary>
    /// <param name="GUID">The unique identifier of the room.</param>
    /// <param name="status">The new status to apply.</param>
    public async Task SetStatus(string GUID, RoomStatus status)
    {
        var room = _rooms.FirstOrDefault(x => x.GUID == GUID);
        if (room == null) return;

        room.StatusEnum = status;
        Touch(room);
        await _store.SaveAll(_rooms);
    }

    /// <summary>
    /// Marks all cached rooms as Dormant during initialization.
    /// Ensures no room is considered active after application restart.
    /// </summary>
    private void MarkAllDormant()
    {
        foreach (var room in _rooms)
            room.StatusEnum = RoomStatus.Dormant;

        _ = _store.SaveAll(_rooms); // fire-and-forget

        Debug.Log($"[RoomCacheService] Marked {_rooms.Count} rooms dormant.");
    }

    /// <summary>
    /// Removes a room from the cache by GUID
    /// and persists the updated room list.
    /// </summary>
    /// <param name="GUID">The unique identifier of the room to remove.</param>
    public async Task Remove(string GUID)
    {
        _rooms.RemoveAll(r => r.GUID == GUID);
        await _store.SaveAll(_rooms);
    }

    /// <summary>
    /// Removes rooms that have not been updated within the expiration window
    /// to prevent stale cache entries.
    /// </summary>
    private void CleanupExpiredRooms()
    {
        Debug.Log($"[RoomCacheService] All rooms not accessed in last 10 minutes will be removed");
        long cutoff = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds();
        int before = _rooms.Count;

        _rooms.RemoveAll(r => r.LastUpdatedUtc < cutoff);

        int removed = before - _rooms.Count;
        if (removed > 0)
        {
            Debug.Log($"[RoomCacheService] Cleaned up {removed} expired rooms.");
            _ = _store.SaveAll(_rooms); // fire-and-forget
        }
    }

    /// <summary>
    /// Retrieves a read-only view of all cached rooms.
    /// </summary>
    /// <returns>A read-only collection of <see cref="RoomData"/>.</returns>
    public IReadOnlyList<RoomData> GetAll() => _rooms.AsReadOnly();

}