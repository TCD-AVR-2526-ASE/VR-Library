using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class BookRepositry : MonoBehaviour
{
    // Start is called once before the first execution of U
    // pdate after the MonoBehaviour is created

    private Dictionary<string, Book> books;
    private List<string> bookNames;
    private List<int> bookIds;

    private int bookCount => bookIds.Count; 
    private const int MAX_CAPACITY = 100;

    public void LoadBoook(Tuple<string, bool> BooklRequest)
    {

    }

    public async void RequestBook(bool online)
    {
        Book book;
        content = "Loading...";
        Paginate();
        textAreaLeft.fontSize = 10f;
        textAreaLeft.enableAutoSizing = false;
        textAreaRight.fontSize = 10f;
        textAreaRight.enableAutoSizing = false;
        ShowPage();

        // move to book system
        if (online)
        {
            Debug.Log("Load from online library");
            BookResponse bookResponse = await GetBookFromOnlineLibrary(bookNames);

            if (bookResponse.success)
            {
                book = bookSystem.AddBook(bookResponse, 10f);
                string text = LoadText(book.path);
                await BookPaginator.ProcessBook(book, text);
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
            book = await GetBookFromLocalLibrary(bookNames);
        }

        // duplicate pagination because test of local DB as well
        // remove one set & throw the other into the book data struct.
        content = "Paginating...";
        textAreaLeft.fontSize = 10f;
        textAreaLeft.enableAutoSizing = false;
        textAreaRight.fontSize = 10f;
        textAreaRight.enableAutoSizing = false;

        LoadText(book.path);
        textAreaLeft.fontSize = 10f;
        textAreaLeft.enableAutoSizing = false;
        textAreaRight.fontSize = 10f;
        textAreaRight.enableAutoSizing = false;
        textAreaCover.fontSize = 20f;
        textAreaCover.enableAutoSizing = false;
        textAreaCover.text = book.title;
        ShowPage();
    }

    Book GetBookFromLocalLibrary(string bookName)
    {
        int id = books[bookName].id;

        if (bookIds.Contains(id))
        {
            return books[bookNames[bookIds.IndexOf(id)]];
        }

        return null;
    }

    public Book AddBook(BookResponse bookResponse, float fontSize = .1f)
    {
        if (bookCount >= MAX_CAPACITY)
        {
            // A random book to be removed
            int idx = Random.Range(0, MAX_CAPACITY);

            string key = bookNames[idx];

            books.Remove(key);

            bookNames[idx] = bookNames[MAX_CAPACITY - 1];
            bookNames.RemoveAt(MAX_CAPACITY - 1);

            bookIds[idx] = bookIds[MAX_CAPACITY - 1];
            bookIds.RemoveAt(MAX_CAPACITY - 1);
        }

        string normalizedName = bookResponse.name.ToLower();
        bookNames.Add(normalizedName);
        bookIds.Add(bookResponse.id);
        Book book = ScriptableObject.CreateInstance<Book>();
        book.Init(bookResponse.path, bookResponse.name, fontSize);
        books.Add(normalizedName, book);

        return books[normalizedName];
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

    string LoadText(string path)
    {
        return File.ReadAllText(path);
    }
}
