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
    public string id;
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

public class BookLoader : MonoBehaviour
{
    public TMP_Text textAreaLeft;
    public TMP_Text textAreaRight;
    public TMP_Text textAreaCover;
    public int maxCharPerPage = 500;
    public TMP_InputField inputName;
    public BookSystem bookSystem;

    string content;

    public async void RequestBook(bool online)
    {
        string bookName = inputName.text;
        Book book;
        content = "Loading...";
        Paginate();
        textAreaLeft.fontSize = 10f;
        textAreaLeft.enableAutoSizing = false;
        textAreaRight.fontSize = 10f;
        textAreaRight.enableAutoSizing = false;
        ShowPage();

        if (online)
        {
            Debug.Log("Load from online library");
            BookResponse bookResponse = await GetBookFromOnlineLibrary(bookName);

            if (bookResponse.success)
            {
                book = bookSystem.AddBook(bookResponse, 10f);
            }
            else
            {
                content = "Failed to get book";
                return;
            }
        }
        else
        {
            Debug.Log("Load from local library");
            book = await GetBookFromLocalLibrary(bookName);
            
        }

        // duplicate pagination because test of local DB as well
        // remove one set & throw the other into the book data struct.
        content = "Paginating...";
        Paginate();
        textAreaLeft.fontSize = 10f;
        textAreaLeft.enableAutoSizing = false;
        textAreaRight.fontSize = 10f;
        textAreaRight.enableAutoSizing = false;
        ShowPage();

        LoadText(book.path);
        Paginate();
        textAreaLeft.fontSize = 10f;
        textAreaLeft.enableAutoSizing = false;
        textAreaRight.fontSize = 10f;
        textAreaRight.enableAutoSizing = false;
        textAreaCover.fontSize = 20f;
        textAreaCover.enableAutoSizing = false;
        textAreaCover.text = book.title;
        ShowPage();
    }


    public void DisplayNewPage(Book book)
    {
        Tuple<string, string> pages = book.GetPageText();
        if (pages == null) return;
        textAreaLeft.text = pages.Item1;
        textAreaRight.text = pages.Item2 ?? "";
    }

    void LoadText(string path)
    {
        content = File.ReadAllText(path);
    }

    async Task<Book> GetBookFromLocalLibrary(string bookName)
    {
        return await bookSystem.GetBookFromLocalLibrary(bookName);
    }

    async void Paginate()
    {
        pages = await BookPaginator.Paginate(content);
        pageCount = pages.Count;
        pageIndex = 0;
    }

    async Task<BookResponse> GetBookFromOnlineLibrary(string bookName)
    {
        string url = "http://127.0.0.1:5000/search";

        string json = "{\"name\": \"" + bookName + "\"}";
        byte[] jsonByte = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonByte);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest().ToTask();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string bookInfo = request.downloadHandler.text;
            BookResponse bookResponse = JsonUtility.FromJson<BookResponse>(bookInfo);
            return bookResponse;
        }

        return null;
    }
}
