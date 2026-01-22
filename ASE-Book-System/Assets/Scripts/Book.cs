using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "Book", menuName = "Scriptable Objects/Book")]
public class Book : ScriptableObject
{
    public string path = null;
    public string id = null;
    public float fontSize = .1f;
    public string title = "";

    public void Init(string path, string name, float fontSize)
    {
        this.path = path;
        this.fontSize = fontSize;
        title = name; 
    }
}
