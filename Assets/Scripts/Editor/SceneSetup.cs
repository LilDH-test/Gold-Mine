using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script — Run from menu: Gold Mine > Setup Scene
/// Creates all GameObjects, prefabs, UI, and wires everything together.
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [MenuItem("Gold Mine/Setup Scene")]
    public static void SetupScene()
    {
        // ========================================
        // 0. CLEAN UP OLD OBJECTS FROM PREVIOUS RUNS
        // ========================================
        CleanupOldObjects();

        // ========================================
        // 1. CORE GAME OBJECTS
        // ========================================

        // GameManager (singleton, persists between scenes)
        var gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();
        Undo.RegisterCreatedObjectUndo(gmGO, "Create GameManager");

        // MatchController
        var mcGO = new GameObject("MatchController");
        var mc = mcGO.AddComponent<MatchController>();
        mcGO.AddComponent<AIController>();
        mcGO.AddComponent<BattlefieldInput>();
        mcGO.AddComponent<LanManager>();
        mcGO.AddComponent<LanMatchHandler>();
        mc.ai = mcGO.GetComponent<AIController>();
        Undo.RegisterCreatedObjectUndo(mcGO, "Create MatchController");

        // ========================================
        // 2. CAMERA SETUP
        // ========================================

        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = GameConstants.ARENA_HEIGHT * 0.55f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.14f); // dark retro blue
        }

        // ========================================
        // 3. ARENA BACKGROUND
        // ========================================

        var arena = new GameObject("Arena");
        var arenaSR = arena.AddComponent<SpriteRenderer>();
        arenaSR.color = new Color(0.09f, 0.13f, 0.24f);
        arena.transform.position = Vector3.zero;
        arena.transform.localScale = new Vector3(GameConstants.ARENA_WIDTH, GameConstants.ARENA_HEIGHT, 1f);
        // Create a simple white sprite for the background
        arenaSR.sprite = CreateWhiteSprite();
        arenaSR.sortingOrder = -100;
        Undo.RegisterCreatedObjectUndo(arena, "Create Arena");

        // Lane divider
        var divider = new GameObject("LaneDivider");
        var divSR = divider.AddComponent<SpriteRenderer>();
        divSR.sprite = CreateWhiteSprite();
        divSR.color = new Color(1f, 0.82f, 0.4f, 0.15f);
        divider.transform.position = new Vector3(0, 0, 0);
        divider.transform.localScale = new Vector3(0.05f, GameConstants.ARENA_HEIGHT, 1f);
        divSR.sortingOrder = -99;
        divider.transform.SetParent(arena.transform);
        
        // Midline (river)
        var midline = new GameObject("Midline");
        var midSR = midline.AddComponent<SpriteRenderer>();
        midSR.sprite = CreateWhiteSprite();
        midSR.color = new Color(0.2f, 0.5f, 0.8f, 0.3f);
        midline.transform.position = new Vector3(0, 0, 0);
        midline.transform.localScale = new Vector3(GameConstants.ARENA_WIDTH, 0.15f, 1f);
        midSR.sortingOrder = -98;
        midline.transform.SetParent(arena.transform);

        // ========================================
        // 4. CREATE PREFABS
        // ========================================

        // Ensure Prefabs folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Tower Prefab
        var towerPrefab = CreateEntityPrefab("TowerPrefab", new Color(0.4f, 0.6f, 0.9f), 
            new Vector2(0.8f, 1.0f), typeof(TowerController), typeof(Damageable));
        mc.towerPrefab = towerPrefab;

        // King Prefab
        var kingPrefab = CreateEntityPrefab("KingPrefab", new Color(1f, 0.82f, 0.4f), 
            new Vector2(1.0f, 1.2f), typeof(TowerController), typeof(Damageable));
        mc.kingPrefab = kingPrefab;

        // Unit Prefab
        var unitPrefab = CreateEntityPrefab("UnitPrefab", new Color(0.5f, 0.9f, 0.5f), 
            new Vector2(0.5f, 0.5f), typeof(UnitController), typeof(Damageable));
        mc.unitPrefab = unitPrefab;

        // Pump Prefab
        var pumpPrefab = CreateEntityPrefab("PumpPrefab", new Color(0.9f, 0.6f, 0.2f), 
            new Vector2(0.6f, 0.8f), typeof(PumpController), typeof(Damageable));
        mc.pumpPrefab = pumpPrefab;

        // Projectile Prefab
        var projPrefab = CreateEntityPrefab("ProjectilePrefab", Color.white, 
            new Vector2(0.15f, 0.15f), typeof(ProjectileController));
        mc.projectilePrefab = projPrefab;

        // ========================================
        // 5. UI CANVAS
        // ========================================

        // Main Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");

        // EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
        }

        // ---- MENU UI ----
        var menuUI = CreateMenuUI(canvasGO.transform);
        var menuComp = menuUI.GetComponent<MenuUI>();

        // ---- HUD ----
        var hudUI = CreateHUD(canvasGO.transform);
        var hudComp = hudUI.GetComponent<HUDManager>();
        mc.hud = hudComp;
        if (menuComp != null) menuComp.hudObject = hudUI;

        // ---- LAN LOBBY ----
        var lanUI = CreateLanLobbyUI(canvasGO.transform);
        var lanComp = lanUI.GetComponent<LanLobbyUI>();
        if (menuComp != null) menuComp.lanLobbyUI = lanComp;
        var lmh = mcGO.GetComponent<LanMatchHandler>();
        if (lanComp != null) lanComp.lanMatchHandler = lmh;

        // ---- Card UI Prefab ----
        var cardUIPrefab = CreateCardUIPrefab();
        if (hudComp != null) hudComp.cardUIPrefab = cardUIPrefab;

        // Mark scene dirty (only outside play mode)
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        Debug.Log("✅ Gold Mine scene setup complete! Press Play to test.");
        EditorUtility.DisplayDialog("Gold Mine Setup", 
            "Scene setup complete!\n\n" +
            "• GameManager, MatchController, AI, LAN created\n" +
            "• Prefabs created in Assets/Prefabs/\n" +
            "• Full UI with Menu, HUD, LAN Lobby\n" +
            "• Camera configured for arena\n\n" +
            "Press Play to test!", "OK");
    }

    // ========================================
    //  HELPERS
    // ========================================

    private static Sprite CreateWhiteSprite()
    {
        var tex = new Texture2D(4, 4);
        var colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    private static GameObject CreateEntityPrefab(string name, Color color, Vector2 size, params System.Type[] components)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = color;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        foreach (var comp in components)
            if (go.GetComponent(comp) == null)
                go.AddComponent(comp);

        string path = $"Assets/Prefabs/{name}.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateMenuUI(Transform canvasTransform)
    {
        var menuRoot = new GameObject("MenuUI");
        menuRoot.transform.SetParent(canvasTransform, false);
        var menuComp = menuRoot.AddComponent<MenuUI>();
        var menuRT = menuRoot.AddComponent<RectTransform>();
        StretchFull(menuRT);

        // ---- Main Menu Panel ----
        var mainPanel = CreatePanel("MainMenuPanel", menuRoot.transform, new Color(0.06f, 0.08f, 0.14f, 0.97f));
        menuComp.mainMenuPanel = mainPanel;

        // Title
        CreateText("GoldMineTitle", mainPanel.transform, "GOLD MINE", 42,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 300));

        // Subtitle
        CreateText("Subtitle", mainPanel.transform, "Tower Defense Card Game", 18,
            new Color(0.7f, 0.7f, 0.7f), new Vector2(0, 240));

        // Level / XP / Gold display
        var metaGroup = new GameObject("MetaInfo");
        metaGroup.transform.SetParent(mainPanel.transform, false);
        var metaRT = metaGroup.AddComponent<RectTransform>();
        metaRT.anchoredPosition = new Vector2(0, 140);

        menuComp.mmLevelText = CreateText("LevelText", metaGroup.transform, "Level: 1", 20,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 30)).GetComponent<TextMeshProUGUI>();
        menuComp.mmXPText = CreateText("XPText", metaGroup.transform, "XP: 0 / 120", 16,
            Color.white, new Vector2(0, 0)).GetComponent<TextMeshProUGUI>();
        menuComp.mmGoldText = CreateText("GoldText", metaGroup.transform, "Gold: 0", 16,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, -30)).GetComponent<TextMeshProUGUI>();

        // Buttons
        menuComp.btnPlayOnline = CreateButton("Play Online (LAN)", mainPanel.transform,
            new Vector2(0, 20), new Color(0.15f, 0.4f, 0.15f));
        menuComp.btnPlayAI = CreateButton("Play vs AI", mainPanel.transform,
            new Vector2(0, -60), new Color(0.12f, 0.12f, 0.35f));
        menuComp.btnShop = CreateButton("Shop", mainPanel.transform,
            new Vector2(0, -140), new Color(0.35f, 0.25f, 0.1f));
        menuComp.btnSettings = CreateButton("Settings", mainPanel.transform,
            new Vector2(0, -220), new Color(0.2f, 0.2f, 0.2f));
        menuComp.btnCredits = CreateButton("Credits", mainPanel.transform,
            new Vector2(0, -300), new Color(0.2f, 0.2f, 0.2f));

        // ---- Shop Panel ----
        var shopPanel = CreatePanel("ShopPanel", menuRoot.transform, new Color(0.06f, 0.08f, 0.14f, 0.97f));
        shopPanel.SetActive(false);
        menuComp.shopPanel = shopPanel;

        CreateText("ShopTitle", shopPanel.transform, "SHOP", 36,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 380));
        menuComp.shopLevelText = CreateText("ShopLevel", shopPanel.transform, "Level: 1", 18,
            Color.white, new Vector2(-200, 330)).GetComponent<TextMeshProUGUI>();
        menuComp.shopGoldText = CreateText("ShopGold", shopPanel.transform, "Gold: 0", 18,
            new Color(1f, 0.82f, 0.4f), new Vector2(200, 330)).GetComponent<TextMeshProUGUI>();

        // Shop scroll area
        var shopScroll = CreateScrollView("ShopGrid", shopPanel.transform, new Vector2(0, -40), new Vector2(900, 600));
        menuComp.shopGrid = shopScroll.transform;

        menuComp.shopHintText = CreateText("ShopHint", shopPanel.transform, "", 16,
            Color.yellow, new Vector2(0, -380)).GetComponent<TextMeshProUGUI>();
        menuComp.btnShopBack = CreateButton("Back", shopPanel.transform,
            new Vector2(0, -430), new Color(0.3f, 0.15f, 0.15f));

        // ---- Settings Panel ----
        var settingsPanel = CreatePanel("SettingsPanel", menuRoot.transform, new Color(0.06f, 0.08f, 0.14f, 0.97f));
        settingsPanel.SetActive(false);
        menuComp.settingsPanel = settingsPanel;

        CreateText("SettingsTitle", settingsPanel.transform, "SETTINGS", 36,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 300));
        CreateText("MusicLabel", settingsPanel.transform, "Music Volume", 18,
            Color.white, new Vector2(0, 100));
        menuComp.musicSlider = CreateSlider("MusicSlider", settingsPanel.transform, new Vector2(0, 50));
        CreateText("SfxLabel", settingsPanel.transform, "SFX Volume", 18,
            Color.white, new Vector2(0, -50));
        menuComp.sfxSlider = CreateSlider("SfxSlider", settingsPanel.transform, new Vector2(0, -100));
        menuComp.btnSettingsBack = CreateButton("Back", settingsPanel.transform,
            new Vector2(0, -300), new Color(0.3f, 0.15f, 0.15f));

        // ---- Credits Panel ----
        var creditsPanel = CreatePanel("CreditsPanel", menuRoot.transform, new Color(0.06f, 0.08f, 0.14f, 0.97f));
        creditsPanel.SetActive(false);
        menuComp.creditsPanel = creditsPanel;

        CreateText("CreditsTitle", creditsPanel.transform, "CREDITS", 36,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 300));
        CreateText("CreditsBody", creditsPanel.transform,
            "Gold Mine\nA Tower Defense Card Game\n\nDeveloped for TSA Competition\n\nBuilt with Unity",
            18, Color.white, new Vector2(0, 100));
        menuComp.btnCreditsBack = CreateButton("Back", creditsPanel.transform,
            new Vector2(0, -300), new Color(0.3f, 0.15f, 0.15f));

        return menuRoot;
    }

    private static GameObject CreateHUD(Transform canvasTransform)
    {
        var hudRoot = new GameObject("HUD");
        hudRoot.transform.SetParent(canvasTransform, false);
        var hudComp = hudRoot.AddComponent<HUDManager>();
        var hudRT = hudRoot.AddComponent<RectTransform>();
        StretchFull(hudRT);
        hudRoot.SetActive(false);

        // Energy display (bottom)
        hudComp.energyValueText = CreateText("EnergyValue", hudRoot.transform,
            "5.0 / 10", 22, new Color(1f, 0.82f, 0.4f), new Vector2(0, -780))
            .GetComponent<TextMeshProUGUI>();

        hudComp.playerEnergyText = CreateText("PlayerEnergy", hudRoot.transform,
            "5.0", 18, new Color(1f, 0.82f, 0.4f), new Vector2(-350, -780))
            .GetComponent<TextMeshProUGUI>();

        // Selected card name
        hudComp.selectedCardText = CreateText("SelectedCard", hudRoot.transform,
            "Select a card", 16, Color.white, new Vector2(0, -820))
            .GetComponent<TextMeshProUGUI>();

        // Hand container
        var handContainer = new GameObject("HandContainer");
        handContainer.transform.SetParent(hudRoot.transform, false);
        var handRT = handContainer.AddComponent<RectTransform>();
        handRT.anchoredPosition = new Vector2(0, -880);
        handRT.sizeDelta = new Vector2(900, 120);
        var handLayout = handContainer.AddComponent<HorizontalLayoutGroup>();
        handLayout.spacing = 10;
        handLayout.childAlignment = TextAnchor.MiddleCenter;
        handLayout.childForceExpandWidth = false;
        handLayout.childForceExpandHeight = false;
        hudComp.handContainer = handContainer.transform;

        // Deck count
        hudComp.deckCountText = CreateText("DeckCount", hudRoot.transform,
            "Deck: 0", 14, new Color(0.7f, 0.7f, 0.7f), new Vector2(400, -780))
            .GetComponent<TextMeshProUGUI>();

        // Level display
        hudComp.hudLevelText = CreateText("HUDLevel", hudRoot.transform,
            "1", 16, new Color(1f, 0.82f, 0.4f), new Vector2(-400, 850))
            .GetComponent<TextMeshProUGUI>();

        // Enemy energy (top)
        hudComp.enemyEnergyText = CreateText("EnemyEnergy", hudRoot.transform,
            "5.0", 16, new Color(1f, 0.4f, 0.4f), new Vector2(0, 850))
            .GetComponent<TextMeshProUGUI>();

        // Exit Game button (top-right, visible during gameplay)
        hudComp.exitButton = CreateButton("Exit Game", hudRoot.transform,
            new Vector2(350, 850), new Color(0.5f, 0.12f, 0.12f));

        // ---- Game Over Panel ----
        var goPanel = CreatePanel("GameOverPanel", hudRoot.transform, new Color(0f, 0f, 0f, 0.85f));
        goPanel.SetActive(false);
        hudComp.gameOverPanel = goPanel;

        hudComp.gameOverTitle = CreateText("GOTitle", goPanel.transform, "Victory!", 48,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 100)).GetComponent<TextMeshProUGUI>();
        hudComp.gameOverRewards = CreateText("GORewards", goPanel.transform,
            "+45 XP  +60 Gold", 20, Color.white, new Vector2(0, 0)).GetComponent<TextMeshProUGUI>();
        hudComp.playAgainButton = CreateButton("Play Again", goPanel.transform,
            new Vector2(0, -100), new Color(0.12f, 0.35f, 0.12f));
        hudComp.returnMenuButton = CreateButton("Main Menu", goPanel.transform,
            new Vector2(0, -180), new Color(0.3f, 0.15f, 0.15f));

        return hudRoot;
    }

    private static GameObject CreateLanLobbyUI(Transform canvasTransform)
    {
        var lanRoot = new GameObject("LanLobbyUI");
        lanRoot.transform.SetParent(canvasTransform, false);
        var lanComp = lanRoot.AddComponent<LanLobbyUI>();
        var lanRT = lanRoot.AddComponent<RectTransform>();
        StretchFull(lanRT);
        lanRoot.SetActive(false);

        // ---- Lobby Panel (Host/Join choice) ----
        var lobbyPanel = CreatePanel("LobbyPanel", lanRoot.transform, new Color(0.06f, 0.08f, 0.14f, 0.97f));
        lanComp.lobbyPanel = lobbyPanel;

        CreateText("LANTitle", lobbyPanel.transform, "LAN MULTIPLAYER", 32,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 200));
        lanComp.btnHost = CreateButton("Host Game", lobbyPanel.transform,
            new Vector2(0, 50), new Color(0.15f, 0.4f, 0.15f));
        lanComp.btnJoin = CreateButton("Join Game", lobbyPanel.transform,
            new Vector2(0, -50), new Color(0.12f, 0.12f, 0.35f));
        lanComp.btnBack = CreateButton("Back", lobbyPanel.transform,
            new Vector2(0, -200), new Color(0.3f, 0.15f, 0.15f));

        // ---- Host Wait Panel ----
        var hostPanel = CreatePanel("HostWaitPanel", lanRoot.transform, new Color(0.06f, 0.08f, 0.14f, 0.97f));
        hostPanel.SetActive(false);
        lanComp.hostWaitPanel = hostPanel;

        CreateText("HostTitle", hostPanel.transform, "HOSTING", 32,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 200));
        lanComp.hostIPText = CreateText("HostIP", hostPanel.transform, "Your IP: ...", 20,
            Color.white, new Vector2(0, 100)).GetComponent<TextMeshProUGUI>();
        lanComp.hostStatusText = CreateText("HostStatus", hostPanel.transform,
            "Waiting for player...", 18, new Color(0.7f, 0.7f, 0.7f),
            new Vector2(0, 40)).GetComponent<TextMeshProUGUI>();
        lanComp.btnStartMatch = CreateButton("Start Match", hostPanel.transform,
            new Vector2(0, -80), new Color(0.15f, 0.4f, 0.15f));
        lanComp.btnCancelHost = CreateButton("Cancel", hostPanel.transform,
            new Vector2(0, -180), new Color(0.3f, 0.15f, 0.15f));

        // ---- Join Scan Panel ----
        var joinPanel = CreatePanel("JoinScanPanel", lanRoot.transform, new Color(0.06f, 0.08f, 0.14f, 0.97f));
        joinPanel.SetActive(false);
        lanComp.joinScanPanel = joinPanel;

        CreateText("JoinTitle", joinPanel.transform, "FIND HOST", 32,
            new Color(1f, 0.82f, 0.4f), new Vector2(0, 300));
        lanComp.scanStatusText = CreateText("ScanStatus", joinPanel.transform,
            "Scanning...", 18, new Color(0.7f, 0.7f, 0.7f),
            new Vector2(0, 230)).GetComponent<TextMeshProUGUI>();

        // Server list
        var serverScroll = CreateScrollView("ServerList", joinPanel.transform, new Vector2(0, 50), new Vector2(700, 250));
        lanComp.serverListContent = serverScroll.transform;

        // Manual IP
        CreateText("ManualLabel", joinPanel.transform, "Or enter IP:", 16,
            Color.white, new Vector2(-130, -130));
        lanComp.manualIPInput = CreateInputField("IPInput", joinPanel.transform,
            new Vector2(80, -130), "127.0.0.1");
        lanComp.btnManualConnect = CreateButton("Connect", joinPanel.transform,
            new Vector2(0, -210), new Color(0.12f, 0.35f, 0.12f));
        lanComp.btnCancelJoin = CreateButton("Cancel", joinPanel.transform,
            new Vector2(0, -300), new Color(0.3f, 0.15f, 0.15f));

        // ---- Connected Panel ----
        var connPanel = CreatePanel("ConnectedPanel", lanRoot.transform, new Color(0.06f, 0.08f, 0.14f, 0.97f));
        connPanel.SetActive(false);
        lanComp.connectedPanel = connPanel;

        lanComp.connectedText = CreateText("ConnectedText", connPanel.transform,
            "Connected! Waiting for host...", 22, new Color(0.5f, 1f, 0.5f),
            new Vector2(0, 0)).GetComponent<TextMeshProUGUI>();

        return lanRoot;
    }

    private static GameObject CreateCardUIPrefab()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var go = new GameObject("CardUIItem");
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 100);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.12f, 0.18f, 0.92f);

        var btn = go.AddComponent<Button>();
        var cardUI = go.AddComponent<CardUI>();
        cardUI.background = bg;
        cardUI.button = btn;

        // Name text
        var nameGO = CreateText("CardName", go.transform, "Card", 14,
            Color.white, new Vector2(0, 20));
        cardUI.nameText = nameGO.GetComponent<TextMeshProUGUI>();

        // Type text
        var typeGO = CreateText("CardType", go.transform, "Unit", 10,
            new Color(0.7f, 0.7f, 0.7f), new Vector2(-40, -15));
        cardUI.typeText = typeGO.GetComponent<TextMeshProUGUI>();

        // Cost text
        var costGO = CreateText("CardCost", go.transform, "⚡ 3", 14,
            new Color(1f, 0.82f, 0.4f), new Vector2(50, -15));
        cardUI.costText = costGO.GetComponent<TextMeshProUGUI>();

        // Selection border
        var borderGO = new GameObject("SelectBorder");
        borderGO.transform.SetParent(go.transform, false);
        var borderRT = borderGO.AddComponent<RectTransform>();
        StretchFull(borderRT);
        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(1f, 0.82f, 0.4f, 0.85f);
        // Make it an outline by keeping it behind
        borderRT.offsetMin = new Vector2(-3, -3);
        borderRT.offsetMax = new Vector2(3, 3);
        borderGO.transform.SetAsFirstSibling();
        cardUI.selectionBorder = borderImg;
        borderGO.SetActive(false);

        // Add layout element for sizing in horizontal layout
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 200;
        le.preferredHeight = 100;

        string path = "Assets/Prefabs/CardUIItem.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ========================================
    //  UI BUILDER UTILITIES
    // ========================================

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        StretchFull(rt);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static GameObject CreateText(string name, Transform parent, string text, int fontSize,
        Color color, Vector2 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(800, 50);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = false;

        return go;
    }

    private static Button CreateButton(string label, Transform parent, Vector2 position, Color bgColor)
    {
        var go = new GameObject($"Btn_{label}");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(400, 60);

        var bg = go.AddComponent<Image>();
        bg.color = bgColor;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;

        // Label
        CreateText("Label", go.transform, label, 20, Color.white, Vector2.zero);

        return btn;
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(400, 30);

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;

        // Background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(go.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        StretchFull(bgRT);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);

        // Fill area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        StretchFull(faRT);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        var fillRT = fillGO.AddComponent<RectTransform>();
        StretchFull(fillRT);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(1f, 0.82f, 0.4f);
        slider.fillRect = fillRT;

        // Handle
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        var haRT = handleArea.AddComponent<RectTransform>();
        StretchFull(haRT);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var hRT = handle.AddComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(20, 30);
        var hImg = handle.AddComponent<Image>();
        hImg.color = Color.white;
        slider.handleRect = hRT;

        return slider;
    }

    private static Transform CreateScrollView(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Content container with vertical layout
        var content = new GameObject("Content");
        content.transform.SetParent(go.transform, false);
        var cRT = content.AddComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 1);
        cRT.anchorMax = new Vector2(1, 1);
        cRT.pivot = new Vector2(0.5f, 1);
        cRT.sizeDelta = new Vector2(0, 0);

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return content.transform;
    }

    private static TMP_InputField CreateInputField(string name, Transform parent, Vector2 position, string placeholder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, 50);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        StretchFull(textRT);
        textRT.offsetMin = new Vector2(10, 5);
        textRT.offsetMax = new Vector2(-10, -5);
        var textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.fontSize = 18;
        textTMP.color = Color.white;

        var phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(go.transform, false);
        var phRT = phGO.AddComponent<RectTransform>();
        StretchFull(phRT);
        phRT.offsetMin = new Vector2(10, 5);
        phRT.offsetMax = new Vector2(-10, -5);
        var phTMP = phGO.AddComponent<TextMeshProUGUI>();
        phTMP.text = placeholder;
        phTMP.fontSize = 18;
        phTMP.color = new Color(0.5f, 0.5f, 0.5f);
        phTMP.fontStyle = FontStyles.Italic;

        var inputField = go.AddComponent<TMP_InputField>();
        inputField.textComponent = textTMP;
        inputField.placeholder = phTMP;
        inputField.textViewport = textRT;

        return inputField;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ========================================
    //  SHOP ITEM PREFAB
    // ========================================

    [MenuItem("Gold Mine/Create Shop Item Prefab")]
    public static void CreateShopItemPrefab()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var go = new GameObject("ShopItem");
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 80);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.14f, 0.2f, 0.9f);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 80;
        le.flexibleWidth = 1;

        var shopItem = go.AddComponent<ShopItemUI>();
        shopItem.nameText = CreateText("Name", go.transform, "Card Name", 16,
            Color.white, new Vector2(-200, 10)).GetComponent<TextMeshProUGUI>();
        shopItem.typeAndCostText = CreateText("TypeCost", go.transform, "Unit • ⚡ 3", 12,
            new Color(0.7f, 0.7f, 0.7f), new Vector2(-200, -15)).GetComponent<TextMeshProUGUI>();
        shopItem.levelTagText = CreateText("Level", go.transform, "Lvl 1", 12,
            new Color(1f, 0.82f, 0.4f), new Vector2(100, 10)).GetComponent<TextMeshProUGUI>();
        shopItem.priceText = CreateText("Price", go.transform, "150 Gold", 14,
            new Color(1f, 0.82f, 0.4f), new Vector2(100, -15)).GetComponent<TextMeshProUGUI>();

        shopItem.buyButton = CreateButton("Buy", go.transform, new Vector2(330, 0),
            new Color(0.15f, 0.35f, 0.15f));
        shopItem.buyButton.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 45);
        shopItem.buyButtonText = shopItem.buyButton.GetComponentInChildren<TextMeshProUGUI>();

        string path = "Assets/Prefabs/ShopItem.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        // Wire up MenuUI reference if possible
        var menuUI = Object.FindAnyObjectByType<MenuUI>();
        if (menuUI != null) menuUI.shopItemPrefab = prefab;

        Debug.Log("✅ Shop item prefab created at Assets/Prefabs/ShopItem.prefab");
    }

    // ========================================
    //  CLEANUP — Remove all objects from previous Setup Scene runs
    // ========================================

    [MenuItem("Gold Mine/Clean Scene (Remove Duplicates)")]
    public static void CleanupOldObjects()
    {
        string[] namesToDelete = {
            "GameManager", "MatchController", "Arena", "Canvas", "EventSystem",
            "LaneDivider", "Midline", "Entities"
        };

        // Collect all root objects
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();

        int count = 0;
        foreach (var root in roots)
        {
            // Don't delete the camera or lights
            if (root.GetComponent<Camera>() != null) continue;
            if (root.GetComponent<Light>() != null) continue;
            if (root.name == "Main Camera") continue;
            if (root.name == "Global Light 2D") continue;
            if (root.name == "Directional Light") continue;

            // Delete if it matches any of our created object names
            foreach (var name in namesToDelete)
            {
                if (root.name == name || root.name.StartsWith(name))
                {
                    Undo.DestroyObjectImmediate(root);
                    count++;
                    break;
                }
            }
        }

        Debug.Log($"🧹 Cleaned up {count} old objects from scene.");
    }
}
