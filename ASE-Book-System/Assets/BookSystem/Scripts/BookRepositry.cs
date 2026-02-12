using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using System.Linq;
using System;
using System.Diagnostics;


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

    public Process server;
    private Dictionary<string, Book> books;
    private BookSystem bookSystem;

    private readonly string savePath = "./Assets/Resources/BookFiles";
    private Queue<Tuple<UnityWebRequestAsyncOperation, Action<UnityWebRequest>>> pendingRequests;

    private int bookCount => books.Count; 
    private const int MAX_CAPACITY = 100;
    public int timeoutInSec;

    // Intialize the local repositry books
    // and load all the books from disk
    private void Awake()
    {
        books = new Dictionary<string, Book>(MAX_CAPACITY);
        pendingRequests = new();

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

    private void Start()
    {
        bookSystem = FindFirstObjectByType<BookSystem>();
        InitFlask();
    }

    private void Update()
    {
        if (pendingRequests == null || pendingRequests.Count == 0)
            return;

        foreach(var request in pendingRequests)
        {
            if (!request.Item1.isDone)
                continue;

            request.Item2(request.Item1.webRequest);
        }
    }

    private void OnApplicationQuit()
    {
        ShutdownFlask();
    }

    public bool PingLocalhost()
    {
        string url = "http://127.0.0.1:5000/health";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 2;
        var updater = request.SendWebRequest();
        while (!updater.isDone)
        {

        }
        string json = request.downloadHandler.text;
        bool request_success = request.result == UnityWebRequest.Result.Success;
        UnityEngine.Debug.Log(json);
        if (!(request_success && json.Contains("ok")))
            UnityEngine.Debug.Log(request_success ? "failed to resolve gutenberg server": "failed to resolve flask server"); 
        return request_success && json.Contains("ok");
        }

        


    //public bool PingGutenberg() {  
    //    return false; 
    //}

    // the bookName is the title of the book (full lower case and spacing allowed)
    // try to get the book from local library if was found locally
    // if not, try to get the book from gutenberg online library
    // if not, return a null value
    // bool online forces an online search. Defaults to search in local repo first.
    public void RequestBook(string bookName, bool online = false)
    {
        Book book = null;
        if(!online)
            book = GetBookFromLocalLibrary(bookName);

        if (book == null)
        {
            var request = CreateBookWebRequest(bookName, timeoutInSec);
            pendingRequests.Enqueue(new Tuple<UnityWebRequestAsyncOperation, Action<UnityWebRequest>>(request, GetBookFromOnlineLibrary));
            return;
        }

        BookPaginator.ProcessBook(book);
        bookSystem.ProcessBookRequest(book);
    }

    // the bookName is the title of the book (full lower case and spacing allowed)
    // try to get a book from local repositry books
    // if not, return a null value
    private Book GetBookFromLocalLibrary(string bookName)
    {
        Book book;
        //MatchName(bookName);
        return books.TryGetValue(bookName, out book) ? book : null;
    }

    // the BookResponse is the response from the GetBookFromOnlineLibrary (struct defined above)
    // add a book to the local repositry
    // if the repositry reaches its max capicity
    // remove a random book from local repositry and add our book on the tail of it
    private Book AddBook(BookResponse bookResponse, float fontSize = 10.0f)
    {
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

    public UnityWebRequestAsyncOperation CreateBookWebRequest(string bookName, int timeout)
    {
        string url = "http://127.0.0.1:5000/search";

        string json = "{\"name\": \"" + bookName + "\"}";
        byte[] jsonByte = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonByte);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = timeout;
        request.SetRequestHeader("Content-Type", "application/json");

        return request.SendWebRequest();
    }

    // !!!BUG!!!
    // bug at request.SendWebRequest();
    // dose not wait web response
    private void GetBookFromOnlineLibrary(UnityWebRequest request)
    {
        // await request.SendWebRequest(); this will deadlock if called from Update()
        // try calling from event using .forgot()

        if (request.result == UnityWebRequest.Result.Success)
        {
            string bookInfo = request.downloadHandler.text;
            BookResponse bookResponse = JsonUtility.FromJson<BookResponse>(bookInfo);
            Book book;

            if (bookResponse != null && bookResponse.success)
                book = AddBook(bookResponse, 10f);
            else
                return;

            BookPaginator.ProcessBook(book);
            bookSystem.ProcessBookRequest(book);
        }
    }

    // match the closest name in the local repositry
    // now we use exact match for finding a book in local repositry
    // repurposed function to search function? 
    string MatchName(string name)
    {
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

    public void InitFlask()
    {
        var scriptPath = Application.dataPath + "/BookSystem/Scripts/Book.py";
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"server.py \"{Application.dataPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        server = new Process();
        server.StartInfo = psi;

        server.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.Log("[Flask Server] " + args.Data);
        };

        server.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.Log("[Flask Error] " + args.Data);
        };

        server.Start();
        server.BeginOutputReadLine();
        server.BeginErrorReadLine();
    }

    private void ShutdownFlask()
    {
        if(server != null && !server.HasExited)
            server.Kill();
    }
}
