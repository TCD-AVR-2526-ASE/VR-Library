using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using System.Linq;
using System;


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
/// Downloads books directly from Project Gutenberg — no Flask server required.
/// </summary>
public class BookRepository : MonoBehaviour
{
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
    /// A field outputting the amount of books currently in storage.
    /// </summary>
    private int bookCount => books.Count;
    /// <summary>
    /// The maximum amount of books we want stored on the local cache.
    /// </summary>
    private const int MAX_CAPACITY = 100;
    /// <summary>
    /// The timeout value for web requests in seconds.
    /// </summary>
    private int timeoutInSec = 60;

    public string NormalizeBookName(string bookName)
    {
        if (string.IsNullOrWhiteSpace(bookName))
            return string.Empty;

        string normalized = bookName.Trim().ToLowerInvariant().Replace('_', ' ');
        return System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ").Trim();
    }

    // Intializes the local repositry books & other variables.
    // and loads all the books from the local cache.
    private void Awake()
    {
        books = new Dictionary<string, Book>(MAX_CAPACITY);

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log("[BookSystem] Created missing BookFiles directory at: " + savePath);
            return;
        }

        string[] files = Directory.GetFiles(savePath, "*.txt");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string title = fileName;
            int id = -1;

            int separatorIndex = fileName.LastIndexOf('_');
            if (separatorIndex > 0)
            {
                string maybeId = fileName.Substring(separatorIndex + 1);
                if (int.TryParse(maybeId, out int parsedId))
                {
                    id = parsedId;
                    title = fileName.Substring(0, separatorIndex);
                }
            }

            BookResponse response = new();
            response.name = title.Replace('_', ' ');
            response.id = id;
            response.path = file;
            response.success = true;

