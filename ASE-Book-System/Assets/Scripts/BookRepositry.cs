using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using System.Linq;


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

public class BookRepositry : MonoBehaviour
{
    // Start is called once before the first execution of U
    // pdate after the MonoBehaviour is created

    private Dictionary<string, Book> books;

    private string savePath = "./Assets/Resources/";

    private int bookCount => books.Count; 
    private const int MAX_CAPACITY = 100;

    public Book RequestBook(string bookName)
    {
        //Debug.Log("BookRepositry::RequestBook");
        Book book;

        book = GetBookFromLocalLibrary(bookName);

        if (book == null)
        {
            //Debug.Log("Load from online library");
            BookResponse bookResponse = GetBookFromOnlineLibrary(bookName);

            if (bookResponse != null && bookResponse.success)
            {
                book = AddBook(bookResponse, 10f);
                Debug.Log(book == null);
            }
            else
            {
                return null;
            }
        }

        BookPaginator.ProcessBook(book);
        return book;
    }

    Book GetBookFromLocalLibrary(string bookName)
    {
        Book book;
        //MatchName(bookName);
        return books.TryGetValue(bookName, out book) ? book : null;
    }

    private Book AddBook(BookResponse bookResponse, float fontSize = 10.0f)
    {
        //Debug.Log("BookRepositry::AddBook");
        if (bookCount >= MAX_CAPACITY)
        {
            List<string> bookNames = books.Keys.ToList();

            // A random book to be removed
            int idx = Random.Range(0, MAX_CAPACITY);

            string key = bookNames[idx];

            books.Remove(key);
        }

        string normalizedName = bookResponse.name.ToLower();
        Book book = ScriptableObject.CreateInstance<Book>();
        book.Init(bookResponse.path, bookResponse.name, fontSize);
        books.Add(normalizedName, book);

        return books[normalizedName];
    }

    BookResponse GetBookFromOnlineLibrary(string bookName)
    {
        //Debug.Log("BookRepositry::GetBookFromOnlinelLibrary");
        string url = "http://127.0.0.1:5000/search";

        string json = "{\"name\": \"" + bookName + "\"}";
        byte[] jsonByte = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonByte);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = 5;
        request.SetRequestHeader("Content-Type", "application/json");

        // sends request and never stops?
        //await request.SendWebRequest().ToTask();
        //request.SendWebRequest().ToTask();
        request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string bookInfo = request.downloadHandler.text;
            BookResponse bookResponse = JsonUtility.FromJson<BookResponse>(bookInfo);
            Debug.Log("This is the current book " + bookResponse.name);
            return bookResponse;
        }

        return null;
    }

    private void Awake()
    {
        books = new Dictionary<string, Book>(MAX_CAPACITY);

        string[] files = Directory.GetFiles(savePath, "*.txt");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            BookResponse response = new BookResponse();
            response.name = fileName.Split('_')[0];
            response.path = file;
            response.success = true;

            AddBook(response);
        }
    }

    string MatchName(string name)
    {
        Debug.Log("BookRepositry::MatchName");
        int minDist = int.MaxValue;
        string target = null;
        if (books.Count == 0) return null;

        foreach (string bookName in books.Keys.ToList())
        {
            int[,] dp = new int[name.Length + 1, bookName.Length + 1];

            for (int i = 0; i <= name.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= bookName.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= name.Length; i++)
            {
                for (int j = 1; j <= bookName.Length; j++)
                {
                    int cost = name[i - 1] == bookName[j - 1] ? 0 : 1;

                    dp[i, j] = Mathf.Min(
                        dp[i - 1, j] + 1,
                        dp[i, j - 1] + 1,
                        dp[i - 1, j - 1] + cost
                    );
                }
            }

            int dist = dp[name.Length, bookName.Length];
            if (minDist > dist)
            {
                minDist = dist;
                target = bookName;
            }
        }

        return target;
    }
}
