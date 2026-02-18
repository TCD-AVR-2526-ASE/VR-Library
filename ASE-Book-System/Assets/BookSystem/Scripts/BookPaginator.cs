using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class BookPaginator : MonoBehaviour
{
    /// <summary>
    /// maximum amount of lines displayed per page.
    /// </summary>
    public static int LinesPerPage = 30;
    /// <summary>
    /// maximum amount of chars per line on page.
    /// </summary>
    public static int MaxCharsPerLine = 60;

    /// <summary>
    /// Loads the book text and segments it into pages.
    /// </summary>
    /// <param name="book"></param>
    public static void ProcessBook(Book book)
    {
        string text = LoadText(book.path);
        List<string> pages = Paginate(text);
        book.Paginate(pages);
    }

    /// <summary>
    /// break the given text into small chunks that are suitable for being dispalyed on one page.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static List<string> Paginate(string text)
    {
        string[] rawLines = text.Split('\n');

        var visualLines = new List<string>();

        foreach (var raw in rawLines)
        {
            string line = raw.TrimEnd('\r');

            // null line is a line
            if (line.Length == 0)
            {
                visualLines.Add(string.Empty);
                continue;
            }

            // make visualline
            while (line.Length > 0)
            {
                if (line.Length <= MaxCharsPerLine)
                {
                    visualLines.Add(line);
                    break;
                }

                // the length ofeach visualline
                int take = MaxCharsPerLine;
                int breakPos = -1;

                // try to find a nice break
                for (int i = take; i >= Math.Max(0, take - 15); i--)
                {
                    if (IsNiceBreakChar(line[i]))
                    {
                        breakPos = i + 1; // break up line after the break character.
                        break;
                    }
                }

                // if not, directly paginate it
                if (breakPos == -1)
                    breakPos = take;

                string visual = line.Substring(0, breakPos).TrimEnd();
                visualLines.Add(visual);

                line = line.Substring(breakPos).TrimStart();
            }
        }

        var pages = new List<string>();
        int index = 0;
        int totalLines = visualLines.Count;

        while (index < totalLines)
        {
            int count = Math.Min(LinesPerPage, totalLines - index);
            var pageLines = visualLines.GetRange(index, count);
            pages.Add(string.Join("\n", pageLines));
            index += count;
        }

        return pages;
    }

    /// <summary>
    /// Load the whole contents of a book from disk.
    /// 'path' should be a valid filepath relative to the Unity Project Root.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static string LoadText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Judge whether the given char c is a good choice to be the breakpoint to create a new chunk.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private static bool IsNiceBreakChar(char c)
    {
        return char.IsWhiteSpace(c) ||
                c == ',' ||
                c == '.' ||
                c == ':' ||
                c == ';' ||
                c == '?' ||
                c == '!' ||
                c == '-' ||
                c == '—' ||
                c == '，' ||
                c == '。' ||
                c == '：' ||
                c == '；' ||
                c == '？' ||
                c == '！'; 
    }
}
