using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

public class RoomRegistryProviderPlayModeTests
{
    //[SetUp]
    //public void SetUp() => ResetRegistryStatic();

    //[TearDown]
    //public void TearDown() => ResetRegistryStatic();

    //[UnityTest]
    //public IEnumerator Provider_Awake_InitializesRegistry()
    //{
    //    var go = new GameObject("RoomRegistryProvider_Test");
    //    go.AddComponent<RoomRegistryProvider>();

    //    yield return null;

    //    Assert.NotNull(RoomRegistryProvider.Registry);
    //    Assert.NotNull(RoomRegistryProvider.Registry.GetAll());

    //    Object.Destroy(go);
    //    yield return null;
    //}

    //[UnityTest]
    //public IEnumerator Provider_SecondInstance_DestroysItself()
    //{
    //    var go1 = new GameObject("Provider1");
    //    go1.AddComponent<RoomRegistryProvider>();
    //    yield return null;

    //    var go2 = new GameObject("Provider2");
    //    var p2 = go2.AddComponent<RoomRegistryProvider>();
    //    yield return null;

    //    // �ڶ��� Awake �� Destroy(this)
    //    Assert.IsTrue(p2 == null || go2.GetComponent<RoomRegistryProvider>() == null);

    //    Object.Destroy(go1);
    //    Object.Destroy(go2);
    //    yield return null;
    //}

    //private static void ResetRegistryStatic()
    //{
    //    var field = typeof(RoomRegistryProvider).GetField("<Registry>k__BackingField",
    //        BindingFlags.Static | BindingFlags.NonPublic);
    //    field?.SetValue(null, null);
    //}
}
