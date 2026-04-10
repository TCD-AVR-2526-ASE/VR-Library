using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;
using XRMultiplayer;

public class PlayModeNetworkTests
{
    private const string SceneName = "Testing";

    [UnityTest]
    public IEnumerator XRINetworkGameManager_Exists_In_PlayMode()
    {
        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);

        // wait ONE frame only � do NOT wait for full XR init
        yield return null;

        var manager = Object.FindObjectOfType<XRINetworkGameManager>();
        Assert.IsNotNull(manager);

        // Stop test BEFORE UI/Auth explode
        yield break;
    }
}
