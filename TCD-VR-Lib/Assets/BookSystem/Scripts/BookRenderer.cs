using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// Renders book text content onto RenderTextures via offscreen cameras + TextMeshPro.
/// After rendering text, composites any annotation drawings on top.
/// </summary>
public class BookRenderer : MonoBehaviour
{
    [Header("Render System")]
    [SerializeField] private string renderingSystemPrefabPath = "BookSystem/Prefabs/BookRenderSystem";

    [Header("Text Formatting")]
    [SerializeField] private float titleFontSize = 20f;

    // Text areas: [0] cover, [1] left page, [2] right page
    private List<TextMeshPro> pages;

    // Cameras: [0] cover, [1] left page, [2] right page
    private List<Camera> cameras;

    // Reusable temp texture for annotation compositing
    private Texture2D compositeTex;

    private void Awake()
    {
        var prefab = Resources.Load<GameObject>(renderingSystemPrefabPath);
        Debug.Assert(prefab != null, "[BookRenderer] BookRenderSystem prefab not found at: " + renderingSystemPrefabPath);

        var renderSystem = Instantiate(prefab, Vector3.up * 1000f, Quaternion.identity);
        renderSystem.name = "BookRenderSystem";

        cameras = new List<Camera>
        {
            renderSystem.transform.Find("CoverCamera")?.GetComponent<Camera>(),
            renderSystem.transform.Find("PageCamera-Left")?.GetComponent<Camera>(),
            renderSystem.transform.Find("PageCamera-Right")?.GetComponent<Camera>()
        };

        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] == null)
            {
                string[] names = { "CoverCamera", "PageCamera-Left", "PageCamera-Right" };
                cameras[i] = GameObject.Find(names[i])?.GetComponent<Camera>();
            }
        }

        foreach (var cam in cameras)
        {
            if (cam == null) continue;
            cam.enabled = false;
            EnsureURPCameraData(cam);

            var listener = cam.GetComponent<AudioListener>();
            if (listener != null)
                Destroy(listener);
        }

        pages = new List<TextMeshPro>();
        string[] contentNames = { "CoverContent", "PageContent-Left", "PageContent-Right" };
        foreach (var contentName in contentNames)
        {
            var tmp = renderSystem.transform.Find(contentName)?.GetComponent<TextMeshPro>();
            if (tmp == null)
                tmp = GameObject.Find(contentName)?.GetComponent<TextMeshPro>();
            pages.Add(tmp);
        }

        compositeTex = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
    }

    /// <summary>
    /// Render the current page spread, cover title, and annotation overlays.
    /// </summary>
    public void DisplayCurrent(Book book)
    {
        Tuple<string, string> pageContent = book.GetPageText();
        if (pageContent == null) return;

        // Update text content
        if (pages[0] != null)
        {
            pages[0].text = book.title ?? "";
            pages[0].fontSize = titleFontSize;
            pages[0].enableAutoSizing = false;
        }

        if (pages[1] != null)
        {
            pages[1].text = pageContent.Item1 ?? "";
            pages[1].fontSize = book.fontSize;
            pages[1].enableAutoSizing = false;
        }

        if (pages[2] != null)
        {
            pages[2].text = pageContent.Item2 ?? "";
            pages[2].fontSize = book.fontSize;
            pages[2].enableAutoSizing = false;
        }

        // Render text to page textures
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] == null || i >= book.renderTextures.Count) continue;
            cameras[i].targetTexture = book.renderTextures[i];
            cameras[i].Render();
        }

        // Composite annotation textures on top of left and right pages
        CompositeAnnotation(book.renderTextures[1], book.title, book.activePage);      // left page
        CompositeAnnotation(book.renderTextures[2], book.title, book.activePage + 1);  // right page
    }

    /// <summary>
    /// Blit the annotation texture on top of the page RenderTexture using CPU pixel blending.
    /// </summary>
    private void CompositeAnnotation(RenderTexture pageRT, string bookTitle, int pageIndex)
    {
        if (!BookAnnotationStore.HasAnnotations(bookTitle, pageIndex)) return;

        var annotationTex = BookAnnotationStore.GetTexture(bookTitle, pageIndex);
        if (annotationTex == null) return;

        // Read the rendered page pixels back from the RT
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = pageRT;

        if (compositeTex == null || compositeTex.width != pageRT.width || compositeTex.height != pageRT.height)
            compositeTex = new Texture2D(pageRT.width, pageRT.height, TextureFormat.RGBA32, false);

        compositeTex.ReadPixels(new Rect(0, 0, pageRT.width, pageRT.height), 0, 0);

        // Alpha-blend annotation pixels on top of page pixels
        Color[] pagePixels = compositeTex.GetPixels();
        Color[] annotPixels = annotationTex.GetPixels();
        int count = Mathf.Min(pagePixels.Length, annotPixels.Length);

        for (int i = 0; i < count; i++)
        {
            Color a = annotPixels[i];
            if (a.a <= 0f) continue;

            Color p = pagePixels[i];
            float outA = a.a + p.a * (1f - a.a);
            if (outA > 0f)
            {
                pagePixels[i] = new Color(
                    (a.r * a.a + p.r * p.a * (1f - a.a)) / outA,
                    (a.g * a.a + p.g * p.a * (1f - a.a)) / outA,
                    (a.b * a.a + p.b * p.a * (1f - a.a)) / outA,
                    outA
                );
            }
        }

        compositeTex.SetPixels(pagePixels);
        compositeTex.Apply();

        // Write blended result back to the RenderTexture
        Graphics.Blit(compositeTex, pageRT);
        RenderTexture.active = prev;
    }

    private void EnsureURPCameraData(Camera cam)
    {
        var urpCamType = System.Type.GetType(
            "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (urpCamType == null) return;

        var existing = cam.GetComponent(urpCamType);
        if (existing == null)
            cam.gameObject.AddComponent(urpCamType);
    }
}
