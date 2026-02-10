using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MenuUI — Main menu, shop, settings, and credits screens.
/// Manages screen transitions and button callbacks.
/// </summary>
public class MenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject shopPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject hudObject; // the HUD + battlefield

    [Header("Main Menu - Meta Info")]
    public TextMeshProUGUI mmLevelText;
    public TextMeshProUGUI mmXPText;
    public TextMeshProUGUI mmGoldText;

    [Header("Main Menu - Buttons")]
    public Button btnPlayOnline;
    public Button btnPlayAI;
    public Button btnShop;
    public Button btnSettings;
    public Button btnCredits;

    [Header("Shop")]
    public TextMeshProUGUI shopLevelText;
    public TextMeshProUGUI shopGoldText;
    public Transform shopGrid;
    public GameObject shopItemPrefab;
    public Button btnShopBack;
    public TextMeshProUGUI shopHintText;

    [Header("Settings")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Button btnSettingsBack;

    [Header("Credits")]
    public Button btnCreditsBack;

    [Header("LAN")]
    public LanLobbyUI lanLobbyUI;

    private void Start()
    {
        // Ensure core singletons exist
        EnsureCoreObjects();

        // Wire up buttons
        btnPlayOnline?.onClick.AddListener(OnPlayOnline);
        btnPlayAI?.onClick.AddListener(OnPlayAI);
        btnShop?.onClick.AddListener(OnShop);
        btnSettings?.onClick.AddListener(OnSettings);
        btnCredits?.onClick.AddListener(OnCredits);

        btnShopBack?.onClick.AddListener(ShowMainMenu);
        btnSettingsBack?.onClick.AddListener(ShowMainMenu);
        btnCreditsBack?.onClick.AddListener(ShowMainMenu);

        // Settings sliders
        if (musicSlider != null)
        {
            musicSlider.value = GameManager.Instance.musicVolume;
            musicSlider.onValueChanged.AddListener(v =>
            {
                GameManager.Instance.musicVolume = v;
                GameManager.Instance.SaveGame();
            });
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = GameManager.Instance.sfxVolume;
            sfxSlider.onValueChanged.AddListener(v =>
            {
                GameManager.Instance.sfxVolume = v;
                GameManager.Instance.SaveGame();
            });
        }

        ShowMainMenu();
    }

    // ---- Screen transitions ----
    public void ShowMainMenu()
    {
        SetScreen(mainMenuPanel);
        RefreshMetaUI();
    }

    private void SetScreen(GameObject activeScreen)
    {
        mainMenuPanel?.SetActive(activeScreen == mainMenuPanel);
        shopPanel?.SetActive(activeScreen == shopPanel);
        settingsPanel?.SetActive(activeScreen == settingsPanel);
        creditsPanel?.SetActive(activeScreen == creditsPanel);

        // Hide LAN lobby when switching to other screens
        if (lanLobbyUI != null && activeScreen != null)
            lanLobbyUI.gameObject.SetActive(false);

        // When going to gameplay (activeScreen == null), hide the ENTIRE menu
        // and show the HUD. When returning to menus, show menu and hide HUD.
        if (activeScreen == null)
        {
            // Hide ALL MenuUI objects in scene (handles duplicates)
            foreach (var menu in FindObjectsByType<MenuUI>(FindObjectsSortMode.None))
                menu.gameObject.SetActive(false);

            // Show HUD
            if (hudObject != null)
                hudObject.SetActive(true);
            else
            {
                var hud = FindAnyObjectByType<HUDManager>(FindObjectsInactive.Include);
                if (hud != null) hud.gameObject.SetActive(true);
            }
        }
        else
        {
            // Show this menu, hide HUD
            gameObject.SetActive(true);
            if (hudObject != null)
                hudObject.SetActive(false);
        }
    }

    private void RefreshMetaUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.GetLevelProgress(out int level, out int xpInto, out int xpNeeded);

        if (mmLevelText != null) mmLevelText.text = level.ToString();
        if (mmXPText != null) mmXPText.text = $"{xpInto} / {xpNeeded}";
        if (mmGoldText != null) mmGoldText.text = gm.gold.ToString();

        if (shopLevelText != null) shopLevelText.text = level.ToString();
        if (shopGoldText != null) shopGoldText.text = gm.gold.ToString();
    }

    // ---- Button handlers ----
    private void OnPlayOnline()
    {
        // Open LAN lobby
        if (lanLobbyUI != null)
        {
            SetScreen(null); // hide menu panels
            mainMenuPanel?.SetActive(false);
            lanLobbyUI.ShowLobby();
        }
        else
        {
            Debug.Log("LAN lobby UI not assigned.");
        }
    }

    private void OnPlayAI()
    {
        // Hide all menu panels
        SetScreen(null);

        // Create MatchController if it doesn't exist
        if (MatchController.Instance == null)
        {
            Debug.Log("Creating MatchController at play time...");
            var mcGO = new GameObject("MatchController");
            var mc = mcGO.AddComponent<MatchController>();
            var ai = mcGO.AddComponent<AIController>();
            mcGO.AddComponent<BattlefieldInput>();
            mc.ai = ai;

            var hudMgr = FindAnyObjectByType<HUDManager>();
            if (hudMgr != null)
            {
                mc.hud = hudMgr;
                if (hudObject == null) hudObject = hudMgr.gameObject;
            }
        }

        // Start match
        if (MatchController.Instance != null)
        {
            MatchController.Instance.StartMatch();
        }
        else
        {
            Debug.LogError("Failed to create MatchController!");
        }
    }

    private void OnShop()
    {
        SetScreen(shopPanel);
        RefreshMetaUI();
        RenderShop();
    }

    private void OnSettings()
    {
        SetScreen(settingsPanel);
    }

    private void OnCredits()
    {
        SetScreen(creditsPanel);
    }

    // ---- Shop ----
    private void RenderShop()
    {
        if (shopGrid == null || shopItemPrefab == null) return;

        // Clear old items
        foreach (Transform child in shopGrid)
            Destroy(child.gameObject);

        var gm = GameManager.Instance;
        int level = gm.CurrentLevel;

        var allCards = new System.Collections.Generic.List<CardDatabase.CardDef>(CardDatabase.AllCards);
        allCards.Sort((a, b) =>
        {
            if (a.unlockLevel != b.unlockLevel) return a.unlockLevel - b.unlockLevel;
            return a.cost - b.cost;
        });

        foreach (var card in allCards)
        {
            bool owned = gm.IsCardOwned(card.id);
            bool locked = level < card.unlockLevel;

            var go = Instantiate(shopItemPrefab, shopGrid);
            var item = go.GetComponent<ShopItemUI>();
            if (item != null)
            {
                item.Setup(card, owned, locked, () => TryBuyCard(card));
            }
        }
    }

    private void TryBuyCard(CardDatabase.CardDef card)
    {
        var gm = GameManager.Instance;

        if (gm.IsCardOwned(card.id)) return;
        if (gm.CurrentLevel < card.unlockLevel) return;

        if (card.priceGold <= 0)
        {
            gm.OwnCard(card.id);
            RefreshMetaUI();
            RenderShop();
            return;
        }

        if (!gm.CanAfford(card.priceGold))
        {
            if (shopHintText != null) shopHintText.text = "Not enough Gold.";
            return;
        }

        gm.SpendGold(card.priceGold);
        gm.OwnCard(card.id);
        RefreshMetaUI();
        RenderShop();

        if (shopHintText != null) shopHintText.text = "Purchased!";
    }

    // ---- Auto-create missing core objects so the game works even without running Setup Scene ----
    private void EnsureCoreObjects()
    {
        try
        {
            // GameManager
            if (GameManager.Instance == null)
            {
                var gmGO = new GameObject("GameManager");
                gmGO.AddComponent<GameManager>();
                Debug.Log("Auto-created GameManager");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create GameManager: {e}");
        }

        try
        {
            // MatchController
            if (MatchController.Instance == null)
            {
                var mcGO = new GameObject("MatchController");
                var mc = mcGO.AddComponent<MatchController>();
                var ai = mcGO.AddComponent<AIController>();
                mcGO.AddComponent<BattlefieldInput>();
                mc.ai = ai;

                // Wire HUD
                var hudMgr = FindAnyObjectByType<HUDManager>();
                if (hudMgr != null)
                {
                    mc.hud = hudMgr;
                    if (hudObject == null) hudObject = hudMgr.gameObject;
                }

                Debug.Log($"Auto-created MatchController. Instance={MatchController.Instance != null}");
            }
            else
            {
                // Ensure HUD is wired
                var mc = MatchController.Instance;
                if (mc.hud == null)
                {
                    mc.hud = FindAnyObjectByType<HUDManager>();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create MatchController: {e}");
        }
    }
}
