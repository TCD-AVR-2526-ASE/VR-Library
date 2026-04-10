using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class RoomRegistryTests
{
    //private FakeRoomStore _store;

    //[SetUp]
    //public void SetUp()
    //{
    //    _store = new FakeRoomStore();
    //}

    //[Test]
    //public void Ctor_LoadsRooms_MarksAllInactive_AndSaves()
    //{
    //    _store.Seed(new List<RoomData>
    //    {
    //        MakeRoom("r1", "Room 1", lastActiveUtc: NowUtc(), status: RoomStatus.Connected),
    //        MakeRoom("r2", "Room 2", lastActiveUtc: NowUtc(), status: RoomStatus.Connected),
    //    });

    //    var registry = new RoomRegistry(_store);

    //    var all = registry.GetAll().ToList();
    //    Assert.AreEqual(2, all.Count);

    //    // ���캯���� MarkAllInactive
    //    Assert.IsTrue(all.All(r => r.Status == RoomStatus.Dormant));

    //    // MarkAllInactive һ���� SaveAll һ�Σ���ʹ 0 ������Ҳ�ᣩ
    //    Assert.GreaterOrEqual(_store.SaveCalls, 1);
    //}

    //[Test]
    //public void Ctor_CleansUpExpiredRooms_OlderThan1Hour()
    //{
    //    long now = NowUtc();
    //    long old = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds(); // 2Сʱǰ -> Ӧ������

    //    _store.Seed(new List<RoomData>
    //    {
    //        MakeRoom("fresh", "Fresh", lastActiveUtc: now, status: RoomStatus.Connected),
    //        MakeRoom("expired", "Expired", lastActiveUtc: old, status: RoomStatus.Connected),
    //    });

    //    var registry = new RoomRegistry(_store);

    //    var all = registry.GetAll().ToList();
    //    Assert.AreEqual(1, all.Count);
    //    Assert.AreEqual("fresh", all[0].SessionID);

    //    // ���� SaveCalls ���� 1��MarkAllInactive��
    //    Assert.GreaterOrEqual(_store.SaveCalls, 1);
    //}

    //[Test]
    //public void Upsert_NewRoom_Adds_AndSaves()
    //{
    //    var registry = new RoomRegistry(_store);
    //    var baseSaves = _store.SaveCalls;

    //    var before = registry.GetAll().Count;

    //    var room = MakeRoom("r1", "Room 1", lastActiveUtc: 0, status: RoomStatus.Connected);
    //    registry.Upsert(room);

    //    var all = registry.GetAll().ToList();
    //    Assert.AreEqual(before + 1, all.Count);
    //    Assert.AreEqual("r1", all[0].SessionID);

    //    // Upsert �� Touch ���� LastUpdatedUtc
    //    Assert.Greater(all[0].LastUpdatedUtc, 0);

    //    // �� Upsert ��һ�� SaveAll
    //    Assert.AreEqual(baseSaves + 1, _store.SaveCalls);
    //}

    //[Test]
    //public void Upsert_SameRoomId_UpdatesFields_DoesNotDuplicate()
    //{
    //    var registry = new RoomRegistry(_store);
    //    var baseSaves = _store.SaveCalls;

    //    registry.Upsert(new RoomData
    //    {
    //        SessionID = "r1",
    //        RoomName = "Old",
    //        SceneName = "OldScene",
    //        MaxPlayers = 2,
    //        Status = RoomStatus.Connected,
    //        Endpoint = "old-endpoint",
    //        LastUpdatedUtc = 0
    //    });

    //    long beforeTouch = registry.GetAll().First().LastUpdatedUtc;

    //    // ͬ id ����
    //    registry.Upsert(new RoomData
    //    {
    //        SessionID = "r1",
    //        RoomName = "New",
    //        SceneName = "NewScene",
    //        MaxPlayers = 8,
    //        Status = RoomStatus.Dormant,
    //        Endpoint = "new-endpoint",
    //        LastUpdatedUtc = 0
    //    });

    //    var all = registry.GetAll().ToList();
    //    Assert.AreEqual(1, all.Count);

    //    var r = all[0];
    //    Assert.AreEqual("New", r.RoomName);
    //    Assert.AreEqual("NewScene", r.SceneName);
    //    Assert.AreEqual(8, r.MaxPlayers);
    //    Assert.AreEqual(RoomStatus.Dormant, r.Status);
    //    Assert.AreEqual("new-endpoint", r.Endpoint);

    //    // Touch ��ʱ��Ӧ�ø��£�>=��ͬһ���ڿ�����ȣ�
    //    Assert.GreaterOrEqual(r.LastUpdatedUtc, beforeTouch);

    //    // ���� Upsert => SaveAll +2���� ctor �����ϣ�
    //    Assert.AreEqual(baseSaves + 2, _store.SaveCalls);
    //}

    //[Test]
    //public void Remove_RemovesRoom_AndSaves()
    //{
    //    var registry = new RoomRegistry(_store);
    //    var baseSaves = _store.SaveCalls;

    //    registry.Upsert(MakeRoom("r1", "Room 1", lastActiveUtc: 0, status: RoomStatus.Connected));
    //    registry.Upsert(MakeRoom("r2", "Room 2", lastActiveUtc: 0, status: RoomStatus.Connected));
    //    Assert.AreEqual(1, registry.GetAll().Count);

    //    registry.Remove("r1");

    //    var all = registry.GetAll().ToList();
    //    Assert.AreEqual(1, all.Count);
    //    Assert.AreEqual("r2", all[0].SessionID);

    //    // 2��Upsert + 1��Remove => SaveAll +3���� ctor �����ϣ�
    //    Assert.AreEqual(baseSaves + 3, _store.SaveCalls);
    //}

    //// -------------------------
    //// helpers
    //// -------------------------

    //private static long NowUtc() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    //private static RoomData MakeRoom(string id, string name, long lastActiveUtc, RoomStatus status)
    //{
    //    return new RoomData
    //    {
    //        SessionID = id,
    //        RoomName = name,
    //        SceneName = "TestScene",
    //        MaxPlayers = 4,
    //        Status = status,
    //        LastUpdatedUtc = lastActiveUtc,
    //        Endpoint = "endpoint"
    //    };
    //}

    //private class FakeRoomStore : IRoomStore
    //{
    //    private List<RoomData> _data = new List<RoomData>();

    //    public int SaveCalls { get; private set; }

    //    public void Seed(List<RoomData> initial)
    //    {
    //        _data = initial ?? new List<RoomData>();
    //        SaveCalls = 0;
    //    }

    //    public List<RoomData> LoadAll()
    //    {
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
