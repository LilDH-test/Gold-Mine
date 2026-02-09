using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// BattlefieldInput — Handles tap/click on the battlefield.
/// Determines which lane was tapped and tells MatchController to play the selected card.
/// Uses the new Input System package.
/// </summary>
public class BattlefieldInput : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        EnhancedTouchSupport.Enable();
    }

    private void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (MatchController.Instance == null) return;
        if (MatchController.Instance.State.isGameOver) return;

        // Mouse click (new Input System)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleTap(Mouse.current.position.ReadValue());
        }

        // Touch (new Input System)
        if (Touch.activeFingers.Count > 0 && Touch.activeFingers[0].currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            HandleTap(Touch.activeFingers[0].currentTouch.screenPosition);
        }
    }

    private void HandleTap(Vector2 screenPos)
    {
        if (mainCam == null) return;

        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));

        // Determine lane: left half = lane 0, right half = lane 1
        int lane = worldPos.x < 0f ? 0 : 1;

        MatchController.Instance.PlayerPlayCard(lane);
    }
}
