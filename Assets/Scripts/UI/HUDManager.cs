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

    private void OnReturnMenu()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Switch to menu UI
        MenuUI menu = FindAnyObjectByType<MenuUI>();
        if (menu != null) menu.ShowMainMenu();
        gameObject.SetActive(false);
    }

    private void OnPlayAgain()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        MatchController.Instance?.StartMatch();
    }
}
