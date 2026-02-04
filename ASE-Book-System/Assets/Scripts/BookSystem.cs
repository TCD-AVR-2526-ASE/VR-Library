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
    
    private Queue<string> requestQueue;
    private Queue<Book> renderQueue;
    private readonly int MAX_REQUEST = 30;
    private BookRepositry bookRepo;
    public BookController bookController;
    [SerializeField]
    private GameObject endlessBookPrefab;
    public BookRenderer bookRenderer;

    void Update()
    {
        //Debug.Log("BookSystem::Update");
        if (requestQueue.Count != 0) {
            int i = MAX_REQUEST;
            //Debug.Log("Enter BookSystem::Update::whileLoop");
            while (i > 0 && requestQueue.Count > 0)
            {
                //Debug.Log("BookSystem::Update::whileLoop");
                string bookRequest = requestQueue.Dequeue();
                Book book = GetBookFromRepositry(bookRequest);
                Debug.Log(book == null);
                EndlessBook endlessBook = GameObject.Instantiate(endlessBookPrefab).GetComponent<EndlessBook>();
                book.SetBookInstance(endlessBook);
                bookController.SetBook(book);
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
                //RenderRequest(bookToRender);
                i--;
            }
        }

    }

    public void AddRenderRequest(Book book)
    {
        renderQueue.Enqueue(book);
    }

    public void AddBookRequest(string bookRequest)
    {
        //Debug.Log("BookSystem::AddBookRequest");
        requestQueue.Enqueue(bookRequest);
    }

    private Book GetBookFromRepositry(string bookName)
    {
        //Debug.Log("BookSystem::GetBookFromRepositry");
        return bookRepo.RequestBook(bookName);
    }


    private void Awake()
    {
        bookRepo = gameObject.AddComponent<BookRepositry>();
        requestQueue = new Queue<string>();
        renderQueue = new Queue<Book>();
    }
}
