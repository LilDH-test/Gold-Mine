using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// LanManager — Handles LAN discovery (UDP broadcast) and game networking (TCP).
/// Host: listens for connections and runs the authoritative MatchController.
/// Client: connects to host and sends card-play commands.
/// </summary>
public class LanManager : MonoBehaviour
{
    public static LanManager Instance { get; private set; }

    // ---- Config ----
    public const int DISCOVERY_PORT = 47777;
    public const int GAME_PORT = 47778;
    public const float BROADCAST_INTERVAL = 1.0f;
    public const float SYNC_INTERVAL = 0.1f; // 10 Hz state sync
    private const string GAME_ID = "GoldMine";

    // ---- State ----
    public bool IsHost { get; private set; }
    public bool IsClient { get; private set; }
    public bool IsConnected { get; private set; }
    public bool InLobby { get; set; }
    public string RemotePlayerName { get; set; } = "";

    // ---- Events (main thread) ----
    public event Action OnClientConnected;
    public event Action OnClientDisconnected;
    public event Action<string> OnMessageReceived; // raw JSON
    public event Action<string, int> OnServerFound;  // hostName, port

    // ---- Networking ----
    private TcpListener tcpListener;
    private TcpClient tcpClient;
    private NetworkStream netStream;
    private UdpClient udpBroadcaster;
    private UdpClient udpListener;

    private Thread listenThread;
    private Thread receiveThread;
    private Thread discoveryThread;

    private readonly Queue<Action> mainThreadQueue = new Queue<Action>();
    private readonly Queue<string> incomingMessages = new Queue<string>();

