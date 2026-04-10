using UnityEngine;
using NUnit.Framework;
using System.IO;

public class BookSystemLoaderTesting
{
    [Test]
    public void BookFilesDirectoryExists()
    {
        string path = "./Assets/Resources/BookFiles";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        Assert.IsTrue(Directory.Exists(path));
    }

    [Test]
    public void BookPrefabExists()
    {
        var prefab = Resources.Load<GameObject>("BookSystem/Prefabs/Book");
        Assert.IsNotNull(prefab, "Book prefab should exist at Resources/BookSystem/Prefabs/Book");
    }

    [Test]
    public void BookManagerPrefabExists()
    {
        var prefab = Resources.Load<GameObject>("BookSystem/Prefabs/BookManager");
        Assert.IsNotNull(prefab, "BookManager prefab should exist at Resources/BookSystem/Prefabs/BookManager");
    }
}
