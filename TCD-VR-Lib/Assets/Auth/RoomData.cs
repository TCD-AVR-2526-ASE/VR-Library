using System;

[Serializable]
public class RoomData
{
    public static object RoomStatus;
    public string GUID; // Unique identifier for the room (used as primary key)
    public string SessionID; // Unity Multiplayer session ID (currently same as GUID)
    public string JoinCode; // Join code used for reconnecting to session
    public string RoomName; // Display name of the room
    public string SceneName; // will be removed
    public int MaxPlayers;  // Maximum allowed players in this session
    public int Status;      // Stored as int for serialization; maps to RoomStatus enum

    public long LastUpdatedUtc;      // will be removed

    public string Endpoint;         // => needs to be stored

    // Convenience property for working with enum safely in gameplay code
    public RoomStatus StatusEnum
    {
        get => (RoomStatus)Status;
        set => Status = (int)value;
    }
}

public enum RoomStatus
{
    Connected, // Room is actively connected to a live session
    Dormant,    // Room exists in cache but not currently connected
    Closed     // Room has been closed and should not be reconnected    
}
