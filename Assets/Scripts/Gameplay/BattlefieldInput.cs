using UnityEngine;

/// <summary>
/// BattlefieldInput — Handles tap/click on the battlefield.
/// Determines which lane was tapped and tells MatchController to play the selected card.
/// </summary>
public class BattlefieldInput : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (MatchController.Instance == null) return;
        if (MatchController.Instance.State.isGameOver) return;

        // Mouse / touch
        if (Input.GetMouseButtonDown(0))
        {
            HandleTap(Input.mousePosition);
        }

        // Mobile touch
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleTap(Input.GetTouch(0).position);
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
