using UnityEngine;

public class BlackboardInteraction : MonoBehaviour
{
    public static bool isWriting = false;

    public Camera mainCamera;
    public Camera blackboardCamera;
    public BlackboardTyping typingScript;
    public MonoBehaviour movementScript;

    bool insideZone = false;

    void Start()
    {
        mainCamera.enabled = true;
        blackboardCamera.enabled = false;
        typingScript.enabled = false;
    }

    void Update()
    {
        // ENTER writing mode (only if near + not already writing)
        if (insideZone && !isWriting && Input.GetKeyDown(KeyCode.G))
        {
            EnterBlackboard();
        }

        // EXIT writing mode (ESC)
        if (isWriting && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitBlackboard();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            insideZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            insideZone = false;
            if (isWriting) ExitBlackboard(); // auto-exit if walking away
        }
    }

    void EnterBlackboard()
    {
        isWriting = true;

        mainCamera.enabled = false;
        blackboardCamera.enabled = true;

        movementScript.enabled = false;
        typingScript.enabled = true;

    }

    void ExitBlackboard()
    {
        isWriting = false;

        blackboardCamera.enabled = false;
        mainCamera.enabled = true;

        movementScript.enabled = true;
        typingScript.enabled = false;
    }
}
