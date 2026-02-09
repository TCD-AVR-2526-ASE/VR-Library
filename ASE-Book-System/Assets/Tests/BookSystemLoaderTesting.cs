using UnityEngine;
using NUnit.Framework;

public class BookSystemLoaderTesting
{
    [SerializeField]
    private BookSystem system;
    private BookPaginator paginator;
    private BookController controller;
    private BookRenderer renderer;

    [Test]
    void Test()
    {
        Debug.Log("Test.");
    }
}
