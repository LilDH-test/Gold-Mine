using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// LanLobbyUI — UI for hosting/joining LAN games.
/// Shows Host/Join buttons, discovered servers, and waiting-for-player screen.
/// </summary>
public class LanLobbyUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject lobbyPanel;
    public GameObject hostWaitPanel;     // "Waiting for player..."
    public GameObject joinScanPanel;     // Scanning for hosts
    public GameObject connectedPanel;    // Both players connected, ready to start

    [Header("Lobby Buttons")]
    public Button btnHost;
    public Button btnJoin;
    public Button btnBack;

    [Header("Host Wait")]
    public TextMeshProUGUI hostIPText;
    public TextMeshProUGUI hostStatusText;
    public Button btnStartMatch;         // Host clicks to start when client joins
    public Button btnCancelHost;

    [Header("Join / Scan")]
    public Transform serverListContent;
    public GameObject serverItemPrefab;
    public TextMeshProUGUI scanStatusText;
    public Button btnCancelJoin;
    public Button btnManualConnect;
    public TMP_InputField manualIPInput;

    [Header("Connected")]
    public TextMeshProUGUI connectedText;

    [Header("References")]
    public LanMatchHandler lanMatchHandler;

    private Dictionary<string, (string host, int port)> foundServers = new Dictionary<string, (string, int)>();

    private void Start()
    {
        btnHost?.onClick.AddListener(OnHost);
        btnJoin?.onClick.AddListener(OnJoin);
        btnBack?.onClick.AddListener(OnBack);
        btnCancelHost?.onClick.AddListener(OnCancelHost);
        btnCancelJoin?.onClick.AddListener(OnCancelJoin);
        btnStartMatch?.onClick.AddListener(OnHostStartMatch);
        btnManualConnect?.onClick.AddListener(OnManualConnect);

        HideAll();
    }

    private void OnEnable()
    {
        if (LanManager.Instance != null)
        {
            LanManager.Instance.OnClientConnected += OnPeerConnected;
            LanManager.Instance.OnClientDisconnected += OnPeerDisconnected;
            LanManager.Instance.OnServerFound += OnServerDiscovered;
        }
    }

    private void OnDisable()
    {
        if (LanManager.Instance != null)
        {
            LanManager.Instance.OnClientConnected -= OnPeerConnected;
            LanManager.Instance.OnClientDisconnected -= OnPeerDisconnected;
            LanManager.Instance.OnServerFound -= OnServerDiscovered;
        }
    }

    // ---- Show/Hide ----
    public void ShowLobby()
    {
        gameObject.SetActive(true);
        lobbyPanel?.SetActive(true);
        hostWaitPanel?.SetActive(false);
        joinScanPanel?.SetActive(false);
        connectedPanel?.SetActive(false);
    }

    private void HideAll()
    {
        lobbyPanel?.SetActive(false);
        hostWaitPanel?.SetActive(false);
        joinScanPanel?.SetActive(false);
        connectedPanel?.SetActive(false);
    }

    // ========================================
    //  HOST FLOW
    // ========================================

    private void OnHost()
    {
        LanManager.Instance?.StartHost();

        lobbyPanel?.SetActive(false);
        hostWaitPanel?.SetActive(true);

        if (hostIPText != null)
            hostIPText.text = $"Your IP: {LanManager.GetLocalIP()}";
        if (hostStatusText != null)
            hostStatusText.text = "Waiting for player to join...";
        if (btnStartMatch != null)
            btnStartMatch.interactable = false;
    }

    private void OnCancelHost()
    {
        LanManager.Instance?.Shutdown();
        hostWaitPanel?.SetActive(false);
        lobbyPanel?.SetActive(true);
    }

    private void OnHostStartMatch()
    {
        HideAll();
        gameObject.SetActive(false);

        lanMatchHandler?.HostStartMatch();
    }

    // ========================================
    //  JOIN FLOW
    // ========================================

    private void OnJoin()
    {
        foundServers.Clear();
        LanManager.Instance?.StartDiscovery();

        lobbyPanel?.SetActive(false);
        joinScanPanel?.SetActive(true);

        if (scanStatusText != null)
            scanStatusText.text = "Scanning for hosts on LAN...";

        ClearServerList();
    }

    private void OnCancelJoin()
    {
        LanManager.Instance?.Shutdown();
        joinScanPanel?.SetActive(false);
        lobbyPanel?.SetActive(true);
    }

    private void OnManualConnect()
    {
        if (manualIPInput == null || string.IsNullOrWhiteSpace(manualIPInput.text)) return;

        string ip = manualIPInput.text.Trim();
        LanManager.Instance?.ConnectToHost(ip, LanManager.GAME_PORT);
    }

    private void OnServerDiscovered(string host, int port)
    {
        string key = $"{host}:{port}";
        if (foundServers.ContainsKey(key)) return;

        foundServers[key] = (host, port);

        if (scanStatusText != null)
            scanStatusText.text = $"Found {foundServers.Count} host(s)";

        // Add to UI list
        if (serverListContent != null && serverItemPrefab != null)
        {
            var go = Instantiate(serverItemPrefab, serverListContent);

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = key;

            var btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();

            string capturedHost = host;
            int capturedPort = port;
            btn.onClick.AddListener(() =>
            {
                LanManager.Instance?.ConnectToHost(capturedHost, capturedPort);
            });
        }
    }

    private void ClearServerList()
    {
        if (serverListContent == null) return;
        foreach (Transform child in serverListContent)
            Destroy(child.gameObject);
    }

    // ========================================
    //  CONNECTION EVENTS
    // ========================================

    private void OnPeerConnected()
    {
        if (LanManager.Instance.IsHost)
        {
            // Host: player joined
            if (hostStatusText != null)
                hostStatusText.text = $"Player joined: {LanManager.Instance.RemotePlayerName}";
            if (btnStartMatch != null)
                btnStartMatch.interactable = true;
        }
        else
        {
            // Client: connected to host
            joinScanPanel?.SetActive(false);
            connectedPanel?.SetActive(true);

            if (connectedText != null)
                connectedText.text = "Connected! Waiting for host to start...";
        }
    }

    private void OnPeerDisconnected()
    {
        if (hostStatusText != null)
            hostStatusText.text = "Player disconnected.";
        if (btnStartMatch != null)
            btnStartMatch.interactable = false;
        if (connectedText != null)
            connectedText.text = "Disconnected from host.";
    }

    // ========================================
    //  BACK
    // ========================================

    private void OnBack()
    {
        LanManager.Instance?.Shutdown();
        HideAll();
        gameObject.SetActive(false);

        // Return to main menu
        var menu = FindAnyObjectByType<MenuUI>();
        if (menu != null) menu.ShowMainMenu();
    }
}
