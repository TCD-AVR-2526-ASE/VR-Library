using UnityEngine;
using TMPro;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System;
using echo17.EndlessBook;

public class BookRenderer : MonoBehaviour
{
    public TMP_Text textAreaLeft;
    public TMP_Text textAreaRight;
    public TMP_Text textAreaCover;
    public int maxCharPerPage = 500;
    public BookSystem bookSystem;
    public float titleFontSize = 20;
    public GameObject bookRenderSystemPrefab;

    string content;
    private GameObject bookRenderSystem;
    // [0] cover [1] leftPage [2] rightPage
    [SerializeField]
    private List<Camera> cameras;
    [SerializeField]
    private List<TextMeshPro> pages;

    public void DisplayCurrent(Book book)
    {

        Tuple<string, string> pagesContent = book.GetPageText();
        Debug.Log(pagesContent.Item1);
        string title = book.title;

        if (pagesContent == null || title == null) return;

        textAreaCover = pages[0];
        textAreaLeft = pages[1];
        textAreaRight = pages[2];

        if (pagesContent.Item1 != null)
        {
            textAreaLeft.text = pagesContent.Item1;
            textAreaLeft.fontSize = book.fontSize;
            textAreaLeft.enableAutoSizing = false;
        }
            
        if (pagesContent.Item2 != null)
        {
            textAreaRight.text = pagesContent.Item2;
            textAreaRight.fontSize = book.fontSize;
            textAreaRight.enableAutoSizing = false;
        }

        if(title != null)
        {
            textAreaCover.text = title;
            textAreaCover.fontSize = titleFontSize;
            textAreaCover.enableAutoSizing = false;
        }

        int i = 0;
        foreach (Camera cam in cameras)
        {
            cam.targetTexture = book.renderTextures[i];
            Debug.Log(cam.targetTexture == null);
            cam.Render();
            i++;
        }
    }

    private void Awake()
    {
        bookRenderSystem = GameObject.Instantiate(bookRenderSystemPrefab);
        cameras = new List<Camera>
        {
            GameObject.Find("CoverCamera").GetComponent<Camera>(),
            GameObject.Find("PageCamera-Left").GetComponent<Camera>(),
            GameObject.Find("PageCamera-Right").GetComponent<Camera>()
        };

        pages = new List<TextMeshPro>
        {
            GameObject.Find("CoverContent").GetComponent<TextMeshPro>(),
            GameObject.Find("PageContent-Left").GetComponent<TextMeshPro>(),
            GameObject.Find("PageContent-Right").GetComponent<TextMeshPro>()
        };
    }

    //content = "Loading...";
    //Paginate();
    //textAreaLeft.fontSize = 10f;
    //textAreaLeft.enableAutoSizing = false;
    //textAreaRight.fontSize = 10f;
    //textAreaRight.enableAutoSizing = false;
    //ShowPage();

    // move to book system

    // duplicate pagination because test of local DB as well
    // remove one set & throw the other into the book data struct.
    //content = "Paginating...";
    //textAreaLeft.fontSize = 10f;
    //textAreaLeft.enableAutoSizing = false;
    //textAreaRight.fontSize = 10f;
    //textAreaRight.enableAutoSizing = false;

    //LoadText(book.path);
    //textAreaLeft.fontSize = 10f;
    //textAreaLeft.enableAutoSizing = false;
    //textAreaRight.fontSize = 10f;
    //textAreaRight.enableAutoSizing = false;
    //textAreaCover.fontSize = 20f;
    //textAreaCover.enableAutoSizing = false;
    //textAreaCover.text = book.title;
    //ShowPage();
}
