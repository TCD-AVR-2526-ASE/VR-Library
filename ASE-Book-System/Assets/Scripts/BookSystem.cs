using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

public class BookSystem : MonoBehaviour
{
    public int bookSum = 100;
    private string savePath = "Assets\\Resources\\";
    private int bookCount = 0;
    private Dictionary<string, Book> books;
    private List<string> bookKeys;
    private List<string> bookIds;

    public Task<Book> GetBookFromLocalLibrary(string bookName)
    {
        string normalizedName = bookName.ToLower();
        string target = MatchName(normalizedName);

        if(books.ContainsKey(target))
            return Task.FromResult(books[target]);

        Debug.Log(target);

        return null;
    }

    public Book GetBookFromLocalLibrary(int bookId)
    {
        string id = bookId.ToString();

        if(bookIds.Contains(id))
            return books[bookKeys[bookIds.IndexOf(id)]];

        return null;
    }

    public Book AddBook(BookResponse bookResponse, float fontSize = .1f)
    {
        if(bookCount >= bookSum)
        {
            int idx = Random.Range(0, bookSum);

            string key = bookKeys[idx];

            books.Remove(key);

            bookKeys[idx] = bookKeys[bookSum - 1];
            bookKeys.RemoveAt(bookSum - 1);

            bookIds[idx] = bookIds[bookSum - 1];
            bookIds.RemoveAt(bookSum - 1);

            bookCount--;
        }

        string normalizedName = bookResponse.name.ToLower();
        bookKeys.Add(normalizedName);
        bookIds.Add(bookResponse.id);
        Book book = ScriptableObject.CreateInstance<Book>();
        book.Init(bookResponse.path, bookResponse.name, fontSize);
        books.Add(normalizedName, book);
        bookCount++;

        return books[normalizedName];
    }

    private void Awake()
    {
        books = new Dictionary<string, Book>(bookSum);
        bookKeys = new List<string>(bookSum);
        bookIds = new List<string>(bookSum);

        Debug.Log("Awake: "+savePath);

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
        int minDist = int.MaxValue;
        string target = null;
        foreach (string bookName in bookKeys)
        {
            int[,] dp = new int[name.Length + 1, bookName.Length + 1];

            for (int i = 0; i <= name.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= bookName.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= name.Length; i++) {
                for (int j = 1; j <= bookName.Length; j++) { 
                    int cost = name[i - 1] == bookName[j - 1] ? 0 : 1;

                    dp[i, j] = Mathf.Min(
                        dp[i-1, j] + 1,
                        dp[i, j-1] + 1,
                        dp[i-1, j-1] + cost
                    );
                }
            }

            int dist = dp[name.Length, bookName.Length];
            if(minDist > dist)
            {
                minDist = dist;
                target = bookName;
            }
        }

        return target;
    }
}
