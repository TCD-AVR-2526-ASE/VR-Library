using UnityEngine;
using TMPro;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System;
using echo17.EndlessBook;

[System.Serializable]
public class BookResponse
{
    public string name;
    public int id;
    public bool success;
    public string path;
}

public static class UnityWebRequestExtension
{
    public static Task ToTask(this UnityWebRequestAsyncOperation op)
    {
        var tcs = new TaskCompletionSource<object>();
        op.completed += _ => tcs.SetResult(null);
        return tcs.Task;
    }
}

public class BookRenderer : MonoBehaviour
{
    public TMP_Text textAreaLeft;
    public TMP_Text textAreaRight;
    public TMP_Text textAreaCover;
    public int maxCharPerPage = 500;
    public BookSystem bookSystem;

    string content;

    public void DisplayNewPage(Book book)
    {
        Tuple<string, string> pages = book.GetPageText();
        if (pages == null) return;
        textAreaLeft.text = pages.Item1;
        textAreaRight.text = pages.Item2 ?? "";
    }
}
