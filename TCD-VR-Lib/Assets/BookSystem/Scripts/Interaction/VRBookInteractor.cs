using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// VR input for book interaction. Attach to each book GameObject.
/// - Right trigger : turn page forward
/// - Left trigger  : turn page backward
/// - Left primary  : open / close
/// - Grip          : grab and move the book (via XRGrabInteractable)
/// All page/open inputs are blocked when annotation mode is active.
/// Hover is only required for page turning and open/close — not for the annotation block.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class VRBookInteractor : MonoBehaviour, IBookInteractor
{
    public bool TurnPageForward => turnForwardThisFrame;
    public bool TurnPageBackward => turnBackwardThisFrame;
    public bool ToggleOpen => toggleOpenThisFrame;

    private bool turnForwardThisFrame;
    private bool turnBackwardThisFrame;
    private bool toggleOpenThisFrame;

    private XRGrabInteractable grabInteractable;
    private bool isHovered;
    private bool inAnnotations = false;

    private InputAction triggerAction;
    private InputAction turnBackAction;
    private InputAction openCloseAction;

    private const string nextPageBinding = "<XRController>{RightHand}/triggerPressed";
    private const string prevPageBinding = "<XRController>{LeftHand}/triggerPressed";
    private const string toggleOpenBookBinding = "<XRController>{LeftHand}/primaryButton";

    // -------------------------------------------------------------------------
    // Static broadcast — updates every book in the scene at once
    // -------------------------------------------------------------------------
    public static void SetAllAnnotationModifiers(bool annotationsAreActive)
    {
        foreach (var interactor in FindObjectsByType<VRBookInteractor>(FindObjectsSortMode.None))
            interactor.SetAnnotationModifier(annotationsAreActive);
    }

    // -------------------------------------------------------------------------
    // Instance setter — call this directly if you have a specific book reference
    // -------------------------------------------------------------------------
    public void SetAnnotationModifier(bool annotationsAreActive)
    {
        inAnnotations = annotationsAreActive;

        if (annotationsAreActive)
        {
            // Fully disable conflicting actions so BookAnnotationUI's draw can fire cleanly
            triggerAction.Disable();
            turnBackAction.Disable();
            openCloseAction.Disable();
        }
        else
        {
            // Re-enable when leaving annotation mode
            triggerAction.Enable();
            turnBackAction.Enable();
            openCloseAction.Enable();
        }

        Debug.Log($"[VRBookInteractor] '{gameObject.name}' — annotation mode: {inAnnotations} | book interactions enabled: {!inAnnotations}");
    }
    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
        grabInteractable.throwOnDetach = false;

        grabInteractable.hoverEntered.AddListener(OnHoverEnter);
        grabInteractable.hoverExited.AddListener(OnHoverExit);
        grabInteractable.selectEntered.AddListener(OnGrabStarted);
        grabInteractable.selectExited.AddListener(OnGrabEnded);

        triggerAction = new InputAction("BookPageForward", InputActionType.Button);
        triggerAction.AddBinding(nextPageBinding);

        turnBackAction = new InputAction("BookPageBackward", InputActionType.Button);
        turnBackAction.AddBinding(prevPageBinding);

        openCloseAction = new InputAction("BookOpenClose", InputActionType.Button);
        openCloseAction.AddBinding(toggleOpenBookBinding);
    }

    public void OnEnable()
    {
        triggerAction.Enable();
        triggerAction.performed += OnTrigger;

        turnBackAction.Enable();
        turnBackAction.performed += OnTurnBack;

        openCloseAction.Enable();
        openCloseAction.performed += OnOpenClose;
    }

    public void OnDisable()
    {
        triggerAction.performed -= OnTrigger;
        triggerAction.Disable();

        turnBackAction.performed -= OnTurnBack;
        turnBackAction.Disable();

        openCloseAction.performed -= OnOpenClose;
        openCloseAction.Disable();
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.hoverEntered.RemoveListener(OnHoverEnter);
            grabInteractable.hoverExited.RemoveListener(OnHoverExit);
            grabInteractable.selectEntered.RemoveListener(OnGrabStarted);
            grabInteractable.selectExited.RemoveListener(OnGrabEnded);
        }

        triggerAction?.Dispose();
        turnBackAction?.Dispose();
        openCloseAction?.Dispose();
    }

    // Clear per-frame flags AFTER BookController has had a chance to read them
    private void LateUpdate()
    {
        turnForwardThisFrame = false;
        turnBackwardThisFrame = false;
        toggleOpenThisFrame = false;
    }

    // -------------------------------------------------------------------------
    // Input callbacks
    // -------------------------------------------------------------------------
    private void OnTrigger(InputAction.CallbackContext ctx)
    {
        if (inAnnotations) return;          // hard block regardless of hover
        if (isHovered) turnForwardThisFrame = true;
    }

    private void OnTurnBack(InputAction.CallbackContext ctx)
    {
        if (inAnnotations) return;
        if (isHovered) turnBackwardThisFrame = true;
    }

    private void OnOpenClose(InputAction.CallbackContext ctx)
    {
        if (inAnnotations) return;
        if (isHovered) toggleOpenThisFrame = true;
    }

    // -------------------------------------------------------------------------
    // Grab callbacks
    // -------------------------------------------------------------------------
    private void OnGrabStarted(SelectEnterEventArgs args)
    {
        
        var netState = GetComponent<NetworkBookState>();
        if (netState != null)
            netState.OnLocalGrabStarted();
    }

    private void OnGrabEnded(SelectExitEventArgs args)
    {
        var netState = GetComponent<NetworkBookState>();
        if (netState != null)
            netState.OnLocalGrabReleased();
    }

    // -------------------------------------------------------------------------
    // Hover callbacks
    // -------------------------------------------------------------------------
    private void OnHoverEnter(HoverEnterEventArgs args) => isHovered = true;

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (grabInteractable.interactorsHovering.Count == 0)
            isHovered = false;
    }
}