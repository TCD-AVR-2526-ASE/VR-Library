using UnityEngine;
using TMPro;

public class BlackboardTyping : MonoBehaviour
{
    public TMP_Text boardText;
    public int maxCharacters = 500;
    public KeyCode clearKey = KeyCode.Backspace;

    void Update()
    {
        foreach (char c in Input.inputString)
        {
            //I need to switch to the Blackboard Camera when player stands near it and start typing where player doesnt move


            // Handle newline
            if (c == '\n' || c == '\r')
            {
                boardText.text += "\n";
                continue;
            }

            // Handle Backspace
            if (c == '\b')
            {
                if (boardText.text.Length > 0)
                    boardText.text = boardText.text.Substring(0, boardText.text.Length - 1);
                continue;
            }

            // Add normal characters
            if (boardText.text.Length < maxCharacters)
                boardText.text += c;
        }
    }
}