    private float broadcastTimer;
    private float syncTimer;
    private bool isRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Shutdown();
    }

    private void OnApplicationQuit()
    {
        Shutdown();
    }

    // ========================================
    //  HOST
    // ========================================

    /// <summary>Start hosting a LAN game.</summary>
    public void StartHost()
    {
        Shutdown();
        IsHost = true;
        IsClient = false;
        isRunning = true;
        InLobby = true;

        // TCP listener
        tcpListener = new TcpListener(IPAddress.Any, GAME_PORT);
        tcpListener.Start();

        listenThread = new Thread(HostListenLoop) { IsBackground = true };
        listenThread.Start();

        // UDP broadcast
        udpBroadcaster = new UdpClient();
        udpBroadcaster.EnableBroadcast = true;

        Debug.Log($"[LAN] Hosting on port {GAME_PORT}, broadcasting on {DISCOVERY_PORT}");
    }

    private void HostListenLoop()
    {
        try
        {
            while (isRunning)
            {
                if (tcpListener.Pending())
                {
                    var client = tcpListener.AcceptTcpClient();
                    // Only accept one client
                    if (tcpClient != null)
                    {
                        client.Close();
                        continue;
                    }

                    tcpClient = client;
                    netStream = tcpClient.GetStream();
                    IsConnected = true;

                    Enqueue(() => OnClientConnected?.Invoke());

                    // Start receiving
                    receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                    receiveThread.Start();
                }
                Thread.Sleep(100);
            }
        }
        catch (Exception e)
        {
            if (isRunning) Debug.LogError($"[LAN] Host listen error: {e.Message}");
        }
    }

    // ========================================
    //  CLIENT
    // ========================================

    /// <summary>Start scanning for LAN hosts.</summary>
    public void StartDiscovery()
    {
        Shutdown();
        IsHost = false;
        IsClient = true;
        isRunning = true;

        udpListener = new UdpClient(DISCOVERY_PORT);
        udpListener.EnableBroadcast = true;

        discoveryThread = new Thread(DiscoveryListenLoop) { IsBackground = true };
        discoveryThread.Start();

        Debug.Log($"[LAN] Scanning for hosts on port {DISCOVERY_PORT}...");
    }

    private void DiscoveryListenLoop()
    {
        try
        {
            var ep = new IPEndPoint(IPAddress.Any, DISCOVERY_PORT);
            while (isRunning)
            {
                if (udpListener.Available > 0)
                {
                    byte[] data = udpListener.Receive(ref ep);
                    string json = Encoding.UTF8.GetString(data);
                    var msg = JsonUtility.FromJson<NetMessages.DiscoveryBroadcast>(json);

                    if (msg != null && msg.gameName == GAME_ID)
                    {
                        string host = ep.Address.ToString();
                        int port = msg.port;
                        Enqueue(() => OnServerFound?.Invoke(host, port));
                    }
                }
                Thread.Sleep(200);
            }
        }
        catch (Exception e)
        {
            if (isRunning) Debug.LogError($"[LAN] Discovery error: {e.Message}");
        }
    }

    /// <summary>Connect to a discovered host.</summary>
    public void ConnectToHost(string hostIP, int port)
    {
        try
        {
            // Stop discovery
            StopDiscovery();

            tcpClient = new TcpClient();
            tcpClient.Connect(hostIP, port);
            netStream = tcpClient.GetStream();
            IsConnected = true;
            InLobby = true;

            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();

            // Send ready
            var ready = new NetMessages.LobbyReadyMsg
            {
                playerName = SystemInfo.deviceName
            };
            SendMessage(ready);

            Debug.Log($"[LAN] Connected to {hostIP}:{port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LAN] Connect failed: {e.Message}");
            IsConnected = false;
        }
    }

    // ========================================
    //  SHARED NETWORKING
    // ========================================

    private void ReceiveLoop()
    {
        byte[] lengthBuf = new byte[4];
        try
        {
            while (isRunning && tcpClient != null && tcpClient.Connected)
            {
                // Read 4-byte length prefix
                int read = ReadExact(netStream, lengthBuf, 4);
                if (read < 4) break;

                int msgLen = BitConverter.ToInt32(lengthBuf, 0);
                if (msgLen <= 0 || msgLen > 65536) break;

                byte[] msgBuf = new byte[msgLen];
                read = ReadExact(netStream, msgBuf, msgLen);
                if (read < msgLen) break;

                string json = Encoding.UTF8.GetString(msgBuf);
                lock (incomingMessages)
                {
                    incomingMessages.Enqueue(json);
                }
            }
        }
        catch (Exception e)
        {
            if (isRunning) Debug.LogWarning($"[LAN] Receive error: {e.Message}");
        }

        Enqueue(() =>
        {
            IsConnected = false;
            OnClientDisconnected?.Invoke();
        });
    }

    private int ReadExact(NetworkStream stream, byte[] buffer, int count)
    {
        int total = 0;
        while (total < count)
        {
            int read = stream.Read(buffer, total, count - total);
            if (read == 0) return total;
            total += read;
        }
        return total;
    }

    /// <summary>Send a message object (JSON + length prefix).</summary>
    public void SendMessage<T>(T msg) where T : class
    {
        if (netStream == null || !IsConnected) return;

        try
        {
            string json = JsonUtility.ToJson(msg);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] length = BitConverter.GetBytes(data.Length);

            netStream.Write(length, 0, 4);
            netStream.Write(data, 0, data.Length);
            netStream.Flush();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LAN] Send error: {e.Message}");
        }
    }

    // ========================================
    //  UPDATE (main thread dispatch)
    // ========================================

    private void Update()
    {
        // Dispatch queued actions
        lock (mainThreadQueue)
        {
            while (mainThreadQueue.Count > 0)
                mainThreadQueue.Dequeue()?.Invoke();
        }

        // Process incoming messages
        lock (incomingMessages)
        {
            while (incomingMessages.Count > 0)
            {
                string json = incomingMessages.Dequeue();
                OnMessageReceived?.Invoke(json);
            }
        }

        // Host: broadcast discovery
        if (IsHost && isRunning && InLobby)
        {
            broadcastTimer += Time.deltaTime;
            if (broadcastTimer >= BROADCAST_INTERVAL)
            {
                broadcastTimer = 0f;
                BroadcastDiscovery();
            }
        }

        // Host: periodic state sync during match
        if (IsHost && IsConnected && !InLobby)
        {
            syncTimer += Time.deltaTime;
            if (syncTimer >= SYNC_INTERVAL)
            {
                syncTimer = 0f;
                SendStateSync();
            }
        }
    }

    private void BroadcastDiscovery()
    {
        if (udpBroadcaster == null) return;

        try
        {
            var msg = new NetMessages.DiscoveryBroadcast
            {
                gameName = GAME_ID,
                hostName = SystemInfo.deviceName,
                port = GAME_PORT
            };

            string json = JsonUtility.ToJson(msg);
            byte[] data = Encoding.UTF8.GetBytes(json);
            var ep = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);
            udpBroadcaster.Send(data, data.Length, ep);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LAN] Broadcast error: {e.Message}");
        }
    }

    private void SendStateSync()
    {
        var mc = MatchController.Instance;
        if (mc == null || mc.State == null) return;

        var s = mc.State;
        var msg = new NetMessages.StateSyncMsg
        {
            playerEnergy = s.enemy.energy,    // Client is "enemy" from host's POV
            enemyEnergy = s.player.energy,     // Host is "enemy" from client's POV
            pLeftHP = s.enemyLeftTower != null ? s.enemyLeftTower.GetComponent<Damageable>().currentHp : 0f,
            pRightHP = s.enemyRightTower != null ? s.enemyRightTower.GetComponent<Damageable>().currentHp : 0f,
            pKingHP = s.enemyKing != null ? s.enemyKing.GetComponent<Damageable>().currentHp : 0f,
            eLeftHP = s.playerLeftTower != null ? s.playerLeftTower.GetComponent<Damageable>().currentHp : 0f,
            eRightHP = s.playerRightTower != null ? s.playerRightTower.GetComponent<Damageable>().currentHp : 0f,
            eKingHP = s.playerKing != null ? s.playerKing.GetComponent<Damageable>().currentHp : 0f,
            isGameOver = s.isGameOver
        };

        // Send client's hand (enemy hand from host POV)
        var hand = s.enemy.hand;
        msg.handCardIds = new string[hand.Count];
        for (int i = 0; i < hand.Count; i++)
            msg.handCardIds[i] = hand[i].id;
        msg.selectedIndex = s.enemy.selectedIndex;
        msg.deckCount = s.enemy.deckQueue.Count;

        SendMessage(msg);
    }

    // ========================================
    //  CLEANUP
    // ========================================

    private void StopDiscovery()
    {
        try { udpListener?.Close(); } catch { }
        udpListener = null;
    }

    public void Shutdown()
    {
        isRunning = false;
        InLobby = false;
        IsConnected = false;
        IsHost = false;
        IsClient = false;

        try { netStream?.Close(); } catch { }
        try { tcpClient?.Close(); } catch { }
        try { tcpListener?.Stop(); } catch { }
        try { udpBroadcaster?.Close(); } catch { }
        try { udpListener?.Close(); } catch { }

        netStream = null;
        tcpClient = null;
        tcpListener = null;
        udpBroadcaster = null;
        udpListener = null;

        Debug.Log("[LAN] Shutdown complete.");
    }

    private void Enqueue(Action action)
    {
        lock (mainThreadQueue) { mainThreadQueue.Enqueue(action); }
    }

    /// <summary>Get the local IP address for display.</summary>
    public static string GetLocalIP()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch { }
        return "127.0.0.1";
    }
}
