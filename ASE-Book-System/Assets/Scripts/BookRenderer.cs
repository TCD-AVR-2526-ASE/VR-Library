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
    // areas for presenting the book's contentss
    public TMP_Text textAreaLeft;
    public TMP_Text textAreaRight;
    public TMP_Text textAreaCover;

    // formatting for each book
    public int maxCharPerPage = 500;
    public float titleFontSize = 20;

    // the book rendering systems
    public GameObject bookRenderSystemPrefab;
    public BookSystem bookSystem;

    private GameObject bookRenderSystem;
    // [0] cover [1] leftPage [2] rightPage
    [SerializeField]
    private List<Camera> cameras;
    [SerializeField]
    private List<TextMeshPro> pages;

    // Reads the current pages and title of assigned book and then updates the render system
    // cameras inside the render system will render the contents into the book
    public void DisplayCurrent(Book book)
    {

        Tuple<string, string> pagesContent = book.GetPageText();
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
            cam.Render();
            i++;
        }
    }

    // Instantiate the book rendering system(prefab) and then references of cameras and text fields
    // disabled the render system cameras (so that they draw on commands instead of every frame)
    private void Awake()
    {
        bookRenderSystem = GameObject.Instantiate(bookRenderSystemPrefab);
        cameras = new List<Camera>
        {
            GameObject.Find("CoverCamera").GetComponent<Camera>(),
            GameObject.Find("PageCamera-Left").GetComponent<Camera>(),
            GameObject.Find("PageCamera-Right").GetComponent<Camera>()
        };

        foreach(var cam in cameras)
        {
            cam.enabled = false;
        }

        pages = new List<TextMeshPro>
        {
            GameObject.Find("CoverContent").GetComponent<TextMeshPro>(),
            GameObject.Find("PageContent-Left").GetComponent<TextMeshPro>(),
            GameObject.Find("PageContent-Right").GetComponent<TextMeshPro>()
        };
    }
}
