using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class DrawCanvas : MonoBehaviour
{
    private Vector2 size;
    [SerializeField]
    private int imageWidth = 1080;
    [SerializeField]
    Transform point;
    [SerializeField]
    Transform topLeft;
    [SerializeField] 
    private Transform bottomRight;
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Color drawColor = Color.white;

    [SerializeField]
    private int brushSize = 5;

    private Material mat;
    private Texture2D tex;

    private Vector2 lastHit = Vector2.zero;
    private bool pressedLast = false;

    private Vector2 dimensions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Image img = GetComponent<Image>();
        Vector2 rectSize = img.GetComponent<RectTransform>().sizeDelta;
        float ratio = rectSize.y / rectSize.x;
        size = new Vector2(imageWidth, (int)(imageWidth * ratio));
        Debug.Log($"img dims=" + size);
        tex = new Texture2D((int)size.x, (int)size.y, TextureFormat.RGBA32, false);
        dimensions = new Vector2(
            bottomRight.position.x - topLeft.position.x,
            topLeft.position.y - bottomRight.position.y
        );
        mat = img.material;
        mat.SetTexture("_MainTex", tex);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            PointerEventData ptr = new PointerEventData(EventSystem.current);
            ptr.position = Input.mousePosition;
            List<RaycastResult> results = new();
            
            EventSystem.current.RaycastAll(ptr, results);

            if (results.Count > 0 && results[0].gameObject.CompareTag("Annotation Surface"))
            {
                Debug.Log("Raycast hit");
                point.position = results[0].worldPosition;
                pressedLast = true;

                Vector2 pixels = new Vector2(
                    (int)((point.position.x - topLeft.position.x) * size.x / dimensions.x),
                    (int) ((point.position.y - bottomRight.position.y) * size.y / dimensions.y)
                );
                Debug.Log($"{point.position} to pixel {pixels}.");
                int[,] brush = new int[2, 2];
                brush[0, 0] = Mathf.Max(0, (int)(pixels.x - brushSize));
                brush[0, 1] = Mathf.Min((int)size.x - 1, (int)(pixels.x + brushSize));
                brush[1, 0] = Mathf.Max(0, (int)(pixels.y - brushSize));
                brush[1, 1] = Mathf.Min((int)size.y - 1, (int)(pixels.y + brushSize));

                for (int x = brush[0, 0]; x <= brush[0, 1]; x++)
                {
                    for (int y = brush[1, 0]; y <= brush[1, 1]; y++)
                    {
                        tex.SetPixel(x, y, drawColor);
                    }
                }
            }
        }
        else
            pressedLast = false;
    }
}
