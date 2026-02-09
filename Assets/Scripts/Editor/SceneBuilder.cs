using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor-only script: builds the full game scene (Main Menu, HUD, Battlefield, Prefabs)
/// with a single menu click. Run via Gold Mine > Build Full Scene.
/// </summary>
public class SceneBuilder : EditorWindow
{
    [MenuItem("Gold Mine/Build Full Scene")]
    public static void BuildScene()
    {
        if (!EditorUtility.DisplayDialog("Build Gold Mine Scene",
            "This will create all UI panels, GameObjects, and prefabs.\nContinue?", "Build", "Cancel"))
            return;

        // ---- Root Objects ----
        var gameManagerGO = CreateGO("GameManager");
        gameManagerGO.AddComponent<GameManager>();

        var matchGO = CreateGO("MatchController");
        var matchCtrl = matchGO.AddComponent<MatchController>();
        matchGO.AddComponent<AIController>();
        matchGO.AddComponent<BattlefieldInput>();
        matchCtrl.ai = matchGO.GetComponent<AIController>();

        // ---- Canvas ----
        var canvasGO = CreateGO("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = CreateGO("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ---- MenuUI root ----
        var menuGO = CreateChild(canvasGO, "MenuUI");
        Stretch(menuGO);
        var menuUI = menuGO.AddComponent<MenuUI>();

        // ==== MAIN MENU PANEL ==== (matches HTML .screen #mainMenu)
        var mmPanel = CreatePanel(menuGO, "MainMenuPanel", HexColor("#000000", 0.78f));
        menuUI.mainMenuPanel = mmPanel;

        // Radial gradient overlay (simulated with a second image)
        var mmGlow = CreateChild(mmPanel, "GlowOverlay");
        SetAnchors(mmGlow, 0f, 0.3f, 0.7f, 1f);
        AddImage(mmGlow, HexColor("#FFD166", 0.06f)).raycastTarget = false;

        // Top row: Title (left) + MetaBox (right)  — matches HTML .menuTop
        var menuTop = CreateChild(mmPanel, "MenuTop");
        SetAnchors(menuTop, 0.04f, 0.82f, 0.96f, 0.96f);

        // Title — Gold Mine
        var titleTMP = CreateTMP(menuTop, "Title", "Gold Mine", 42, TextAlignmentOptions.Left);
        SetAnchors(titleTMP.gameObject, 0f, 0f, 0.50f, 1f);
        titleTMP.color = HexColor("#FFD166");

        // Meta box (right side) — Level / XP / Gold
        var metaBox = CreateChild(menuTop, "MetaBox");
        SetAnchors(metaBox, 0.52f, 0f, 1f, 1f);
        AddImage(metaBox, HexColor("#0f1118", 0.88f));
        var metaOutline = metaBox.AddComponent<Outline>();
        metaOutline.effectColor = HexColor("#FFFFFF", 0.10f);
        metaOutline.effectDistance = new Vector2(1, 1);

        // Meta rows inside box
        var mLevelRow = CreateChild(metaBox, "LevelRow");
        SetAnchors(mLevelRow, 0.05f, 0.66f, 0.95f, 0.95f);
        CreateTMP(mLevelRow, "LvLabel", "Level", 14, TextAlignmentOptions.Left).color = HexColor("#FFD700", 0.7f);
        SetAnchors(mLevelRow.transform.Find("LvLabel").gameObject, 0f, 0f, 0.5f, 1f);
        menuUI.mmLevelText = CreateTMP(mLevelRow, "LevelVal", "1", 16, TextAlignmentOptions.Right);
        SetAnchors(menuUI.mmLevelText.gameObject, 0.5f, 0f, 1f, 1f);

        var mXPRow = CreateChild(metaBox, "XPRow");
        SetAnchors(mXPRow, 0.05f, 0.35f, 0.95f, 0.64f);
        CreateTMP(mXPRow, "XPLabel", "XP", 14, TextAlignmentOptions.Left).color = HexColor("#FFD700", 0.7f);
        SetAnchors(mXPRow.transform.Find("XPLabel").gameObject, 0f, 0f, 0.5f, 1f);
        menuUI.mmXPText = CreateTMP(mXPRow, "XPVal", "0 / 120", 16, TextAlignmentOptions.Right);
        SetAnchors(menuUI.mmXPText.gameObject, 0.5f, 0f, 1f, 1f);

        var mGoldRow = CreateChild(metaBox, "GoldRow");
        SetAnchors(mGoldRow, 0.05f, 0.05f, 0.95f, 0.34f);
        CreateTMP(mGoldRow, "GoldLabel", "Gold", 14, TextAlignmentOptions.Left).color = HexColor("#FFD700", 0.7f);
        SetAnchors(mGoldRow.transform.Find("GoldLabel").gameObject, 0f, 0f, 0.5f, 1f);
        menuUI.mmGoldText = CreateTMP(mGoldRow, "GoldVal", "0", 16, TextAlignmentOptions.Right);
        menuUI.mmGoldText.color = HexColor("#FFD700");
        SetAnchors(menuUI.mmGoldText.gameObject, 0.5f, 0f, 1f, 1f);

        // Center button row — matches HTML .btnRow (3 side-by-side buttons)
        menuUI.btnPlayOnline = CreateMenuBtn(mmPanel, "BtnOnline", "Play\nOnline", false, 0.04f, 0.46f, 0.34f, 0.56f);
        menuUI.btnPlayAI = CreateMenuBtn(mmPanel, "BtnAI", "Play\nAgainst AI", true, 0.36f, 0.46f, 0.64f, 0.56f);
        menuUI.btnShop = CreateMenuBtn(mmPanel, "BtnShop", "Shop", false, 0.66f, 0.46f, 0.96f, 0.56f);

        // Bottom bar — matches HTML .bottomBar (Settings + Credits link-style)
        menuUI.btnSettings = CreateLinkBtn(mmPanel, "BtnSettings", "Settings", 0.04f, 0.04f, 0.30f, 0.10f);
        menuUI.btnCredits = CreateLinkBtn(mmPanel, "BtnCredits", "Credits", 0.70f, 0.04f, 0.96f, 0.10f);

        // ==== SHOP PANEL ==== (matches HTML #shopScreen)
        var shopPanel = CreatePanel(menuGO, "ShopPanel", HexColor("#000000", 0.78f));
        shopPanel.SetActive(false);
        menuUI.shopPanel = shopPanel;

        // Shop top row: Title+Hint (left) + Meta box (right)
        var shopTop = CreateChild(shopPanel, "ShopTop");
        SetAnchors(shopTop, 0.04f, 0.88f, 0.96f, 0.98f);

        var shopTitleCol = CreateChild(shopTop, "TitleCol");
        SetAnchors(shopTitleCol, 0f, 0f, 0.55f, 1f);
        var shopTitle = CreateTMP(shopTitleCol, "ShopTitle", "Shop", 30, TextAlignmentOptions.Left);
        SetAnchors(shopTitle.gameObject, 0f, 0.45f, 1f, 1f);
        shopTitle.color = HexColor("#FFD166");
        menuUI.shopHintText = CreateTMP(shopTitleCol, "ShopHint", "Cards unlock as you level up.", 12, TextAlignmentOptions.Left);
        menuUI.shopHintText.color = HexColor("#FFD700", 0.65f);
        SetAnchors(menuUI.shopHintText.gameObject, 0f, 0f, 1f, 0.42f);

        var shopMetaBox = CreateChild(shopTop, "ShopMetaBox");
        SetAnchors(shopMetaBox, 0.58f, 0f, 1f, 1f);
        AddImage(shopMetaBox, HexColor("#0f1118", 0.88f));
        shopMetaBox.AddComponent<Outline>().effectColor = HexColor("#FFFFFF", 0.10f);

        var sLvRow = CreateChild(shopMetaBox, "LvRow");
        SetAnchors(sLvRow, 0.08f, 0.55f, 0.92f, 0.90f);
        CreateTMP(sLvRow, "L", "Level", 13, TextAlignmentOptions.Left).color = HexColor("#FFD700", 0.7f);
        SetAnchors(sLvRow.transform.Find("L").gameObject, 0f, 0f, 0.5f, 1f);
        menuUI.shopLevelText = CreateTMP(sLvRow, "V", "1", 14, TextAlignmentOptions.Right);
        SetAnchors(menuUI.shopLevelText.gameObject, 0.5f, 0f, 1f, 1f);

        var sGdRow = CreateChild(shopMetaBox, "GdRow");
        SetAnchors(sGdRow, 0.08f, 0.18f, 0.92f, 0.53f);
        CreateTMP(sGdRow, "L2", "Gold", 13, TextAlignmentOptions.Left).color = HexColor("#FFD700", 0.7f);
        SetAnchors(sGdRow.transform.Find("L2").gameObject, 0f, 0f, 0.5f, 1f);
        menuUI.shopGoldText = CreateTMP(sGdRow, "V2", "0", 14, TextAlignmentOptions.Right);
        menuUI.shopGoldText.color = HexColor("#FFD700");
        SetAnchors(menuUI.shopGoldText.gameObject, 0.5f, 0f, 1f, 1f);

        // Shop grid with scroll
        var shopScroll = CreateChild(shopPanel, "ShopScroll");
        SetAnchors(shopScroll, 0.03f, 0.10f, 0.97f, 0.89f);
        var scrollRect = shopScroll.AddComponent<ScrollRect>();
        var viewport = CreateChild(shopScroll, "Viewport");
        Stretch(viewport);
        viewport.AddComponent<RectMask2D>();
        var content = CreateChild(viewport, "Content");
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 800);
        var gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(480, 140);
        gridLayout.spacing = new Vector2(16, 12);
        gridLayout.padding = new RectOffset(10, 10, 10, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;
        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        menuUI.shopGrid = content.transform;

        menuUI.shopHintText = CreateTMP(shopPanel, "ShopHint", "", 18, TextAlignmentOptions.Center);
        SetAnchors(menuUI.shopHintText.gameObject, 0.1f, 0.04f, 0.7f, 0.09f);

        menuUI.btnShopBack = CreateLinkBtn(shopPanel, "BtnShopBack", "Back", 0.70f, 0.02f, 0.96f, 0.07f);

        // ==== SETTINGS PANEL ==== (matches HTML #settingsScreen)
        var settingsPanel = CreatePanel(menuGO, "SettingsPanel", HexColor("#000000", 0.78f));
        settingsPanel.SetActive(false);
        menuUI.settingsPanel = settingsPanel;

        // Settings top row
        var setTop = CreateChild(settingsPanel, "SetTop");
        SetAnchors(setTop, 0.04f, 0.88f, 0.96f, 0.96f);
        var setTitle = CreateTMP(setTop, "SettingsTitle", "Settings", 30, TextAlignmentOptions.Left);
        SetAnchors(setTitle.gameObject, 0f, 0f, 0.6f, 1f);
        setTitle.color = HexColor("#FFD166");
        menuUI.btnSettingsBack = CreateLinkBtn(setTop, "BtnSettingsBack", "Back", 0.65f, 0.1f, 1f, 0.9f);

        // Settings panel content
        var setPanel = CreateChild(settingsPanel, "SetPanel");
        SetAnchors(setPanel, 0.04f, 0.55f, 0.96f, 0.86f);
        AddImage(setPanel, HexColor("#0f1118", 0.85f));
        setPanel.AddComponent<Outline>().effectColor = HexColor("#FFFFFF", 0.10f);

        // Music row (matches HTML .toggleRow)
        var musicRow = CreateChild(setPanel, "MusicRow");
        SetAnchors(musicRow, 0.04f, 0.52f, 0.96f, 0.95f);
        var musicLabel = CreateTMP(musicRow, "MusicTitle", "Music", 18, TextAlignmentOptions.Left);
        SetAnchors(musicLabel.gameObject, 0f, 0.3f, 0.35f, 1f);
        musicLabel.fontStyle = FontStyles.Bold;
        var musicSub = CreateTMP(musicRow, "MusicSub", "Volume", 13, TextAlignmentOptions.Left);
        musicSub.color = HexColor("#FFD700", 0.65f);
        SetAnchors(musicSub.gameObject, 0f, 0f, 0.35f, 0.35f);
        menuUI.musicSlider = CreateSlider(musicRow, "MusicSlider", 0.40f, 0.15f, 0.98f, 0.85f);

        // Divider line
        var divider = CreateChild(setPanel, "Divider");
        SetAnchors(divider, 0.04f, 0.49f, 0.96f, 0.51f);
        AddImage(divider, HexColor("#FFFFFF", 0.06f));

        // SFX row
        var sfxRow = CreateChild(setPanel, "SFXRow");
        SetAnchors(sfxRow, 0.04f, 0.05f, 0.96f, 0.48f);
        var sfxLabel = CreateTMP(sfxRow, "SFXTitle", "SFX", 18, TextAlignmentOptions.Left);
        SetAnchors(sfxLabel.gameObject, 0f, 0.3f, 0.35f, 1f);
        sfxLabel.fontStyle = FontStyles.Bold;
        var sfxSub = CreateTMP(sfxRow, "SFXSub", "Volume", 13, TextAlignmentOptions.Left);
        sfxSub.color = HexColor("#FFD700", 0.65f);
        SetAnchors(sfxSub.gameObject, 0f, 0f, 0.35f, 0.35f);
        menuUI.sfxSlider = CreateSlider(sfxRow, "SFXSlider", 0.40f, 0.15f, 0.98f, 0.85f);

        // ==== CREDITS PANEL ==== (matches HTML #creditsScreen with real team names)
        var creditsPanel = CreatePanel(menuGO, "CreditsPanel", HexColor("#000000", 0.78f));
        creditsPanel.SetActive(false);
        menuUI.creditsPanel = creditsPanel;

        // Credits top row
        var credTop = CreateChild(creditsPanel, "CredTop");
        SetAnchors(credTop, 0.04f, 0.88f, 0.96f, 0.96f);
        var credTitle = CreateTMP(credTop, "CreditsTitle", "Credits", 30, TextAlignmentOptions.Left);
        SetAnchors(credTitle.gameObject, 0f, 0f, 0.6f, 1f);
        credTitle.color = HexColor("#FFD166");
        menuUI.btnCreditsBack = CreateLinkBtn(credTop, "BtnCreditsBack", "Back", 0.65f, 0.1f, 1f, 0.9f);

        // Credits content panel
        var credPanel = CreateChild(creditsPanel, "CredPanel");
        SetAnchors(credPanel, 0.04f, 0.35f, 0.96f, 0.86f);
        AddImage(credPanel, HexColor("#0f1118", 0.85f));
        credPanel.AddComponent<Outline>().effectColor = HexColor("#FFFFFF", 0.10f);

        // Team members (from HTML prototype)
        string[] names  = { "Logan Bishop", "Ben Garcia", "Mikey Bryant", "Gavin Giersdorf", "Sebastian Melendez", "Alan Estrada" };
        string[] roles  = { "Lead Developer", "Developer", "Developer", "Artist, Play-Tester", "Artist, Play-Tester", "Play-Tester, Lead QA" };
        float rowH = 1f / (names.Length + 0.5f);
        for (int i = 0; i < names.Length; i++)
        {
            float yMin = 1f - (i + 1) * rowH;
            float yMax = 1f - i * rowH - 0.02f;
            var nameT = CreateTMP(credPanel, $"Name{i}", names[i], 16, TextAlignmentOptions.Left);
            nameT.fontStyle = FontStyles.Bold;
            SetAnchors(nameT.gameObject, 0.06f, yMin, 0.50f, yMax);
            var roleT = CreateTMP(credPanel, $"Role{i}", "\u2014 " + roles[i], 14, TextAlignmentOptions.Left);
            roleT.color = HexColor("#FFD700", 0.65f);
            SetAnchors(roleT.gameObject, 0.52f, yMin, 0.96f, yMax);
        }

        // ==== LAN LOBBY ====
        var lanGO = CreateChild(menuGO, "LanLobbyUI");
        Stretch(lanGO);
        var lanUI = lanGO.AddComponent<LanLobbyUI>();
        menuUI.lanLobbyUI = lanUI;
        lanGO.SetActive(false);

        // Lobby Panel
        var lobbyP = CreatePanel(lanGO, "LobbyPanel", HexColor("#0f1118", 0.96f));
        lanUI.lobbyPanel = lobbyP;
        var lobbyTitle = CreateTMP(lobbyP, "LobbyTitle", "LAN LOBBY", 36, TextAlignmentOptions.Center);
        SetAnchors(lobbyTitle.gameObject, 0.2f, 0.82f, 0.8f, 0.92f);
        lobbyTitle.color = HexColor("#FFD700");
        lanUI.btnHost = CreateButton(lobbyP, "BtnHost", "HOST GAME", 0.2f, 0.60f, 0.8f, 0.68f);
        lanUI.btnJoin = CreateButton(lobbyP, "BtnJoin", "JOIN GAME", 0.2f, 0.48f, 0.8f, 0.56f);
        lanUI.btnBack = CreateButton(lobbyP, "BtnBack", "BACK", 0.3f, 0.30f, 0.7f, 0.38f);

        // Host Wait Panel
        var hostWait = CreatePanel(lanGO, "HostWaitPanel", HexColor("#0f1118", 0.96f));
        hostWait.SetActive(false);
        lanUI.hostWaitPanel = hostWait;
        lanUI.hostIPText = CreateTMP(hostWait, "HostIPText", "Your IP: ...", 20, TextAlignmentOptions.Center);
        SetAnchors(lanUI.hostIPText.gameObject, 0.1f, 0.70f, 0.9f, 0.78f);
        lanUI.hostStatusText = CreateTMP(hostWait, "HostStatus", "Waiting for player...", 20, TextAlignmentOptions.Center);
        SetAnchors(lanUI.hostStatusText.gameObject, 0.1f, 0.58f, 0.9f, 0.66f);
        lanUI.btnStartMatch = CreateButton(hostWait, "BtnStartMatch", "START MATCH", 0.2f, 0.44f, 0.8f, 0.52f);
        lanUI.btnCancelHost = CreateButton(hostWait, "BtnCancelHost", "CANCEL", 0.3f, 0.30f, 0.7f, 0.38f);

        // Join Scan Panel
        var joinScan = CreatePanel(lanGO, "JoinScanPanel", HexColor("#0f1118", 0.96f));
        joinScan.SetActive(false);
        lanUI.joinScanPanel = joinScan;
        lanUI.scanStatusText = CreateTMP(joinScan, "ScanStatus", "Scanning...", 20, TextAlignmentOptions.Center);
        SetAnchors(lanUI.scanStatusText.gameObject, 0.1f, 0.82f, 0.9f, 0.90f);
        // Server list scroll
        var serverScroll = CreateChild(joinScan, "ServerScroll");
        SetAnchors(serverScroll, 0.05f, 0.35f, 0.95f, 0.80f);
        var sScrollRect = serverScroll.AddComponent<ScrollRect>();
        var sViewport = CreateChild(serverScroll, "Viewport");
        Stretch(sViewport);
        sViewport.AddComponent<RectMask2D>();
        var sContent = CreateChild(sViewport, "Content");
        var sContentRT = sContent.GetComponent<RectTransform>();
        sContentRT.anchorMin = new Vector2(0, 1);
        sContentRT.anchorMax = new Vector2(1, 1);
        sContentRT.pivot = new Vector2(0.5f, 1);
        sContentRT.sizeDelta = new Vector2(0, 600);
        var vlg = sContent.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        var sCSF = sContent.AddComponent<ContentSizeFitter>();
        sCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sScrollRect.content = sContentRT;
        sScrollRect.viewport = sViewport.GetComponent<RectTransform>();
        lanUI.serverListContent = sContent.transform;

        // Manual IP
        lanUI.manualIPInput = CreateInputField(joinScan, "ManualIPInput", "Enter IP...", 0.05f, 0.22f, 0.65f, 0.30f);
        lanUI.btnManualConnect = CreateButton(joinScan, "BtnManualConnect", "CONNECT", 0.68f, 0.22f, 0.95f, 0.30f);
        lanUI.btnCancelJoin = CreateButton(joinScan, "BtnCancelJoin", "CANCEL", 0.3f, 0.10f, 0.7f, 0.18f);

        // Connected Panel
        var connPanel = CreatePanel(lanGO, "ConnectedPanel", HexColor("#0f1118", 0.96f));
        connPanel.SetActive(false);
        lanUI.connectedPanel = connPanel;
        lanUI.connectedText = CreateTMP(connPanel, "ConnectedText", "Connected!", 24, TextAlignmentOptions.Center);
        SetAnchors(lanUI.connectedText.gameObject, 0.1f, 0.55f, 0.9f, 0.65f);

        // ==== HUD (in-game) ==== (matches HTML #topbar + #bottom)
        var hudGO = CreateChild(canvasGO, "HUD");
        Stretch(hudGO);
        var hudMgr = hudGO.AddComponent<HUDManager>();
        hudGO.SetActive(false);
        menuUI.hudObject = hudGO;
        matchCtrl.hud = hudMgr;

        // Top bar — matches HTML #topbar (semi-transparent dark gradient)
        var topBar = CreateChild(hudGO, "TopBar");
        SetAnchors(topBar, 0f, 0.92f, 1f, 1f);
        AddImage(topBar, HexColor("#12141c", 0.82f));

        // Enemy Energy column
        var eCol = CreateChild(topBar, "EnemyCol");
        SetAnchors(eCol, 0.02f, 0f, 0.24f, 1f);
        var eLabel = CreateTMP(eCol, "ELabel", "Enemy Energy", 11, TextAlignmentOptions.Left);
        eLabel.color = HexColor("#FFD700", 0.65f);
        SetAnchors(eLabel.gameObject, 0f, 0.5f, 1f, 1f);
        hudMgr.enemyEnergyText = CreateTMP(eCol, "EVal", "0", 14, TextAlignmentOptions.Left);
        SetAnchors(hudMgr.enemyEnergyText.gameObject, 0f, 0f, 1f, 0.5f);

        // Selected card column (center)
        var sCol = CreateChild(topBar, "SelectedCol");
        SetAnchors(sCol, 0.26f, 0f, 0.54f, 1f);
        var sLabel = CreateTMP(sCol, "SLabel", "Selected", 11, TextAlignmentOptions.Center);
        sLabel.color = HexColor("#FFD700", 0.65f);
        SetAnchors(sLabel.gameObject, 0f, 0.5f, 1f, 1f);
        hudMgr.selectedCardText = CreateTMP(sCol, "SVal", "-", 14, TextAlignmentOptions.Center);
        SetAnchors(hudMgr.selectedCardText.gameObject, 0f, 0f, 1f, 0.5f);

        // Your Energy column
        var pCol = CreateChild(topBar, "PlayerCol");
        SetAnchors(pCol, 0.56f, 0f, 0.78f, 1f);
        var pLabel = CreateTMP(pCol, "PLabel", "Your Energy", 11, TextAlignmentOptions.Right);
        pLabel.color = HexColor("#FFD700", 0.65f);
        SetAnchors(pLabel.gameObject, 0f, 0.5f, 1f, 1f);
        hudMgr.playerEnergyText = CreateTMP(pCol, "PVal", "0", 14, TextAlignmentOptions.Right);
        SetAnchors(hudMgr.playerEnergyText.gameObject, 0f, 0f, 1f, 0.5f);

        // Level column
        var lvCol = CreateChild(topBar, "LevelCol");
        SetAnchors(lvCol, 0.80f, 0f, 0.98f, 1f);
        var lvLabel = CreateTMP(lvCol, "LvLabel", "Level", 11, TextAlignmentOptions.Right);
        lvLabel.color = HexColor("#FFD700", 0.65f);
        SetAnchors(lvLabel.gameObject, 0f, 0.5f, 1f, 1f);
        hudMgr.hudLevelText = CreateTMP(lvCol, "LvVal", "1", 14, TextAlignmentOptions.Right);
        SetAnchors(hudMgr.hudLevelText.gameObject, 0f, 0f, 1f, 0.5f);

        // ==== Bottom HUD ==== (matches HTML #bottom)
        var botBar = CreateChild(hudGO, "BottomHUD");
        SetAnchors(botBar, 0f, 0f, 1f, 0.20f);
        AddImage(botBar, HexColor("#12141c", 0.82f));

        // Energy row label + value
        var enRow = CreateChild(botBar, "EnergyRow");
        SetAnchors(enRow, 0.03f, 0.82f, 0.97f, 0.95f);
        var enLabel = CreateTMP(enRow, "EnLabel", "Energy", 12, TextAlignmentOptions.Left);
        enLabel.color = HexColor("#FFD700", 0.65f);
        SetAnchors(enLabel.gameObject, 0f, 0f, 0.3f, 1f);
        hudMgr.energyValueText = CreateTMP(enRow, "EnValue", "0 / 10", 13, TextAlignmentOptions.Right);
        SetAnchors(hudMgr.energyValueText.gameObject, 0.3f, 0f, 1f, 1f);

        // Segmented energy bar — matches HTML #energyBar
        var energyBar = CreateChild(botBar, "EnergyBar");
        SetAnchors(energyBar, 0.03f, 0.72f, 0.97f, 0.82f);
        AddImage(energyBar, HexColor("#000000", 0.25f));
        var ebOutline = energyBar.AddComponent<Outline>();
        ebOutline.effectColor = HexColor("#FFFFFF", 0.10f);
        ebOutline.effectDistance = new Vector2(1, 1);

        hudMgr.energySegments = new Image[10];
        for (int i = 0; i < 10; i++)
        {
            var seg = CreateChild(energyBar, $"Seg{i}");
            float segW = 1f / 10f;
            SetAnchors(seg, segW * i + 0.002f, 0.08f, segW * (i + 1) - 0.002f, 0.92f);
            hudMgr.energySegments[i] = AddImage(seg, HexColor("#FFD166", 0.90f));
        }

        // Hand area — matches HTML #handRow
        var handArea = CreateChild(botBar, "HandArea");
        SetAnchors(handArea, 0.025f, 0.18f, 0.975f, 0.70f);
        var hlg = handArea.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(0, 0, 0, 0);
        hudMgr.handContainer = handArea.transform;

        // Hint row + deck count — matches HTML #hint
        var hintRow = CreateChild(botBar, "HintRow");
        SetAnchors(hintRow, 0.03f, 0.02f, 0.97f, 0.16f);
        var hintTxt = CreateTMP(hintRow, "HintText", "Tap card, then tap battlefield. Left = Lane A, Right = Lane B", 10, TextAlignmentOptions.Left);
        hintTxt.color = HexColor("#FFD700", 0.50f);
        SetAnchors(hintTxt.gameObject, 0f, 0f, 0.70f, 1f);
        hudMgr.deckCountText = CreateTMP(hintRow, "DeckCount", "Deck: 0", 12, TextAlignmentOptions.Right);
        SetAnchors(hudMgr.deckCountText.gameObject, 0.72f, 0f, 1f, 1f);

        // Game Over overlay — matches HTML #overlay
        var goPanel = CreatePanel(hudGO, "GameOverPanel", HexColor("#000000", 0.80f));
        goPanel.SetActive(false);
        hudMgr.gameOverPanel = goPanel;
        // Radial warm glow
        var goGlow = CreateChild(goPanel, "Glow");
        SetAnchors(goGlow, 0.1f, 0.2f, 0.9f, 0.8f);
        AddImage(goGlow, HexColor("#FFD166", 0.06f)).raycastTarget = false;

        hudMgr.gameOverTitle = CreateTMP(goPanel, "GameOverTitle", "Victory!", 40, TextAlignmentOptions.Center);
        SetAnchors(hudMgr.gameOverTitle.gameObject, 0.1f, 0.58f, 0.9f, 0.72f);
        hudMgr.gameOverTitle.color = HexColor("#FFD700");
        hudMgr.gameOverRewards = CreateTMP(goPanel, "GameOverRewards", "+45 XP, +60 Gold", 18, TextAlignmentOptions.Center);
        hudMgr.gameOverRewards.color = HexColor("#FFD700", 0.70f);
        SetAnchors(hudMgr.gameOverRewards.gameObject, 0.1f, 0.48f, 0.9f, 0.56f);
        hudMgr.returnMenuButton = CreateMenuBtn(goPanel, "BtnReturnMenu", "Return to Menu", true, 0.12f, 0.34f, 0.48f, 0.44f);
        hudMgr.playAgainButton = CreateMenuBtn(goPanel, "BtnPlayAgain", "Play Again (AI)", false, 0.52f, 0.34f, 0.88f, 0.44f);

        // ==== PREFABS ====
        BuildPrefabs(menuUI, hudMgr, lanUI, matchCtrl);

        // ==== Arena Background ====
        var arenaGO = CreateGO("ArenaBackground");
        arenaGO.AddComponent<ArenaRenderer>();
        arenaGO.transform.position = new Vector3(0, 0, 1); // behind entities

        // ==== Camera setup ====
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = HexColor("#06070c");
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        // ==== LAN Manager ====
        var lanMgrGO = CreateGO("LanManager");
        lanMgrGO.AddComponent<LanManager>();
        var lanMatchHandler = lanMgrGO.AddComponent<LanMatchHandler>();
        lanUI.lanMatchHandler = lanMatchHandler;

        // Mark dirty (only outside play mode)
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        Debug.Log("<color=#FFD700>✓ Gold Mine scene built successfully! Save the scene (Ctrl+S).</color>");
    }

    // ==================================================================
    //  PREFABS
    // ==================================================================
    static void BuildPrefabs(MenuUI menuUI, HUDManager hud, LanLobbyUI lanUI, MatchController match)
    {
        string prefabDir = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabDir))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // ---- CardUI Prefab ----
        {
            var go = new GameObject("CardUI");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 60);
            var img = go.AddComponent<Image>();
            img.color = HexColor("#0e1017", 0.92f);

