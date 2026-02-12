using UnityEngine;
using NUnit.Framework;
using System.Diagnostics;
using UnityEngine.Networking;

public class BookSystemLoaderTesting
{
    private BookSystem system;
    private BookPaginator paginator;
    private BookController controller;
    private BookRenderer renderer;
    private BookRepositry repo;

    float frameBufferTolerance = 0.035f;
    int timeout = 10;

    [SetUp]
    public void SetUp()
    {
        var obj = new GameObject();
        system = obj.AddComponent<BookSystem>();
        paginator = obj.AddComponent<BookPaginator>();
        controller = obj.AddComponent<BookController>();
        renderer = obj.AddComponent<BookRenderer>();
        repo = obj.GetComponent<BookRepositry>();
    }

    [Test]
    public void InitFlaskServer()
    {
        repo.InitFlask();
        Assert.IsTrue(!repo.server.HasExited);
    }


    [Test]
    // Test the connection to localhost by pinging connection.
    // if ping doesn't return within timeout, test fails.
    public void ConnectToLocalhost()
    {
        Assert.IsTrue(repo.PingLocalhost());
    }

    

    [Test]
    // Test asynchronicity of the request; Try performing a basic operation
    // while the program is to wait for the response to come in.
    // fails if the operation doesn't perform.
    public void DoSomethingWhileAwaitingRequest()
    {
        Assert.IsTrue(repo.PingLocalhost());
            
        string bogusTitle = "dwiuabdaisndpoiandosadqa";
        int a = 1;
        repo.RequestBook(bogusTitle, true);
        a += 1;
        Assert.AreEqual(a, 2);
    }

    [Test]
    // Test timing out on bad requests.
    // Intentionally test a bad input and make the request time out within x minutes.
    // Fails if the function runs longer than timeout after request sending.
    public void TimeoutOnBadRequest()
    {
        Assert.IsTrue(repo.PingLocalhost());

        string bogusTitle = "aidnaindsiajbfijdynfpsad";
        UnityWebRequestAsyncOperation request;
        var timer = new Stopwatch();
        request = repo.CreateBookWebRequest(bogusTitle, timeout);
        timer.Start();
        while (!request.isDone)
        {
            // do nothing
        }
        request.webRequest.Dispose();
        timer.Stop();

        Assert.LessOrEqual(timer.Elapsed.Seconds, (float) timeout + frameBufferTolerance);
    }

    [Test]
    // Test the success of a genuine online request. 
    // Succeeds if the file exists in the Assets/Resources/BookFiles folder.
    public void TestRealRequest()
    {
        string title = "1984";
        UnityWebRequestAsyncOperation request;
        var timer = new Stopwatch();
        request = repo.CreateBookWebRequest(title, timeout);
        timer.Start();
        while (!request.isDone)
        {
            // do nothing
        }
        timer.Stop();

        Assert.Less(timer.Elapsed.Seconds, (float) timeout);
        Assert.IsTrue(request.webRequest.result == UnityWebRequest.Result.Success);

        request.webRequest.Dispose();
    }
}
