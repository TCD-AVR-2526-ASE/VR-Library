using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BookSystem : MonoBehaviour
{
    public int bookSum = 100;
    private string savePath = "Assets\\Resources\\";
    private int bookCount = 0;
    private Queue<Tuple<string, bool>> requestQueue;
    private int MAX_REQUEST = 30;
    BookRepositry bookLoader;

    private async Task Update()
    {
        if (requestQueue.Count == 0) return;
        int i = MAX_REQUEST;

        while (i > 0 && requestQueue.Count > 0) {
            Tuple<string, bool> bookRequest = requestQueue.Dequeue();
            if (bookRequest.Item2)
            {
                
            }
            else { 

            }
            i--;
        }
    }

    public void AddBookRequest(Tuple<string, bool> bookRequest)
    {
        requestQueue.Enqueue(bookRequest);
    }

    public Task<Book> GetBookFromLocalLibrary(string bookName)
    {
        
    }

    public Book GetBookFromLocalLibrary(int bookId)
    {
       
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
