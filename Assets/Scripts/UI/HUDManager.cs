using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUDManager — In-game heads-up display.
/// Shows energy bar, card hand, enemy energy, selected card name, and level.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Top Bar")]
    public TextMeshProUGUI enemyEnergyText;
    public TextMeshProUGUI playerEnergyText;
    public TextMeshProUGUI selectedCardText;
    public TextMeshProUGUI hudLevelText;

    [Header("Energy Bar")]
    public Image[] energySegments; // 10 segments
    public TextMeshProUGUI energyValueText;
    public Color segmentFillColor = new Color(1f, 0.82f, 0.4f);
    public Color segmentEmptyColor = new Color(0f, 0f, 0f, 0.25f);

    [Header("Hand")]
    public Transform handContainer;
    public GameObject cardUIPrefab;
    public TextMeshProUGUI deckCountText;

    [Header("Exit / Surrender")]
    public Button exitButton;
    public int exitGoldPenalty = 25;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitle;
    public TextMeshProUGUI gameOverRewards;
    public Button returnMenuButton;
    public Button playAgainButton;

    private GameState currentState;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (MatchController.Instance != null)
        {
            MatchController.Instance.OnMatchStart += OnMatchStart;
            MatchController.Instance.OnMatchEnd += OnMatchEnd;
        }

        if (returnMenuButton != null)
            returnMenuButton.onClick.AddListener(OnReturnMenu);
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgain);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitGame);

        // Fix layout: anchor HUD elements to screen edges so they're visible at any aspect ratio
        FixLayout();
    }

    /// <summary>Reposition HUD elements using anchors so they work at any resolution.</summary>
    private void FixLayout()
    {
        // Bottom bar: energy + hand
        AnchorToBottom(playerEnergyText, 0.02f, 0.14f, 0.18f, 0.18f);
        AnchorToBottom(energyValueText,  0.20f, 0.14f, 0.50f, 0.18f);
        AnchorToBottom(deckCountText,    0.75f, 0.14f, 0.98f, 0.18f);
        AnchorToBottom(selectedCardText, 0.20f, 0.10f, 0.80f, 0.14f);

        // Hand container: anchor to bottom of screen
        if (handContainer != null)
        {
            var rt = handContainer.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.05f, 0.0f);
                rt.anchorMax = new Vector2(0.95f, 0.10f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        // Top bar: level + enemy energy + exit button
        AnchorToTop(hudLevelText,    0.02f, 0.92f, 0.15f, 0.98f);
        AnchorToTop(enemyEnergyText, 0.40f, 0.92f, 0.60f, 0.98f);

        // Exit button: top-right corner
        if (exitButton != null)
        {
            var rt = exitButton.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.78f, 0.93f);
                rt.anchorMax = new Vector2(0.98f, 0.98f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }
    }

    private void AnchorToBottom(TMPro.TextMeshProUGUI text, float xMin, float yMin, float xMax, float yMax)
    {
        if (text == null) return;
        var rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void AnchorToTop(TMPro.TextMeshProUGUI text, float xMin, float yMin, float xMax, float yMax)
    {
        if (text == null) return;
        var rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (MatchController.Instance != null)
        {
            MatchController.Instance.OnMatchStart -= OnMatchStart;
            MatchController.Instance.OnMatchEnd -= OnMatchEnd;
        }
    }

    private void OnMatchStart()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        gameObject.SetActive(true);
    }

    // ---- Called every frame by MatchController ----
    public void UpdateHUD(GameState state)
    {
        currentState = state;

        // Energy texts
        if (playerEnergyText != null)
            playerEnergyText.text = state.player.energy.ToString("F1");
        if (enemyEnergyText != null)
            enemyEnergyText.text = state.enemy.energy.ToString("F1");
        if (energyValueText != null)
            energyValueText.text = $"{state.player.energy:F1} / {state.player.maxEnergy}";

        // Energy bar segments
        if (energySegments != null)
        {
            int filled = Mathf.FloorToInt(state.player.energy + 0.000001f);
            for (int i = 0; i < energySegments.Length; i++)
            {
                if (energySegments[i] != null)
                    energySegments[i].color = i < filled ? segmentFillColor : segmentEmptyColor;
            }
        }

        // Level
        if (hudLevelText != null)
            hudLevelText.text = GameManager.Instance.CurrentLevel.ToString();

        // Selected card name
        if (selectedCardText != null && state.player.hand.Count > 0)
        {
            var card = state.player.hand[state.player.selectedIndex];
            selectedCardText.text = card.name;
        }

        // Deck count
        if (deckCountText != null)
            deckCountText.text = $"Deck: {state.player.deckQueue.Count}";
    }

    // ---- Hand UI ----
    public void RefreshHand(GameState state)
    {
        if (handContainer == null || cardUIPrefab == null) return;

        // Clear old
        foreach (Transform child in handContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < state.player.hand.Count; i++)
        {
            var card = state.player.hand[i];
            var go = Instantiate(cardUIPrefab, handContainer);

            var cardUI = go.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(card, i, i == state.player.selectedIndex);
                int capturedIndex = i;
                cardUI.OnClicked += () => OnCardClicked(capturedIndex);
            }
        }
    }

    private void OnCardClicked(int index)
    {
        if (currentState == null) return;
        currentState.player.selectedIndex = index;
        RefreshHand(currentState);

        if (selectedCardText != null && currentState.player.hand.Count > index)
            selectedCardText.text = currentState.player.hand[index].name;
    }

    // ---- Game Over ----
    private void OnMatchEnd(bool playerWon)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);

        if (gameOverTitle != null)
            gameOverTitle.text = playerWon ? "Victory!" : "Defeat!";

        GameManager.Instance.GetLevelProgress(out int level, out _, out _);
        int xpGain = playerWon ? Mathf.FloorToInt(45 + level * 6) : Mathf.FloorToInt(18 + level * 3);
        int goldGain = playerWon ? Mathf.FloorToInt(60 + level * 10) : 0;

        if (gameOverRewards != null)
        {
            gameOverRewards.text = playerWon
                ? $"You gained +{xpGain} XP and +{goldGain} Gold."
                : $"You gained +{xpGain} XP. (Gold only on wins)";
        }
    }

    private void OnExitGame()
    {
        // Apply gold penalty (never go below 0)
        if (GameManager.Instance != null)
        {
            int currentGold = GameManager.Instance.gold;
            int penalty = Mathf.Min(exitGoldPenalty, currentGold);
            if (penalty > 0)
                GameManager.Instance.gold = currentGold - penalty;
            GameManager.Instance.Save();
        }

        // Force-end the match as a loss (no XP/gold rewards)
        if (MatchController.Instance != null)
            MatchController.Instance.ForceEndMatch();

        // Hide exit button and game over panel, go back to menu
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        foreach (var menu in FindObjectsByType<MenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            menu.gameObject.SetActive(true);
            menu.ShowMainMenu();
        }
        gameObject.SetActive(false);
    }

    private void OnReturnMenu()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Re-enable ALL MenuUI objects and show main menu
        foreach (var menu in FindObjectsByType<MenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            menu.gameObject.SetActive(true);
            menu.ShowMainMenu();
        }
        gameObject.SetActive(false);
    }

    private void OnPlayAgain()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        MatchController.Instance?.StartMatch();
    }
}
