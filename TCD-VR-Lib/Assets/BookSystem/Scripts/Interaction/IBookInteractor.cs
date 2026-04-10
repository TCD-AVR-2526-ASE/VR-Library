/// <summary>
/// Abstraction for book interaction input.
/// Implement this for each input mode (desktop mouse/keyboard, VR controllers).
/// Drag-to-turn is not supported — page turns use discrete button/key presses only.
/// </summary>
public interface IBookInteractor
{
    bool TurnPageForward { get; }
    bool TurnPageBackward { get; }
    bool ToggleOpen { get; }
}
