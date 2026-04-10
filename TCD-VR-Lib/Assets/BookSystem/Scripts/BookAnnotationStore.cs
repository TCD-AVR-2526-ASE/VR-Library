using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages per-page annotation textures (highlighter) and text notes.
/// Saves highlighter as PNG, notes as TXT in BookFiles/Annotations/.
/// </summary>
public static class BookAnnotationStore
{
    private static readonly string AnnotationsFolder = "./Assets/Resources/BookFiles/Annotations";
    public const int TextureSize = 1024;

    private static Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();
    private static Dictionary<string, string> notesCache = new Dictionary<string, string>();

    public static Texture2D GetTexture(string bookTitle, int pageIndex)
    {
        string key = MakeKey(bookTitle, pageIndex);
        if (cache.TryGetValue(key, out var tex) && tex != null)
            return tex;

        string path = GetFilePath(bookTitle, pageIndex);
        if (File.Exists(path))
        {
            byte[] pngData = File.ReadAllBytes(path);
            tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            tex.LoadImage(pngData);
            tex.Apply();
            cache[key] = tex;
            return tex;
        }

        tex = CreateBlankTexture();
        cache[key] = tex;
        return tex;
    }

    public static bool HasAnnotations(string bookTitle, int pageIndex)
    {
        string key = MakeKey(bookTitle, pageIndex);
        if (cache.ContainsKey(key)) return true;
        return File.Exists(GetFilePath(bookTitle, pageIndex));
    }

    public static void Save(string bookTitle, int pageIndex)
    {
        string key = MakeKey(bookTitle, pageIndex);
        if (!cache.TryGetValue(key, out var tex) || tex == null) return;

        if (!Directory.Exists(AnnotationsFolder))
            Directory.CreateDirectory(AnnotationsFolder);

        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(GetFilePath(bookTitle, pageIndex), pngData);
    }

    public static void Clear(string bookTitle, int pageIndex)
    {
        string key = MakeKey(bookTitle, pageIndex);
        if (cache.TryGetValue(key, out var tex) && tex != null)
            ClearTexture(tex);

        string path = GetFilePath(bookTitle, pageIndex);
        if (File.Exists(path))
            File.Delete(path);
    }

    /// <summary>
    /// Draw a flat highlighter strip: wide horizontally, thin vertically.
    /// </summary>
    public static void DrawHighlighterStamp(Texture2D tex, int cx, int cy, int halfWidth, int halfHeight, Color color)
    {
        int xMin = Mathf.Max(0, cx - halfWidth);
        int xMax = Mathf.Min(tex.width - 1, cx + halfWidth);
        int yMin = Mathf.Max(0, cy - halfHeight);
        int yMax = Mathf.Min(tex.height - 1, cy + halfHeight);

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                Color existing = tex.GetPixel(x, y);
                // Alpha blend — don't stack opacity on already-highlighted pixels
                if (existing.a >= color.a) continue;
                tex.SetPixel(x, y, color);
            }
        }
    }

    /// <summary>
    /// Draw a highlighter line between two points using flat rectangular stamps.
    /// </summary>
    public static void DrawHighlighterLine(Texture2D tex, Vector2Int from, Vector2Int to, int halfWidth, int halfHeight, Color color)
    {
        float dist = Vector2Int.Distance(from, to);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dist));

        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0 : (float)i / steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));
            DrawHighlighterStamp(tex, x, y, halfWidth, halfHeight, color);
        }

        tex.Apply();
    }

    // --- Notes ---

    public static string GetNote(string bookTitle, int pageIndex)
    {
        string key = MakeKey(bookTitle, pageIndex);
        if (notesCache.TryGetValue(key, out string note))
            return note;

        string path = GetNoteFilePath(bookTitle, pageIndex);
        if (File.Exists(path))
        {
            note = File.ReadAllText(path);
            notesCache[key] = note;
            return note;
        }

        return "";
    }

    public static void SaveNote(string bookTitle, int pageIndex, string noteText)
    {
        string key = MakeKey(bookTitle, pageIndex);
        notesCache[key] = noteText;

        if (!Directory.Exists(AnnotationsFolder))
            Directory.CreateDirectory(AnnotationsFolder);

        string path = GetNoteFilePath(bookTitle, pageIndex);
        if (string.IsNullOrWhiteSpace(noteText))
        {
            if (File.Exists(path)) File.Delete(path);
            notesCache.Remove(key);
            return;
        }

        File.WriteAllText(path, noteText);
    }

    public static bool HasNote(string bookTitle, int pageIndex)
    {
        string key = MakeKey(bookTitle, pageIndex);
        if (notesCache.TryGetValue(key, out string n) && !string.IsNullOrWhiteSpace(n))
            return true;
        return File.Exists(GetNoteFilePath(bookTitle, pageIndex));
    }

    // --- Internals ---

    private static Texture2D CreateBlankTexture()
    {
        var tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        ClearTexture(tex);
        return tex;
    }

    private static void ClearTexture(Texture2D tex)
    {
        Color[] clear = new Color[tex.width * tex.height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;
        tex.SetPixels(clear);
        tex.Apply();
    }

    private static string MakeKey(string bookTitle, int pageIndex)
    {
        return bookTitle.ToLower() + "_" + pageIndex;
    }

    private static string GetFilePath(string bookTitle, int pageIndex)
    {
        string safe = SanitizeFilename(bookTitle);
        return Path.Combine(AnnotationsFolder, $"{safe}_page{pageIndex}.png");
    }

    private static string GetNoteFilePath(string bookTitle, int pageIndex)
    {
        string safe = SanitizeFilename(bookTitle);
        return Path.Combine(AnnotationsFolder, $"{safe}_page{pageIndex}_note.txt");
    }

    private static string SanitizeFilename(string title)
    {
        string safe = title.ToLower();
        foreach (char c in Path.GetInvalidFileNameChars())
            safe = safe.Replace(c, '_');
        return safe;
    }
}
