using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BoardTool : MonoBehaviour
{
    [SerializeField] private Transform tip;
    [SerializeField] private BoardInteraction board;
    [SerializeField] private bool isEraser = false;
    // overwritten on spawn by BoardInteraction.interactionToleranceDist
    [SerializeField] public float interactionToleranceDist = 10.0f;
    private XRGrabInteractable grab;
    private Vector2? lastUV;

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    public void SetBoard(BoardInteraction targetBoard)
    {
        board = targetBoard;
    }

    
    void Update()
    {
        if (board == null || tip == null)
            return;

        if (!grab.isSelected)
        {
            lastUV = null;
            return;
        }

        Ray ray = new(tip.position, tip.forward);
        Ray backRay = new(tip.position, -tip.forward);
        board.DrawFromRay(ray, isEraser, lastUV);
        board.DrawFromRay(backRay, isEraser, lastUV);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionToleranceDist) && hit.collider.gameObject == board.gameObject) {
            Debug.Log("Hit blackboard!");
            lastUV = hit.textureCoord; 

            var tipToChalk = transform.position - tip.position;
            transform.localPosition = hit.point + tipToChalk;
        }
        // ignored for now since I think the chalk messes with the ray
        /*else if (Physics.Raycast(backRay, out RaycastHit hit2, interactionToleranceDist) && hit2.collider.gameObject == board.gameObject)
            lastUV = hit2.textureCoord;*/
        else
            lastUV = null;
    }
}