            AddBook(response);
        }
    }

    // initialize references to other book system components.
    private void Start()
    {
        bookSystem = FindFirstObjectByType<BookSystem>();
    }

    /// <summary>
    /// Clears all cached books from memory and deletes local book files from disk.
    /// </summary>
    public void ClearLocalCache()
    {
        books.Clear();

        if (Directory.Exists(savePath))
        {
            string[] files = Directory.GetFiles(savePath, "*.txt");
            foreach (string file in files)
            {
                try { File.Delete(file); }
                catch (Exception e) { Debug.LogWarning($"[BookSystem] Could not delete {file}: {e.Message}"); }
            }
        }

        Debug.Log($"[BookSystem] Local book cache cleared.");
    }

    /// <summary>
    /// O(n) Book search by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Book GetBook(int id)
    {
        foreach(var book in books.Values)
            if (book.id == id)
                return book;

        return null;
    }

    /// <summary>
    /// Requests a book from the Book System. <br></br>
    /// First searches for the book on the local cache and processes it if there is a match,
    /// else downloads directly from Project Gutenberg via Gutendex.
    /// </summary>
    /// <param name="bookName">The book title (partial or exact) in lowercase, with spacing allowed.</param>
    /// <param name="online">Force an online query; defaults to false.</param>
    public void RequestBook(string bookName, bool online = false)
    {
        LoadBookData(bookName, book =>
        {
            if (book == null)
                return;

            if (bookSystem != null)
                bookSystem.ProcessBookRequest(book);
        }, online);
    }

    /// <summary>
    /// Loads a book's data without spawning a physical book object.
    /// This is used by shared/networked books so every client can bind the same title
    /// to an already spawned network object.
    /// </summary>
    public void LoadBookData(string bookName, Action<Book> onLoaded, bool online = false)
    {
        StartCoroutine(LoadBookDataRoutine(bookName, onLoaded, online));
    }

    public Book GetCachedBook(string bookName)
    {
        return GetBookFromLocalLibrary(bookName);
    }

    public bool TryGetBookText(Book book, out string text)
    {
        text = null;

        if (book == null || string.IsNullOrWhiteSpace(book.path) || !File.Exists(book.path))
            return false;

        try
        {
            text = File.ReadAllText(book.path);
            return !string.IsNullOrEmpty(text);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[BookSystem] Failed to read book text for '{book.title}': {e.Message}");
            return false;
        }
    }

    public Book CreateOrUpdateBookFromText(string bookTitle, int bookId, string bookText, float fontSize = 10f, bool saveToCache = true)
    {
        if (string.IsNullOrWhiteSpace(bookTitle) || string.IsNullOrEmpty(bookText))
            return null;

        string normalizedName = NormalizeBookName(bookTitle);
        if (books.TryGetValue(normalizedName, out Book existing))
        {
            BookPaginator.ProcessBookText(existing, bookText);
            return existing;
        }

        string filePath = saveToCache ? SaveBookTextToCache(bookTitle, bookId, bookText) : null;
        if (saveToCache && string.IsNullOrEmpty(filePath))
            return null;

        Book book = ScriptableObject.CreateInstance<Book>();
        book.Init(filePath ?? string.Empty, bookTitle, fontSize, bookId);
        BookPaginator.ProcessBookText(book, bookText);
        books[normalizedName] = book;

        Debug.Log($"[BookSystem] Created authoritative book data for '{bookTitle}'.");
        return book;
    }

    private IEnumerator LoadBookDataRoutine(string bookName, Action<Book> onLoaded, bool online)
    {
        Book book = null;
        if (!online)
            book = GetBookFromLocalLibrary(bookName);

        if (book != null)
        {
            Debug.Log("[BookSystem] Fetching book " + bookName + " from local cache.");
            BookPaginator.ProcessBook(book);
            onLoaded?.Invoke(book);
            yield break;
        }

        Debug.Log("[BookSystem] Fetching book '" + bookName + "' from Project Gutenberg...");
        yield return StartCoroutine(DownloadBookFromGutenberg(bookName, onLoaded));
    }

    /// <summary>
    /// Downloads a book directly from Project Gutenberg using the Gutendex API.
    /// 1. Searches Gutendex for the book title
    /// 2. Finds the plain text download URL
    /// 3. Downloads the text file
    /// 4. Saves it locally and processes it
    /// </summary>
    private IEnumerator DownloadBookFromGutenberg(string bookName, Action<Book> onLoaded = null)
    {
        // Step 1: Search Gutendex for the book
        string searchUrl = "https://gutendex.com/books?search=" + UnityWebRequest.EscapeURL(bookName);
        Debug.Log($"[BookSystem] Searching Gutendex: {searchUrl}");

        using (UnityWebRequest searchRequest = UnityWebRequest.Get(searchUrl))
        {
            searchRequest.SetRequestHeader("User-Agent", "Mozilla/5.0");
            searchRequest.timeout = timeoutInSec;
            yield return searchRequest.SendWebRequest();

            if (searchRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[BookSystem] Gutendex search failed: {searchRequest.error}");
                yield break;
            }

            string json = searchRequest.downloadHandler.text;
            GutendexSearchResponse response = JsonUtility.FromJson<GutendexSearchResponse>(json);

            if (response == null || response.results == null || response.results.Length == 0)
            {
                Debug.LogWarning($"[BookSystem] No books found on Gutenberg for '{bookName}'.");
                yield break;
            }

            // Find the best match (first result)
            GutendexBookResult bookResult = response.results[0];
            Debug.Log($"[BookSystem] Found: '{bookResult.title}' (ID: {bookResult.id})");

            // Step 2: Find the plain text download URL
            string textUrl = FindTextUrl(bookResult);
            if (string.IsNullOrEmpty(textUrl))
            {
                Debug.LogError($"[BookSystem] No plain text format available for '{bookResult.title}'.");
                yield break;
            }

            Debug.Log($"[BookSystem] Downloading text from: {textUrl}");

            // Step 3: Download the text file
            using (UnityWebRequest downloadRequest = UnityWebRequest.Get(textUrl))
            {
                downloadRequest.SetRequestHeader("User-Agent", "Mozilla/5.0");
                downloadRequest.timeout = timeoutInSec;
                yield return downloadRequest.SendWebRequest();

                if (downloadRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[BookSystem] Download failed: {downloadRequest.error}");
                    yield break;
                }

                string bookText = downloadRequest.downloadHandler.text;

                if (string.IsNullOrEmpty(bookText))
                {
                    Debug.LogError($"[BookSystem] Downloaded file is empty for '{bookResult.title}'.");
                    yield break;
                }

                // Step 4: Save the file locally
                string safeName = SanitizeFileName(bookResult.title);
                // Truncate for Windows MAX_PATH
                if (safeName.Length > 80)
                    safeName = safeName.Substring(0, 80);

                string fileName = $"{safeName}_{bookResult.id}.txt";
                string filePath = Path.Combine(savePath, fileName);

                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                try
                {
                    File.WriteAllText(filePath, bookText, Encoding.UTF8);
                    Debug.Log($"[BookSystem] Saved book to: {filePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BookSystem] Failed to save book file: {e.Message}");
                    yield break;
                }

                // Step 5: Create BookResponse and process
                BookResponse bookResponse = new BookResponse
                {
                    name = bookResult.title,
                    id = bookResult.id,
                    success = true,
                    path = filePath
                };

                Book book = AddBook(bookResponse, 10f);
                BookPaginator.ProcessBook(book);
                onLoaded?.Invoke(book);
            }
        }
    }

    /// <summary>
    /// Finds the best plain text URL from a Gutendex book result's formats.
    /// Gutendex returns formats as a dictionary, but since JsonUtility can't parse dictionaries,
    /// we try well-known Gutenberg URL patterns.
    /// </summary>
    private string FindTextUrl(GutendexBookResult book)
    {
        // Gutenberg has predictable text URLs based on book ID
        // Try the most common patterns
        return $"https://www.gutenberg.org/cache/epub/{book.id}/pg{book.id}.txt";
    }

    /// <summary>
    /// Sanitizes a string to be safe for use as a filename.
    /// </summary>
    private string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    /// <summary>
    /// Query the cache (dictionary compiled on Awake() + any additions since) for the provided book title.
    /// </summary>
    /// <param name="bookName">The exact book title in lowercase, including spacing potentially.</param>
    /// <returns>A Book object matching the <paramref name="bookName"/>, or a null value otherwise.</returns>
    private Book GetBookFromLocalLibrary(string bookName)
    {
        string normalizedName = NormalizeBookName(bookName);
        return books.TryGetValue(normalizedName, out Book book) ? book : null;
    }

    /// <summary>
    /// References a new book to the local cache dictionary and creates its corresponding data object.
    /// <br></br> If book capacity is reached, deletes a random book from the cache.
    /// </summary>
    /// <param name="bookResponse">The struct containing the output of the query.</param>
    /// <param name="fontSize">A font size for the book; Defaults to 10</param>
    /// <returns>A Book object containing the data corresponding to the new book.</returns>
    private Book AddBook(BookResponse bookResponse, float fontSize = 20.0f)
    {
        if (bookCount >= MAX_CAPACITY)
        {
            List<string> bookNames = books.Keys.ToList();
            int idx = Random.Range(0, bookNames.Count);
            string key = bookNames[idx];
            books.Remove(key);
        }

        string normalizedName = NormalizeBookName(bookResponse.name);

        // Return existing book if already cached
        if (books.TryGetValue(normalizedName, out Book existing))
            return existing;

        Book book = ScriptableObject.CreateInstance<Book>();
        book.Init(bookResponse.path, bookResponse.name, fontSize, bookResponse.id);
        books.Add(normalizedName, book);

        Debug.Log("[BookSystem] Successfully added book to the registry.");

        return books[normalizedName];
    }

    private string SaveBookTextToCache(string bookTitle, int bookId, string bookText)
    {
        string safeName = SanitizeFileName(bookTitle);
        if (safeName.Length > 80)
            safeName = safeName.Substring(0, 80);

        string suffix = bookId >= 0 ? $"_{bookId}" : string.Empty;
        string fileName = $"{safeName}{suffix}.txt";
        string filePath = Path.Combine(savePath, fileName);

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        try
        {
            File.WriteAllText(filePath, bookText, Encoding.UTF8);
            return filePath;
        }
        catch (Exception e)
        {
            Debug.LogError($"[BookSystem] Failed to save authoritative book file: {e.Message}");
            return null;
        }
    }

    // --- Gutendex JSON response classes ---

    [Serializable]
    private class GutendexSearchResponse
    {
        public int count;
        public GutendexBookResult[] results;
    }

    [Serializable]
    private class GutendexBookResult
    {
        public int id;
        public string title;
    }
}
