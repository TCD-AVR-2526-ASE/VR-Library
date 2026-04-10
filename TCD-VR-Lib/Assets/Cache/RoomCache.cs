using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class RoomCache : IRoomStore
{
    [System.Serializable]
    private class Wrapper { public List<RoomData> Rooms = new(); }

    private readonly string _path;

    /// <summary>
    /// Initializes a file-backed room cache using Unity's persistent data path.
    /// Creates a full file path for storing serialized room metadata locally.
    /// </summary>
    /// <param name="filename">The JSON file name used to store cached room data.</param>
    public RoomCache(string filename = "rooms_cache.json")
    {
        var basePath = Application.persistentDataPath;
        Debug.Log($"RoomCache base path (persistent): {basePath}");
        if (string.IsNullOrEmpty(basePath))
        {
            basePath = Application.dataPath;
            Debug.Log($"RoomCache base path (if prev failed): {basePath}");
        }
        _path = Path.Combine(basePath, filename);
        Debug.Log($"RoomCache full path: {_path}");

    }

    /// <summary>
    /// Loads all cached rooms from the local JSON file.
    /// Returns an empty list if the file does not exist or contains invalid data.
    /// </summary>
    /// <returns>A task resolving to the list of cached <see cref="RoomData"/>.</returns>
    public Task<List<RoomData>> LoadAll()
    {
        // No cache file exists
        if (!File.Exists(_path)) return Task.FromResult(new List<RoomData>());

        var json = File.ReadAllText(_path); // Read the entire file content
        // Handle empty or invalid JSON
        if (string.IsNullOrWhiteSpace(json)) return Task.FromResult(new List<RoomData>());

        // Deserialize the JSON into the Wrapper class
        var wrapper = JsonUtility.FromJson<Wrapper>(json);
        return Task.FromResult(wrapper?.Rooms ?? new List<RoomData>());
    }

    /// <summary>
    /// Serializes and writes all room data to disk.
    /// Overwrites the existing cache file with the provided room list.
    /// </summary>
    /// <param name="rooms">The collection of rooms to persist locally.</param>
    /// <returns>A completed task once the file write operation finishes.</returns>
    public Task SaveAll(List<RoomData> rooms)
    {
        // Wrap the list of rooms in the Wrapper class for serialization
        var wrapper = new Wrapper { Rooms = rooms ?? new List<RoomData>() };
        var json = JsonUtility.ToJson(wrapper, prettyPrint: true);

        // Ensure the directory exists before writing
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        // Write the JSON content to the cache file
        File.WriteAllText(_path, json);
        return Task.CompletedTask;
    }
}
