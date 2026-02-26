using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;

public class BookRenderer : MonoBehaviour
{
    /// <summary>
    /// Path relative to Assets/Resources folder to access the BookRenderingSystem prefab (no file extension)
    /// </summary>
    private readonly string renderingSystemPrefabPath = "BookSystem/Prefabs/BookRenderSystem";

    // areas for presenting the book's contentss
    private TMP_Text textAreaLeft;
    private TMP_Text textAreaRight;
    private TMP_Text textAreaCover;

    // formatting for each book
    [SerializeField]
    private int maxCharPerPage = 500;
    [SerializeField]
    private float titleFontSize = 20;

    // the book rendering systems
    private GameObject bookRenderSystemPrefab;
    private BookSystem bookSystem;
    private GameObject bookRenderSystem;

    // [0] cover [1] leftPage [2] rightPage
    [SerializeField]
    private List<Camera> cameras;
    [SerializeField]
    private List<TextMeshPro> pages;

    private Camera targetCamera;

    // Instantiate the book rendering system(prefab) and then references of cameras and text fields
    // disabled the render system cameras (so that they draw on commands instead of every frame)
    private void Awake()
    {
        bookRenderSystemPrefab = Resources.Load<GameObject>(renderingSystemPrefabPath);
        Debug.Assert(bookRenderSystemPrefab != null);

        float spawnDepth = 3.5f;
        targetCamera = Camera.main;
        Vector3 viewportCenter = new Vector3(0.5f, 0.4f, spawnDepth);
        Debug.Log(viewportCenter);
        Vector3 spawnPosition = targetCamera.ViewportToWorldPoint(viewportCenter);
        spawnPosition.y += 1.0f; // adjust height above ground
        bookRenderSystem = Instantiate(bookRenderSystemPrefab, spawnPosition, Quaternion.identity);
        bookRenderSystem.transform.localScale = Vector3.one * 4.5f;
        //bookRenderSystem = Instantiate(bookRenderSystemPrefab);
        cameras = new List<Camera>
        {
            GameObject.Find("CoverCamera").GetComponent<Camera>(),
            GameObject.Find("PageCamera-Left").GetComponent<Camera>(),
            GameObject.Find("PageCamera-Right").GetComponent<Camera>()
        };

        foreach (var cam in cameras)
        {
            cam.enabled = false;
        }

        pages = new List<TextMeshPro>
        {
            GameObject.Find("CoverContent").GetComponent<TextMeshPro>(),
            GameObject.Find("PageContent-Left").GetComponent<TextMeshPro>(),
            GameObject.Find("PageContent-Right").GetComponent<TextMeshPro>()
        };
        //float spawnDepth = 5f;
        //targetCamera = Camera.main;
        //Vector3 viewportCenter = new Vector3(0.5f, 0.5f, spawnDepth);
        //Vector3 spawnPosition = targetCamera.ViewportToWorldPoint(viewportCenter);

        //bookRenderSystem = Instantiate(bookRenderSystemPrefab, spawnPosition, Quaternion.identity);

    }

    private void Start()
    {
        bookSystem = FindFirstObjectByType<BookSystem>();
    }

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
}