            var cardUI = go.AddComponent<CardUI>();
            cardUI.background = img;
            cardUI.button = go.AddComponent<Button>();

            cardUI.nameText = CreateTMP(go, "Name", "Card", 14, TextAlignmentOptions.Left);
            SetAnchors(cardUI.nameText.gameObject, 0.05f, 0.3f, 0.65f, 0.9f);
            cardUI.typeText = CreateTMP(go, "Type", "Unit", 10, TextAlignmentOptions.Left);
            SetAnchors(cardUI.typeText.gameObject, 0.05f, 0f, 0.5f, 0.35f);
            cardUI.costText = CreateTMP(go, "Cost", "⚡ 3", 14, TextAlignmentOptions.Right);
            SetAnchors(cardUI.costText.gameObject, 0.65f, 0.3f, 0.95f, 0.9f);
            cardUI.costText.color = HexColor("#FFD700");

            var border = CreateChild(go, "SelectionBorder");
            Stretch(border);
            var borderImg = AddImage(border, HexColor("#FFD700", 0f));
            // Use Outline component for border effect
            var outline = border.AddComponent<Outline>();
            outline.effectColor = HexColor("#FFD700");
            outline.effectDistance = new Vector2(2, 2);
            cardUI.selectionBorder = borderImg;
            border.SetActive(false);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{prefabDir}/CardUI.prefab");
            hud.cardUIPrefab = prefab;
            Object.DestroyImmediate(go);
        }

        // ---- ShopItemUI Prefab ----
        {
            var go = new GameObject("ShopItem");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(480, 140);
            AddImage(go, HexColor("#141620", 0.9f));

            var item = go.AddComponent<ShopItemUI>();
            item.nameText = CreateTMP(go, "Name", "Card Name", 18, TextAlignmentOptions.Left);
            SetAnchors(item.nameText.gameObject, 0.05f, 0.55f, 0.65f, 0.9f);
            item.typeAndCostText = CreateTMP(go, "TypeCost", "Unit • ⚡ 3", 13, TextAlignmentOptions.Left);
            SetAnchors(item.typeAndCostText.gameObject, 0.05f, 0.30f, 0.65f, 0.55f);
            item.levelTagText = CreateTMP(go, "LevelTag", "Lvl 1", 12, TextAlignmentOptions.Left);
            SetAnchors(item.levelTagText.gameObject, 0.05f, 0.05f, 0.3f, 0.30f);
            item.priceText = CreateTMP(go, "Price", "Free", 14, TextAlignmentOptions.Right);
            SetAnchors(item.priceText.gameObject, 0.55f, 0.55f, 0.95f, 0.9f);

            var buyBtnGO = new GameObject("BuyButton");
            buyBtnGO.transform.SetParent(go.transform, false);
            var buyRT = buyBtnGO.AddComponent<RectTransform>();
            SetAnchors(buyBtnGO, 0.65f, 0.08f, 0.95f, 0.45f);
            var buyImg = AddImage(buyBtnGO, HexColor("#FFD700", 0.85f));
            item.buyButton = buyBtnGO.AddComponent<Button>();
            item.buyButtonText = CreateTMP(buyBtnGO, "BuyText", "Buy", 14, TextAlignmentOptions.Center);
            Stretch(item.buyButtonText.gameObject);
            item.buyButtonText.color = Color.black;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{prefabDir}/ShopItem.prefab");
            menuUI.shopItemPrefab = prefab;
            Object.DestroyImmediate(go);
        }

        // ---- ServerItem Prefab (for LAN) ----
        {
            var go = new GameObject("ServerItem");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60);
            AddImage(go, HexColor("#1a1e2e", 0.8f));
            go.AddComponent<Button>();
            var t = CreateTMP(go, "ServerText", "192.168.1.1:7777", 16, TextAlignmentOptions.Center);
            Stretch(t.gameObject);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{prefabDir}/ServerItem.prefab");
            lanUI.serverItemPrefab = prefab;
            Object.DestroyImmediate(go);
        }

        // ---- Game Entity Prefabs with Pixel Art Sprites ----
        {
            // Save pixel art sprites as assets
            string spriteDir = "Assets/Sprites";
            if (!AssetDatabase.IsValidFolder(spriteDir))
                AssetDatabase.CreateFolder("Assets", "Sprites");

            // Generate all sprites
            var pTowerSprite = PixelSpriteGenerator.PlayerTower();
            var eTowerSprite = PixelSpriteGenerator.EnemyTower();
            var pKingSprite  = PixelSpriteGenerator.PlayerKing();
            var eKingSprite  = PixelSpriteGenerator.EnemyKing();
            var pUnitSprite  = PixelSpriteGenerator.PlayerUnit();
            var eUnitSprite  = PixelSpriteGenerator.EnemyUnit();
            var pumpSprite   = PixelSpriteGenerator.Pump();
            var projSprite   = PixelSpriteGenerator.Projectile();

            // Assign sprites to MatchController
            match.playerTowerSprite = pTowerSprite;
            match.enemyTowerSprite  = eTowerSprite;
            match.playerKingSprite  = pKingSprite;
            match.enemyKingSprite   = eKingSprite;
            match.playerUnitSprite  = pUnitSprite;
            match.enemyUnitSprite   = eUnitSprite;
            match.pumpSprite        = pumpSprite;
            match.projectileSprite  = projSprite;

            // Tower prefab
            var tower = new GameObject("Tower");
            var tSR = tower.AddComponent<SpriteRenderer>();
            tSR.sprite = pTowerSprite;
            tSR.sortingOrder = 5;
            tower.AddComponent<Damageable>();
            tower.AddComponent<TowerController>();
            AddHealthBarChild(tower, HexColor("#FFD166"));
            var towerPrefab = PrefabUtility.SaveAsPrefabAsset(tower, $"{prefabDir}/Tower.prefab");
            match.towerPrefab = towerPrefab;
            Object.DestroyImmediate(tower);

            // King prefab
            var king = new GameObject("King");
            var kSR = king.AddComponent<SpriteRenderer>();
            kSR.sprite = pKingSprite;
            kSR.sortingOrder = 5;
            king.AddComponent<Damageable>();
            king.AddComponent<TowerController>();
            AddHealthBarChild(king, HexColor("#FFD166"));
            var kingPrefab = PrefabUtility.SaveAsPrefabAsset(king, $"{prefabDir}/King.prefab");
            match.kingPrefab = kingPrefab;
            Object.DestroyImmediate(king);

            // Unit prefab
            var unit = new GameObject("Unit");
            var uSR = unit.AddComponent<SpriteRenderer>();
            uSR.sprite = pUnitSprite;
            uSR.sortingOrder = 10;
            unit.AddComponent<Damageable>();
            unit.AddComponent<UnitController>();
            AddHealthBarChild(unit, HexColor("#6ee7a8"));
            var unitPrefab = PrefabUtility.SaveAsPrefabAsset(unit, $"{prefabDir}/Unit.prefab");
            match.unitPrefab = unitPrefab;
            Object.DestroyImmediate(unit);

            // Pump prefab
            var pump = new GameObject("Pump");
            var pmSR = pump.AddComponent<SpriteRenderer>();
            pmSR.sprite = pumpSprite;
            pmSR.sortingOrder = 6;
            pump.AddComponent<Damageable>();
            pump.AddComponent<PumpController>();
            AddHealthBarChild(pump, HexColor("#FFD166"));
            var pumpPrefab = PrefabUtility.SaveAsPrefabAsset(pump, $"{prefabDir}/Pump.prefab");
            match.pumpPrefab = pumpPrefab;
            Object.DestroyImmediate(pump);

            // Projectile prefab
            var proj = new GameObject("Projectile");
            var prSR = proj.AddComponent<SpriteRenderer>();
            prSR.sprite = projSprite;
            prSR.sortingOrder = 15;
            proj.transform.localScale = Vector3.one * 0.35f;
            proj.AddComponent<ProjectileController>();
            var projPrefab = PrefabUtility.SaveAsPrefabAsset(proj, $"{prefabDir}/Projectile.prefab");
            match.projectilePrefab = projPrefab;
            Object.DestroyImmediate(proj);
        }
    }

    static void AddHealthBarChild(GameObject parent, Color barColor)
    {
        var hb = new GameObject("HealthBar");
        hb.transform.SetParent(parent.transform, false);
        hb.transform.localPosition = new Vector3(0, -0.6f, 0);
        var healthBar = hb.AddComponent<HealthBar>();

        var bg = new GameObject("BG");
        bg.transform.SetParent(hb.transform, false);
        var bgSR = bg.AddComponent<SpriteRenderer>();
        bgSR.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        bg.transform.localScale = new Vector3(1f, 0.12f, 1f);
        healthBar.background = bg.transform;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(hb.transform, false);
        var fillSR = fill.AddComponent<SpriteRenderer>();
        fillSR.color = barColor;
        fillSR.sortingOrder = 1;
        fill.transform.localScale = new Vector3(1f, 0.12f, 1f);
        healthBar.fill = fill.transform;
        healthBar.fillRenderer = fillSR;
    }

    // ==================================================================
    //  HELPER METHODS
    // ==================================================================

    static GameObject CreateGO(string name)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    static GameObject CreateChild(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject CreatePanel(GameObject parent, string name, Color color)
    {
        var go = CreateChild(parent, name);
        Stretch(go);
        AddImage(go, color);
        return go;
    }

    static Image AddImage(GameObject go, Color color)
    {
        var img = go.GetComponent<Image>();
        if (img == null) img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static TextMeshProUGUI CreateTMP(GameObject parent, string name, string text, int fontSize, TextAlignmentOptions align)
    {
        var go = CreateChild(parent, name);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = HexColor("#FFD700");
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        return tmp;
    }

    static Button CreateButton(GameObject parent, string name, string label,
        float xMin, float yMin, float xMax, float yMax)
    {
        var go = CreateChild(parent, name);
        SetAnchors(go, xMin, yMin, xMax, yMax);

        var img = AddImage(go, HexColor("#1a1e2e", 0.9f));

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = HexColor("#1a1e2e");
        colors.highlightedColor = HexColor("#2a2e3e");
        colors.pressedColor = HexColor("#FFD700");
        btn.colors = colors;

        // Outline
        var outline = go.AddComponent<Outline>();
        outline.effectColor = HexColor("#FFD700", 0.6f);
        outline.effectDistance = new Vector2(1, 1);

        var tmp = CreateTMP(go, "Label", label, 22, TextAlignmentOptions.Center);
        Stretch(tmp.gameObject);
        tmp.color = HexColor("#FFD700");

        return btn;
    }

    // Matches HTML .btn / .btn.primary — retro 3px gold border, dark or gold gradient
    static Button CreateMenuBtn(GameObject parent, string name, string label,
        bool isPrimary, float xMin, float yMin, float xMax, float yMax)
    {
        var go = CreateChild(parent, name);
        SetAnchors(go, xMin, yMin, xMax, yMax);

        Color bgColor = isPrimary ? HexColor("#FFD700") : HexColor("#0f3460");
        var img = AddImage(go, bgColor);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = isPrimary ? HexColor("#FFE44D") : HexColor("#1a4a8a");
        colors.pressedColor = isPrimary ? HexColor("#CCAA00") : HexColor("#FFD700");
        btn.colors = colors;

        // 3px gold border via Outline
        var outline = go.AddComponent<Outline>();
        outline.effectColor = HexColor("#FFD700", isPrimary ? 0.8f : 0.7f);
        outline.effectDistance = new Vector2(3, 3);

        var tmp = CreateTMP(go, "Label", label, 18, TextAlignmentOptions.Center);
        Stretch(tmp.gameObject);
        tmp.color = isPrimary ? HexColor("#1a1a2e") : Color.white;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }

    // Matches HTML .linkBtn — pill-shaped, subtle 1px border, small text, transparent bg
    static Button CreateLinkBtn(GameObject parent, string name, string label,
        float xMin, float yMin, float xMax, float yMax)
    {
        var go = CreateChild(parent, name);
        SetAnchors(go, xMin, yMin, xMax, yMax);

        var img = AddImage(go, HexColor("#FFFFFF", 0.04f));

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = HexColor("#FFFFFF", 0.04f);
        colors.highlightedColor = HexColor("#FFFFFF", 0.10f);
        colors.pressedColor = HexColor("#FFD700", 0.25f);
        btn.colors = colors;

        // Subtle 1px border
        var outline = go.AddComponent<Outline>();
        outline.effectColor = HexColor("#FFFFFF", 0.12f);
        outline.effectDistance = new Vector2(1, 1);

        var tmp = CreateTMP(go, "Label", label, 14, TextAlignmentOptions.Center);
        Stretch(tmp.gameObject);
        tmp.color = HexColor("#FFFFFF", 0.7f);

        return btn;
    }

    static Slider CreateSlider(GameObject parent, string name,
        float xMin, float yMin, float xMax, float yMax)
    {
        var go = CreateChild(parent, name);
        SetAnchors(go, xMin, yMin, xMax, yMax);

        // Background
        var bgGO = CreateChild(go, "Background");
        Stretch(bgGO);
        AddImage(bgGO, HexColor("#111111", 0.8f));

        // Fill area
        var fillArea = CreateChild(go, "Fill Area");
        var fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.2f);
        fillAreaRT.anchorMax = new Vector2(1, 0.8f);
        fillAreaRT.offsetMin = new Vector2(5, 0);
        fillAreaRT.offsetMax = new Vector2(-5, 0);

        var fillGO = CreateChild(fillArea, "Fill");
        Stretch(fillGO);
        var fillImg = AddImage(fillGO, HexColor("#FFD700"));

        // Handle area
        var handleArea = CreateChild(go, "Handle Slide Area");
        var hAreaRT = handleArea.GetComponent<RectTransform>();
        hAreaRT.anchorMin = Vector2.zero;
        hAreaRT.anchorMax = Vector2.one;
        hAreaRT.offsetMin = new Vector2(10, 0);
        hAreaRT.offsetMax = new Vector2(-10, 0);

        var handle = CreateChild(handleArea, "Handle");
        var handleRT = handle.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(30, 0);
        var handleImg = AddImage(handle, Color.white);

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillGO.GetComponent<RectTransform>();
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;

        return slider;
    }

    static TMP_InputField CreateInputField(GameObject parent, string name, string placeholder,
        float xMin, float yMin, float xMax, float yMax)
    {
        var go = CreateChild(parent, name);
        SetAnchors(go, xMin, yMin, xMax, yMax);
        AddImage(go, HexColor("#111118", 0.9f));

        var textArea = CreateChild(go, "Text Area");
        Stretch(textArea);
        textArea.AddComponent<RectMask2D>();

        var textGO = CreateChild(textArea, "Text");
        Stretch(textGO);
        var textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.fontSize = 18;
        textTMP.color = HexColor("#FFD700");

        var phGO = CreateChild(textArea, "Placeholder");
        Stretch(phGO);
        var phTMP = phGO.AddComponent<TextMeshProUGUI>();
        phTMP.text = placeholder;
        phTMP.fontSize = 18;
        phTMP.color = HexColor("#FFD700", 0.4f);
        phTMP.fontStyle = FontStyles.Italic;

        var input = go.AddComponent<TMP_InputField>();
        input.textViewport = textArea.GetComponent<RectTransform>();
        input.textComponent = textTMP;
        input.placeholder = phTMP;

        return input;
    }

    // ---- Color helpers ----
    static Color HexColor(string hex, float alpha = 1f)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        c.a = alpha;
        return c;
    }
}
