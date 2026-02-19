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
// A struct conceived to mirror the output from the flask server's download process (defined in book.py).
public class BookResponse
{
    /// <summary>
    ///  The requested book's title as defined on Gutenberg.
    /// </summary>
    public string name;
    /// <summary>
    ///  The requested book's ID on Gutenberg's DB.
    /// </summary>
    public int id;
    /// <summary>
    ///  The success of the web request.
    /// </summary>
    public bool success;
    /// <summary>
    ///  The absolute path to the requested book's txt file. <br></br>
    ///  (Might be worth redoing to relative to the Assets folder)
    /// </summary>
    public string path;
}

/// <summary>
/// A sub-system of the book system handling the saving, loading and storage of the supported books.
/// </summary>

public class BookRepository : MonoBehaviour
{
    /// <summary>
    /// A reference to the active Flask server. 
    /// </summary>
    public Process server { get; private set; }
    /// <summary>
    /// A dictionary mapping query titles to Book data objects.
    /// </summary>
    private Dictionary<string, Book> books;
    /// <summary>
    /// A reference to the book super-system.
    /// </summary>
    private BookSystem bookSystem;

    /// <summary>
    /// The path relative to the project root where book .txt files should be.
    /// </summary>
    private readonly string savePath = "./Assets/Resources/BookFiles";
    /// <summary>
    /// A queue of tuples representing web requests awaiting a response, and a function to be called as soon as the response comes in.
    /// </summary>
    private List<Tuple<UnityWebRequestAsyncOperation, Action<UnityWebRequest>>> pendingRequests;

    /// <summary>
    /// A field outputting the amount of books currently in storage.
    /// </summary>
    private int bookCount => books.Count; 
    /// <summary>
    /// The maximum amount of books we want stored on the local cache.
    /// </summary>
    private const int MAX_CAPACITY = 100;
    /// <summary>
    /// The timeout value for web requests in seconds. Defaults to 10.
    /// </summary>
    public int timeoutInSec { get; private set; }

    // Intializes the local repositry books & other variables.
    // and loads all the books from the local cache.
    private void Awake()
    {
        books = new Dictionary<string, Book>(MAX_CAPACITY);
        pendingRequests = new();

        string[] files = Directory.GetFiles(savePath, "*.txt");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            BookResponse response = new();
            response.name = fileName.Split('_')[0];
            response.path = file;
            response.success = true;

            AddBook(response);
        }
    }

    // initialize references to other book system components & start the Flask server.
    private void Start()
    {
        bookSystem = FindFirstObjectByType<BookSystem>();
        InitFlask();
    }

    // process book requests every frame.
    private void Update()
    {
        if (pendingRequests == null || pendingRequests.Count == 0)
            return;

        var clearList = new List<Tuple<UnityWebRequestAsyncOperation, Action<UnityWebRequest>>>();
        foreach(var request in pendingRequests)
        {
            if (!request.Item1.isDone)
                continue;

            request.Item2(request.Item1.webRequest);
            clearList.Add(request);
        }

        foreach(var request in clearList)
        {
            pendingRequests.Remove(request);
        }

        clearList.Clear();
    }

    // Close the Flask server cleanly when exiting the app/game.
    private void OnApplicationQuit()
    {
        ShutdownFlask();
    }

    /// <summary>
    /// Sends a ping request to the Flask server (which extends to the Gutenberg API). Times out within 2 seconds (NOT Asynchronous).
    /// </summary>
    /// <returns>
    /// A bool representing the success of the operation.
    /// </returns>
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
        bool ping_success = request_success && json.Contains("ok");

        if (!ping_success)
            UnityEngine.Debug.Log(request_success ? "failed to resolve gutenberg server": "failed to resolve flask server"); 
        return ping_success;
    }

    /// <summary>
    /// Requests a book from the Book System. <br></br>
    /// First searches for the book on the local cache and processes it if there is a match, 
    /// else sends a book request to the Flask server. 
    /// </summary>
    /// <param name="bookName">The book title (partial or exact) in lowercase, with spacing allowed.</param>
    /// <param name="online">Force an online query; defaults to false.</param>
    public void RequestBook(string bookName, bool online = false)
    {
        Book book = null;
        if(!online)
            book = GetBookFromLocalLibrary(bookName);

        if (book == null)
        {
            // automatically sends a book request.
            var request = CreateBookWebRequest(bookName, timeoutInSec);
            pendingRequests.Add(new Tuple<UnityWebRequestAsyncOperation, Action<UnityWebRequest>>(request, GetBookFromOnlineLibrary));
            return;
        }

        BookPaginator.ProcessBook(book);
        bookSystem.ProcessBookRequest(book);
    }

    /// <summary>
    /// Query the cache (dictionary compiled on Awake() + any additions since) for the provided book title.
    /// </summary>
    /// <param name="bookName">The exact book title in lowercase, including spacing potentially.</param>
    /// <returns>A Book object matching the <paramref name="bookName"/>, or a null value otherwise.</returns>
    private Book GetBookFromLocalLibrary(string bookName)
    {
        return books.TryGetValue(bookName, out Book book) ? book : null;
    }

    /// <summary>
    /// References a new book to the local cache dictionary and creates its corresponding data object (<paramref name="Book"/>).
    /// <br></br> If book capacity is reached, deletes a random book from the cache.
    /// </summary>
    /// <param name="bookResponse">The struct containing the output of the Flask query.</param>
    /// <param name="fontSize">A font size for the book; Defaults to 10</param>
    /// <returns>A <paramref name="Book"/> object containing the data corresponding to the new book.</returns>
    private Book AddBook(BookResponse bookResponse, float fontSize = 10.0f)
    {
        if (bookCount >= MAX_CAPACITY)
        {
            // needs a rework, deleting a straight up random book could cause issues!
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

    /// <summary>
    /// Creates and sends a web query request to the Flask server for the given <paramref name="bookName"/>.
    /// </summary>
    /// <param name="bookName">A partial or full book title to be queried.</param>
    /// <param name="timeout">The amount of time in seconds after which the request will be considered ignored.</param>
    /// <returns>a UnityWebRequestAsyncOperation object that can be iteratively updated in Update().</returns>
    public UnityWebRequestAsyncOperation CreateBookWebRequest(string bookName, int timeout)
    {
        // The web address to the Flask server. Currently set to localhost, port 5000 and listener /search (matches the defined operator in book.py)
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

    /// <summary>
    /// Processes a successful web request to produce a book created from its data.
    /// </summary>
    /// <param name="request">The successful request object.</param>
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

    /// <summary>
    /// Start up the Flask server and set up listeners for print() statements in the python file. <br></br>
    /// MUST ALWAYS BE COMPLEMENTED BY A ShutdownFlask() CALL AT THE END OF THE DESIRED LIFETIME!
    /// </summary>
    public void InitFlask()
    {
        // don't start a new server instance if there is another instance running.
        if (server != null && !server.HasExited)
            return;

        var scriptPath = Application.dataPath + "/BookSystem/Scripts/Book.py";
        // launches a python (FileName) script
        // defined at scriptPath (Arguments),
        // not using shell (UseShellExecute) or creating a window (CreateNoWindow),
        // Reading output (RedirectStandardOutput) and error messages (RedirectStandardError)
        // to output in Unity Debug statements.
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\"",
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

    /// <summary>
    /// Sends a signal to the Flask server requesting its termination.
    /// </summary>
    public void ShutdownFlask()
    {
        if(server != null && !server.HasExited)
            server.Kill();
    }
}
