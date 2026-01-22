using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using System.Threading.Tasks;
using System;

public class BookPaginator : MonoBehaviour
{
    // how many visualline per page
    public static int LinesPerPage = 30;
    // chars per visualline
    public static int MaxCharsPerLine = 60;

    public static Task<List<string>> Paginate(string text)
    {
        return Task.Run(() =>
        {
            // get raw lines
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
                            breakPos = i + 1; // ��������ָ��
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
        });
    }

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
