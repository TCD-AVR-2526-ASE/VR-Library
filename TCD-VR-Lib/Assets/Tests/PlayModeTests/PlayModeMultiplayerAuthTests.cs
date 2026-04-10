using NUnit.Framework;
using UnityEngine;

public class PlayModeMultiplayerAuthTests
{
    [SetUp]
    public void SetUp()
    {
        // Session creation?
    }

    [Test]
    public void CompareUsernames()
    {
        // log in the sessions -> need respective AuthenticationService.Instance returned!
        // try getting the other session's player's username
        // compare user names

        // session 1 username
        string username1 = "josh";
        // session 2 username
        string username2 = "gongxu";

        // session 1 query for session 2 un
        string result1 = "";
        // session 2 query for session 1 un
        string result2 = "";

        Assert.IsTrue(username1 == result2);
        Assert.IsTrue(username2 == result1);
    }
}
