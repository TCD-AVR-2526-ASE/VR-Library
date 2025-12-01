using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
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

    [SerializeField]
    private float rayDepth = 10f;

    private Material mat;
    private Texture2D tex;

    private Vector2 lastHit = Vector2.zero;
    private bool pressedLast = false;

    private Vector2 ratios;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Image img = GetComponent<Image>();
        Vector2 rectSize = img.GetComponent<RectTransform>().sizeDelta;
        float ratio = rectSize.x / rectSize.y;
        size = new Vector2((int)imageWidth, (int)(imageWidth / ratio));
        tex = new Texture2D((int)size.x, (int)size.y, TextureFormat.RGBA32, false);
        ratios = new Vector2(
            1f / (bottomRight.localPosition.x - topLeft.localPosition.x),
            1f / (topLeft.localPosition.y - bottomRight.localPosition.y)
        );
        mat = img.material;
        mat.SetTexture("_MainTex", tex);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDepth) && hit.collider == GetComponent<BoxCollider>())
            {
                Debug.Log("Raycast hit");
                point.position = hit.point;
                pressedLast = true;

                Vector2 pixels = new Vector2(
                    (hit.point.x - topLeft.localPosition.x) * ratios.x,
                    (hit.point.y - bottomRight.localPosition.y) * ratios.y
                );
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
