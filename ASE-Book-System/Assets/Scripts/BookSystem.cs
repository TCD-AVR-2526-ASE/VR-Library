using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BookSystem : MonoBehaviour
{
    
    private Queue<string> requestQueue;
    private readonly int MAX_REQUEST = 30;
    private BookRepositry bookRepo;
    public BookController bookController;

    void Update()
    {
        Debug.Log("BookSystem::Update");
        if (requestQueue.Count == 0) return;

        int i = MAX_REQUEST;
        Debug.Log("Enter BookSystem::::Update::whileLoop");
        while (i > 0 && requestQueue.Count > 0) {
            Debug.Log("BookSystem::::Update::whileLoop");
            string bookRequest = requestQueue.Dequeue();
            Book book = GetBookFromRepositry(bookRequest);
            bookController.bookData = book;
            i--;
        }
    }

    public void AddBookRequest(string bookRequest)
    {
        Debug.Log("BookSystem::AddBookRequest");
        requestQueue.Enqueue(bookRequest);
    }

    private Book GetBookFromRepositry(string bookName)
    {
        Debug.Log("BookSystem::GetBookFromRepositry");
        return bookRepo.RequestBook(bookName).Result;
    }


    private void Awake()
    {
        bookRepo = gameObject.AddComponent<BookRepositry>();
        requestQueue = new Queue<string>();
    }
}
