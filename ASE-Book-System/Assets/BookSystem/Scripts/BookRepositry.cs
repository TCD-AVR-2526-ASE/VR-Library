using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using System.Linq;
using System.ComponentModel;


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

    private readonly string savePath = "./Assets/Resources/BookFiles";
    private Queue<UnityWebRequestAsyncOperation> pendingRequests;

    private int bookCount => books.Count; 
    private const int MAX_CAPACITY = 100;
    public int timeoutInSec;

    // Intialize the local repositry books
    // and load all the books from disk
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

    public bool PingLocalhost()
    {
        //// localhost IP
        //string url = "127.0.0.1";
        //string site = "/search";

        //try
        //{
        //    using (System.Net.NetworkInformation.Ping pinger = new System.Net.NetworkInformation.Ping())
        //    {
        //        pinger.Site = site;
        //        PingReply reply = pinger.Send(url);
        //        return reply.Status == IPStatus.Success;
        //    }
        //}
        //catch (PingException)
        //{
        //    return false;
        //}

        string url = "http://127.0.0.1:5000/health";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 2;
        var updater = request.SendWebRequest();
        while (!updater.isDone)
        {

        }
        string json = request.downloadHandler.text;
        bool request_success = request.result == UnityWebRequest.Result.Success;

        if (!(request_success && json.Contains("ok")))
            Debug.Log(request_success ? "failed to resolve gutenberg server": "failed to resolve flask server"); 
        return request_success && json.Contains("ok");
        }


    //public bool PingGutenberg() {  
    //    return false; 
    //}

    // the bookName is the title of the book (full lower case and spacing allowed)
    // try to get the book from local library if was found locally
    // if not, try to get the book from gutenberg online library
    // if not, return a null value
    public Book RequestBook(string bookName)
    {
        Book book;

        book = GetBookFromLocalLibrary(bookName);

        if (book == null)
        {
            var request = CreateBookWebRequest(bookName, timeoutInSec);

            // -------------------- needs decoupling; This should only happen once the request comes in. -------------------- 
            BookResponse bookResponse = GetBookFromOnlineLibrary(request.webRequest);

            if (bookResponse != null && bookResponse.success)
            {
                book = AddBook(bookResponse, 10f);
            }
            else
            {
                return null;
            }
        }

        BookPaginator.ProcessBook(book);
        return book;
    }

    // the bookName is the title of the book (full lower case and spacing allowed)
    // try to get a book from local repositry books
    // if not, return a null value
    Book GetBookFromLocalLibrary(string bookName)
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
    private BookResponse GetBookFromOnlineLibrary(UnityWebRequest request)
    {
        // await request.SendWebRequest(); this will deadlock if called from Update()
        // try calling from event using .forgot()

        if (request.result == UnityWebRequest.Result.Success)
        {
            string bookInfo = request.downloadHandler.text;
            BookResponse bookResponse = JsonUtility.FromJson<BookResponse>(bookInfo);
            return bookResponse;
        }

        return null;
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
}
