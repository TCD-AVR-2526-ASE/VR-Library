using echo17.EndlessBook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BookSystem : MonoBehaviour
{
    // Queues
    private Queue<string> requestQueue;
    private Queue<Book> renderQueue;

    // the max number of requests processed per frame
    private readonly int MAX_REQUEST = 30;

    // the book subsystems
    private BookRepositry bookRepo;
    public BookController bookController;
    public BookRenderer bookRenderer;

    // the prefab of each book
    [SerializeField]
    private GameObject endlessBookPrefab;

    // the main to handle the book request for asking book
    // or the render request for rendering the new book pages
    void Update()
    {
        if (requestQueue.Count != 0) {
            int i = MAX_REQUEST;
            while (i > 0 && requestQueue.Count > 0)
            {
                string bookRequest = requestQueue.Dequeue();
                Book book = GetBookFromRepositry(bookRequest);
                EndlessBook endlessBook = GameObject.Instantiate(endlessBookPrefab).GetComponent<EndlessBook>();
                book.SetBookInstance(endlessBook);
                bookController.SetBook(book);
                bookRenderer.DisplayCurrent(book);
                i--;
            }
        }

        if(renderQueue != null && renderQueue.Count > 0)
        {
            int i = MAX_REQUEST;
            while (i > 0 && renderQueue.Count > 0)
            {
                Book bookToRender = renderQueue.Dequeue();
                bookRenderer.DisplayCurrent(bookToRender);
                i--;
            }
        }

    }

    // Add one book rendering request to the tail of the render queue
    public void AddRenderRequest(Book book)
    {
        renderQueue.Enqueue(book);
    }

    // Add one book loading request to the tail of the loading queue
    // the bookRequest must exactly matches the title of the book
    public void AddBookRequest(string bookRequest)
    {
        requestQueue.Enqueue(bookRequest);
    }

    // Get one book from the local repositry
    // the bookName must matches the title of the book
    private Book GetBookFromRepositry(string bookName)
    {
        return bookRepo.RequestBook(bookName);
    }

    // Instantiate the bookSystem
    // instantiate the render queue and loading queue
    private void Awake()
    {
        bookRepo = gameObject.AddComponent<BookRepositry>();
        requestQueue = new Queue<string>();
        renderQueue = new Queue<Book>();
    }
}
