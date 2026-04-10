using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class RoomRegistryMorePlayModeTests
{
    //[Test]
    //public void Ctor_NullStore_Throws()
    //{
    //    Assert.Throws<ArgumentNullException>(() => new RoomRegistry(null));
    //}

    //[Test]
    //public void Ctor_EmptyStore_SavesOnceBecauseMarkAllInactiveAlwaysSaves()
    //{
    //    var store = new FakeRoomStore(new List<RoomData>());
    //    var registry = new RoomRegistry(store);

    //    Assert.AreEqual(0, registry.GetAll().Count);
    //    Assert.AreEqual(1, store.SaveCalls);
    //}

    //[Test]
    //public void Ctor_WithFreshRooms_DoesNotRemoveAny()
    //{
    //    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    //    var store = new FakeRoomStore(new List<RoomData>
    //    {
    //        MakeRoom("r1", now),
    //        MakeRoom("r2", now),
    //    });

    //    var registry = new RoomRegistry(store);
    //    Assert.AreEqual(2, registry.GetAll().Count);
    //}

    //[Test]
    //public void Ctor_WithMultipleExpiredRooms_RemovesAllExpired()
    //{
    //    var old = DateTimeOffset.UtcNow.AddHours(-3).ToUnixTimeSeconds();
    //    var store = new FakeRoomStore(new List<RoomData>
    //    {
    //        MakeRoom("e1", old),
    //        MakeRoom("e2", old),
    //        MakeRoom("e3", old),
    //    });

    //    var registry = new RoomRegistry(store);
    //    Assert.AreEqual(0, registry.GetAll().Count);
    //}

    //[Test]
    //public void Upsert_MultipleTimesSameId_DoesNotDuplicate()
    //{
    //    var store = new FakeRoomStore(new List<RoomData>());
    //    var registry = new RoomRegistry(store);

    //    for (int i = 0; i < 50; i++)
    //    {
    //        registry.Upsert(new RoomData
    //        {
    //            SessionID = "same",
    //            RoomName = "N" + i,
    //            SceneName = "S",
    //            MaxPlayers = 4,
    //            Status = RoomStatus.Connected,
    //            LastUpdatedUtc = 0,
    //            Endpoint = "E" + i
    //        });
    //    }

    //    var all = registry.GetAll().ToList();
    //    Assert.AreEqual(1, all.Count);
    //    Assert.AreEqual("same", all[0].SessionID);
    //    Assert.AreEqual("N49", all[0].RoomName);
    //}

    //[Test]
    //public void Remove_WhenRoomNotFound_StillSaves()
    //{
    //    var store = new FakeRoomStore(new List<RoomData>());
    //    var registry = new RoomRegistry(store);
    //    var baseSaves = store.SaveCalls; // ctor �Ѿ� SaveAll һ��

    //    registry.Remove("not-exist");

    //    Assert.AreEqual(baseSaves + 1, store.SaveCalls); // Remove ���� SaveAll
    //}

    //[Test]
    //public void SetStatus_WhenRoomNotFound_DoesNotSave()
    //{
    //    var store = new FakeRoomStore(new List<RoomData>());
    //    var registry = new RoomRegistry(store);
    //    var baseSaves = store.SaveCalls;

    //    registry.SetStatus("not-exist", RoomStatus.Connected);

    //    Assert.AreEqual(baseSaves, store.SaveCalls); // not found: return���� Save
    //}

    //[Test]
    //public void GetAll_IsReadOnlyCollection_CannotAdd()
    //{
    //    var store = new FakeRoomStore(new List<RoomData>());
    //    var registry = new RoomRegistry(store);

    //    var list = (System.Collections.Generic.IList<RoomData>)registry.GetAll();
    //    Assert.Throws<NotSupportedException>(() => list.Add(MakeRoom("x", DateTimeOffset.UtcNow.ToUnixTimeSeconds())));
    //}

    //[Test]
    //public void Remove_RemovesAllMatchingIds_IfDuplicatesExistInStore()
    //{
    //    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    //    var store = new FakeRoomStore(new List<RoomData>
    //    {
    //        MakeRoom("dup", now),
    //        MakeRoom("dup", now),
    //        MakeRoom("keep", now)
    //    });

    //    var registry = new RoomRegistry(store);

    //    registry.Remove("dup");
    //    registry.Remove("dup");

    //    var all = registry.GetAll().ToList();
    //    Assert.AreEqual(3, all.Count);
    //    Assert.AreEqual("keep", all[0].SessionID);
    //}

    //private static RoomData MakeRoom(string id, long lastActiveUtc)
    //{
    //    return new RoomData
    //    {
    //        SessionID = id,
    //        RoomName = "Test",
    //        SceneName = "TestScene",
    //        MaxPlayers = 4,
    //        Status = RoomStatus.Connected,
    //        LastUpdatedUtc = lastActiveUtc,
    //        Endpoint = "endpoint"
    //    };
    //}

    //private class FakeRoomStore : IRoomStore
    //{
    //    private List<RoomData> _data;
    //    public int SaveCalls { get; private set; }

    //    public FakeRoomStore(List<RoomData> initial)
    //    {
    //        _data = initial ?? new List<RoomData>();
    //    }

    //    public List<RoomData> LoadAll()
    //    {
    //        // copy
    //        return _data.Select(Clone).ToList();
    //    }

    //    public void SaveAll(List<RoomData> rooms)
    //    {
    //        SaveCalls++;
    //        _data = (rooms ?? new List<RoomData>()).Select(Clone).ToList();
    //    }

    //    private static RoomData Clone(RoomData r)
    //    {
    //        return new RoomData
    //        {
    //            SessionID = r.SessionID,
    //            RoomName = r.RoomName,
    //            SceneName = r.SceneName,
    //            MaxPlayers = r.MaxPlayers,
    //            Status = r.Status,
    //            LastUpdatedUtc = r.LastUpdatedUtc,
    //            Endpoint = r.Endpoint
    //        };
    //    }
    //}
}
