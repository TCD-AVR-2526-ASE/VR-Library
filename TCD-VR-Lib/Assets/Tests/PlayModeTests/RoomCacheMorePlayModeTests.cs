using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class RoomCacheMorePlayModeTests
{
    //private RoomCache _cache;
    //private string _cachePath;

    //[SetUp]
    //public void SetUp()
    //{
    //    var filename = $"cache_tests/{Guid.NewGuid():N}/rooms_cache.json";
    //    _cache = new RoomCache(filename);
    //    _cachePath = GetPrivatePath(_cache);

    //    if (File.Exists(_cachePath))
    //        File.Delete(_cachePath);
    //}

    //[TearDown]
    //public void TearDown()
    //{
    //    if (!string.IsNullOrEmpty(_cachePath) && File.Exists(_cachePath))
    //        File.Delete(_cachePath);

    //    var dir = Path.GetDirectoryName(_cachePath);
    //    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
    //    {
    //        try
    //        {
    //            Directory.Delete(dir, recursive: true);
    //        }
    //        catch { /* ignore */ }
    //    }
    //}

    //[Test]
    //public void SaveAll_CreatesFile()
    //{
    //    _cache.SaveAll(new List<RoomData>());
    //    Assert.IsTrue(File.Exists(_cachePath));
    //}

    //[Test]
    //public void SaveAll_CreatesDirectory_WhenUsingSubfolderFilename()
    //{
    //    var dir = Path.GetDirectoryName(_cachePath);
    //    Assert.IsNotNull(dir);

    //    _cache.SaveAll(new List<RoomData>());
    //    Assert.IsTrue(Directory.Exists(dir));
    //}

    //[Test]
    //public void SaveAll_WritesNonEmptyJson()
    //{
    //    _cache.SaveAll(new List<RoomData>());
    //    var json = File.ReadAllText(_cachePath);
    //    Assert.IsFalse(string.IsNullOrWhiteSpace(json));
    //    Assert.IsTrue(json.Contains("Rooms"));
    //}

    //[Test]
    //public void SaveAll_OverwritesExistingFile()
    //{
    //    _cache.SaveAll(new List<RoomData> { MakeRoom("a") });
    //    var first = _cache.LoadAll();
    //    Assert.AreEqual(1, first.Count);
    //    Assert.AreEqual("a", first[0].SessionID);

    //    _cache.SaveAll(new List<RoomData> { MakeRoom("b") });
    //    var second = _cache.LoadAll();
    //    Assert.AreEqual(1, second.Count);
    //    Assert.AreEqual("b", second[0].SessionID);
    //}

    //[Test]
    //public void LoadAll_WhenFileWhitespace_ReturnsEmptyList()
    //{
    //    Directory.CreateDirectory(Path.GetDirectoryName(_cachePath)!);
    //    File.WriteAllText(_cachePath, " \n\t  ");
    //    var rooms = _cache.LoadAll();
    //    Assert.NotNull(rooms);
    //    Assert.AreEqual(0, rooms.Count);
    //}

    //[Test]
    //public void LoadAll_WhenJsonIsEmptyObject_ReturnsEmptyList()
    //{
    //    Directory.CreateDirectory(Path.GetDirectoryName(_cachePath)!);
    //    File.WriteAllText(_cachePath, "{}");
    //    var rooms = _cache.LoadAll();
    //    Assert.NotNull(rooms);
    //    Assert.AreEqual(0, rooms.Count);
    //}

    //[Test]
    //public void LoadAll_WhenJsonHasEmptyRoomsArray_ReturnsEmptyList()
    //{
    //    Directory.CreateDirectory(Path.GetDirectoryName(_cachePath)!);
    //    File.WriteAllText(_cachePath, "{ \"Rooms\": [] }");
    //    var rooms = _cache.LoadAll();
    //    Assert.NotNull(rooms);
    //    Assert.AreEqual(0, rooms.Count);
    //}

    //private static string GetPrivatePath(RoomCache cache)
    //{
    //    var field = typeof(RoomCache).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic);
    //    Assert.NotNull(field, "RoomCache private field '_path' not found. Implementation changed?");
    //    var path = field.GetValue(cache) as string;
    //    Assert.IsFalse(string.IsNullOrEmpty(path), "RoomCache _path is null/empty.");
    //    return path!;
    //}

    //private static RoomData MakeRoom(string id)
    //{
    //    return new RoomData
    //    {
    //        SessionID = id,
    //        RoomName = "Test",
    //        SceneName = "TestScene",
    //        MaxPlayers = 4,
    //        Status = RoomStatus.Connected,
    //        LastUpdatedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    //        Endpoint = "endpoint"
    //    };
    //}
}
