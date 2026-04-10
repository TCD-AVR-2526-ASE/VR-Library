using NUnit.Framework;
using UnityEngine;
using Unity.Netcode;
using XRMultiplayer;

public class EditModeNetworkTests
{
    // ---------- XRINetworkGameManager ----------

    [Test] // 1
    public void CanAdd_XRINetworkGameManager()
    {
        var go = new GameObject();
        var mgr = go.AddComponent<XRINetworkGameManager>();
        Assert.IsNotNull(mgr);
    }

    [Test] // 2
    public void XRINetworkGameManager_IsMonoBehaviour()
    {
        Assert.IsTrue(typeof(XRINetworkGameManager).IsSubclassOf(typeof(MonoBehaviour)));
    }

    [Test] // 3
    public void XRINetworkGameManager_EnabledByDefault()
    {
        var mgr = new GameObject().AddComponent<XRINetworkGameManager>();
        Assert.IsTrue(mgr.enabled);
    }

    [Test] // 4
    public void XRINetworkGameManager_CanBeDisabled()
    {
        var mgr = new GameObject().AddComponent<XRINetworkGameManager>();
        mgr.enabled = false;
        Assert.IsFalse(mgr.enabled);
    }

    [Test] // 5
    public void XRINetworkGameManager_CanBeDestroyed()
    {
        var go = new GameObject();
        var mgr = go.AddComponent<XRINetworkGameManager>();
        Object.DestroyImmediate(go);
        Assert.IsTrue(mgr == null);
    }

    // ---------- Netcode NetworkManager ----------

    [Test] // 6
    public void CanAdd_NetworkManager()
    {
        var nm = new GameObject().AddComponent<NetworkManager>();
        Assert.IsNotNull(nm);
    }

    [Test] // 7
    public void NetworkManager_NotListening_ByDefault()
    {
        var nm = new GameObject().AddComponent<NetworkManager>();
        Assert.IsFalse(nm.IsListening);
    }

    [Test]
    public void NetworkManager_EnabledByDefault()
    {
        var nm = new GameObject().AddComponent<NetworkManager>();
        Assert.IsTrue(nm.enabled);
    }

    [Test]
    public void NetworkManager_CanBeDisabled()
    {
        var nm = new GameObject().AddComponent<NetworkManager>();
        nm.enabled = false;
        Assert.IsFalse(nm.enabled);
    }


    [Test] // 8
    public void NetworkManager_HasNetworkConfig()
    {
        var nm = new GameObject().AddComponent<NetworkManager>();
        Assert.IsNotNull(nm.NetworkConfig);
    }

    [Test] // 9
    public void NetworkManager_DefaultPlayerPrefab_IsNull()
    {
        var nm = new GameObject().AddComponent<NetworkManager>();
        Assert.IsNotNull(nm.NetworkConfig);
        Assert.IsNull(nm.NetworkConfig.PlayerPrefab);
    }

    [Test] // 10
    public void NetworkManager_CanBeDestroyed()
    {
        var go = new GameObject();
        var nm = go.AddComponent<NetworkManager>();
        Object.DestroyImmediate(go);
        Assert.IsTrue(nm == null);
    }

    // ---------- Session / Auth Managers ----------

    [Test] // 11
    public void CanAdd_SessionManager()
    {
        var mgr = new GameObject().AddComponent<SessionManager>();
        Assert.IsNotNull(mgr);
    }

    [Test] // 12
    public void CanAdd_AuthenticationManager()
    {
        var mgr = new GameObject().AddComponent<AuthenticationManager>();
        Assert.IsNotNull(mgr);
    }

    [Test] // 13
    public void SessionManager_IsMonoBehaviour()
    {
        Assert.IsTrue(typeof(SessionManager).IsSubclassOf(typeof(MonoBehaviour)));
    }

    [Test] // 14
    public void AuthenticationManager_IsMonoBehaviour()
    {
        Assert.IsTrue(typeof(AuthenticationManager).IsSubclassOf(typeof(MonoBehaviour)));
    }

    // ---------- NetworkObject / Netcode primitives ----------

    [Test] // 15
    public void CanAdd_NetworkObject()
    {
        var netObj = new GameObject().AddComponent<NetworkObject>();
        Assert.IsNotNull(netObj);
    }

    [Test] // 16
    public void NetworkObject_IsNotSpawned_ByDefault()
    {
        var netObj = new GameObject().AddComponent<NetworkObject>();
        Assert.IsFalse(netObj.IsSpawned);
    }

    [Test] // 17
    public void NetworkObject_OwnerId_DefaultsToZero()
    {
        var netObj = new GameObject().AddComponent<NetworkObject>();
        Assert.AreEqual(0ul, netObj.OwnerClientId);
    }

    // ---------- General networking safety ----------

    [Test] // 18
    public void NetworkManager_Singleton_IsNull_WithoutScene()
    {
        Assert.IsNull(NetworkManager.Singleton);
    }

    [Test] // 19
    public void Multiple_NetworkManagers_CanExist_InEditMode()
    {
        new GameObject().AddComponent<NetworkManager>();
        new GameObject().AddComponent<NetworkManager>();
        Assert.GreaterOrEqual(Object.FindObjectsByType<NetworkManager>(FindObjectsSortMode.None).Length, 2);
    }

    [Test] // 20
    public void Creating_NetworkComponents_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            new GameObject().AddComponent<XRINetworkGameManager>();
            new GameObject().AddComponent<NetworkManager>();
            new GameObject().AddComponent<SessionManager>();
            new GameObject().AddComponent<AuthenticationManager>();
        });
    }
}
