using UnityEngine;
using NUnit.Framework;

public class BookSystemLoaderTesting
{
    private BookSystem system;
    private BookPaginator paginator;
    private BookController controller;
    private BookRenderer renderer;

    [SetUp]
    void Init()
    {
        var obj = new GameObject();
        system = obj.AddComponent<BookSystem>();
        paginator = obj.AddComponent<BookPaginator>();
        controller = obj.AddComponent<BookController>();
        renderer = obj.AddComponent<BookRenderer>();
    }

    [Test]
    // Test the connection to localhost by pinging connection.
    // if ping doesn't return within timeout, test fails.
    private void ConnectToLocalhost()
    {

    }

    [Test]
    // Test the connection to Gutenberg.
    // Important to NOT MANUALLY RUN the backend!!!
    // [will probably require an extra function in the py backend]
    private void ConnectToGutenberg()
    {

    }

    [Test]
    // Test asynchronicity of the request; Try performing a basic operation
    // while the program is to wait for the response to come in.
    // fails if the operation doesn't perform.
    private void DoSomethingWhileAwaitingRequest()
    {

    }

    [Test]
    // Test timing out on bad requests.
    // Intentionally test a bad input and make the request time out within x minutes.
    // Fails if the function runs longer than timeout after request sending.
    private void TimeoutOnBadRequest()
    {

    }

    [Test]
    // Test the success of a genuine online request. 
    // Succeeds if the file exists in the Assets/Resources/BookFiles folder.
    private void TestRealRequest()
    {

    }
}
