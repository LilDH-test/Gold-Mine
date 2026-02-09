using UnityEngine;

/// <summary>
/// LanMatchHandler — Bridges LanManager messages with MatchController.
/// On the HOST: receives client card-plays and executes them as "enemy" actions.
/// On the CLIENT: receives state syncs and updates local display.
/// Attach alongside MatchController.
/// </summary>
public class LanMatchHandler : MonoBehaviour
{
    private void OnEnable()
    {
        if (LanManager.Instance != null)
        {
            LanManager.Instance.OnMessageReceived += HandleMessage;
            LanManager.Instance.OnClientConnected += OnPeerConnected;
            LanManager.Instance.OnClientDisconnected += OnPeerDisconnected;
        }

        if (MatchController.Instance != null)
        {
            MatchController.Instance.OnMatchEnd += OnMatchEnd;
        }
    }

    private void OnDisable()
    {
        if (LanManager.Instance != null)
        {
            LanManager.Instance.OnMessageReceived -= HandleMessage;
            LanManager.Instance.OnClientConnected -= OnPeerConnected;
            LanManager.Instance.OnClientDisconnected -= OnPeerDisconnected;
        }

        if (MatchController.Instance != null)
        {
            MatchController.Instance.OnMatchEnd -= OnMatchEnd;
        }
    }

    // ========================================
    //  MESSAGE DISPATCH
    // ========================================

    private void HandleMessage(string json)
    {
        // Parse the base message to get type
        var baseMsg = JsonUtility.FromJson<NetMessages.NetMessage>(json);
        if (baseMsg == null) return;

        if (LanManager.Instance.IsHost)
        {
            HandleHostMessage(baseMsg.type, json);
        }
        else
        {
            HandleClientMessage(baseMsg.type, json);
        }
    }

    // ========================================
    //  HOST — receives commands from the remote player (who is "enemy")
    // ========================================

    private void HandleHostMessage(string type, string json)
    {
        var mc = MatchController.Instance;
        if (mc == null) return;

        switch (type)
        {
            case NetMessages.PLAY_CARD:
            {
                var msg = JsonUtility.FromJson<NetMessages.PlayCardMsg>(json);
                if (msg == null || mc.State.isGameOver) return;

                // The remote player is "enemy" on the host
                var party = mc.State.enemy;
                if (party.hand.Count == 0) return;

                // Find the card by ID in hand
                int cardIndex = -1;
                for (int i = 0; i < party.hand.Count; i++)
                {
                    if (party.hand[i].id == msg.cardId)
                    {
                        cardIndex = i;
                        break;
                    }
                }
                if (cardIndex < 0) cardIndex = msg.handIndex;
                if (cardIndex < 0 || cardIndex >= party.hand.Count) return;

                party.selectedIndex = cardIndex;
                var card = party.hand[party.selectedIndex];

                if (!party.TrySpend(card.cost)) return;

                if (card.type == CardDatabase.CardType.Unit)
                    mc.SpawnUnit("enemy", msg.lane, card);
                else if (card.type == CardDatabase.CardType.Pump)
                    mc.SpawnPump("enemy", msg.lane, card);

                DeckHelper.RotateHand(party);

                // Send spawn notification to client
                var spawnMsg = new NetMessages.SpawnEntityMsg
                {
                    entityType = card.type == CardDatabase.CardType.Unit ? "unit" : "pump",
                    side = "player", // from CLIENT's perspective, enemy units are "player"... wait no
                    lane = msg.lane,
                    cardId = card.id
                };
                // Actually from client's POV: the host's enemy = client's player side
                // The client sees their own units as "player". The enemy side from host is client's player.
                // So when client plays a card, host spawns "enemy", but client already knows it's their unit.
                // We send it so client can render it locally if needed.
                spawnMsg.side = "enemy"; // on client display, their own units show as "player" via perspective flip
                LanManager.Instance.SendMessage(spawnMsg);
                break;
            }

            case NetMessages.SELECT_CARD:
            {
                var msg = JsonUtility.FromJson<NetMessages.SelectCardMsg>(json);
                if (msg != null)
                {
                    mc.State.enemy.selectedIndex = Mathf.Clamp(msg.handIndex, 0, mc.State.enemy.hand.Count - 1);
                }
                break;
            }

            case NetMessages.LOBBY_READY:
            {
                var msg = JsonUtility.FromJson<NetMessages.LobbyReadyMsg>(json);
                if (msg != null)
                {
                    LanManager.Instance.RemotePlayerName = msg.playerName;
                    Debug.Log($"[LAN] Remote player ready: {msg.playerName}");
                }
                break;
            }

            case NetMessages.PING:
            {
                var ping = JsonUtility.FromJson<NetMessages.PingMsg>(json);
                var pong = new NetMessages.PongMsg { timestamp = ping.timestamp };
                LanManager.Instance.SendMessage(pong);
                break;
            }
        }
    }

    // ========================================
    //  CLIENT — receives state updates from host
    // ========================================

    private void HandleClientMessage(string type, string json)
    {
        switch (type)
        {
            case NetMessages.STATE_SYNC:
            {
                var msg = JsonUtility.FromJson<NetMessages.StateSyncMsg>(json);
                if (msg == null) return;

                // Update local display state
                var mc = MatchController.Instance;
                if (mc == null || mc.State == null) return;

                // From client POV: msg.playerEnergy is OUR energy, msg.enemyEnergy is theirs
                mc.State.player.energy = msg.playerEnergy;
                mc.State.enemy.energy = msg.enemyEnergy;

                // Tower HP
                if (mc.State.playerLeftTower != null)
                    mc.State.playerLeftTower.GetComponent<Damageable>().currentHp = msg.pLeftHP;
                if (mc.State.playerRightTower != null)
                    mc.State.playerRightTower.GetComponent<Damageable>().currentHp = msg.pRightHP;
                if (mc.State.playerKing != null)
                    mc.State.playerKing.GetComponent<Damageable>().currentHp = msg.pKingHP;
                if (mc.State.enemyLeftTower != null)
                    mc.State.enemyLeftTower.GetComponent<Damageable>().currentHp = msg.eLeftHP;
                if (mc.State.enemyRightTower != null)
                    mc.State.enemyRightTower.GetComponent<Damageable>().currentHp = msg.eRightHP;
                if (mc.State.enemyKing != null)
                    mc.State.enemyKing.GetComponent<Damageable>().currentHp = msg.eKingHP;

                // Hand
                if (msg.handCardIds != null)
                {
                    mc.State.player.hand.Clear();
                    foreach (var id in msg.handCardIds)
                    {
                        var card = CardDatabase.GetCard(id);
                        if (card != null) mc.State.player.hand.Add(card);
                    }
                    mc.State.player.selectedIndex = msg.selectedIndex;
                }

                mc.State.isGameOver = msg.isGameOver;
                break;
            }

            case NetMessages.MATCH_START:
            {
                var msg = JsonUtility.FromJson<NetMessages.MatchStartMsg>(json);
                if (msg == null) return;

                var mc = MatchController.Instance;
                if (mc != null)
                {
                    // Client starts a local match for visuals
                    mc.StartLanClientMatch();

                    // Load our hand from the host's assignment
                    mc.State.player.hand.Clear();
                    foreach (var id in msg.handCardIds)
                    {
                        var card = CardDatabase.GetCard(id);
                        if (card != null) mc.State.player.hand.Add(card);
                    }

                    mc.hud?.RefreshHand(mc.State);
                }
                break;
            }

            case NetMessages.MATCH_END:
            {
                var msg = JsonUtility.FromJson<NetMessages.MatchEndMsg>(json);
                if (msg == null) return;

                var mc = MatchController.Instance;
                if (mc != null)
                {
                    mc.State.isGameOver = true;
                    // Award locally
                    GameManager.Instance.AwardMatch(msg.playerWon);
                }
                break;
            }

            case NetMessages.PONG:
            {
                var msg = JsonUtility.FromJson<NetMessages.PongMsg>(json);
                float rtt = Time.realtimeSinceStartup - msg.timestamp;
                Debug.Log($"[LAN] Ping: {(rtt * 1000f):F0}ms");
                break;
            }
        }
    }

    // ========================================
    //  HOST EVENTS
    // ========================================

    /// <summary>Start the LAN match (host only). Called from LanLobbyUI.</summary>
    public void HostStartMatch()
    {
        if (!LanManager.Instance.IsHost) return;

        var mc = MatchController.Instance;
        if (mc == null) return;

        LanManager.Instance.InLobby = false;

        // Start the match normally (host side)
        mc.StartLanHostMatch();

        // Tell client to start
        var msg = new NetMessages.MatchStartMsg();
        var enemyHand = mc.State.enemy.hand;
        msg.handCardIds = new string[enemyHand.Count];
        for (int i = 0; i < enemyHand.Count; i++)
            msg.handCardIds[i] = enemyHand[i].id;
        msg.deckCount = mc.State.enemy.deckQueue.Count;

        LanManager.Instance.SendMessage(msg);
    }

    private void OnMatchEnd(bool hostWon)
    {
        if (!LanManager.Instance.IsHost) return;

        // Tell client their result (opposite of host)
        var msg = new NetMessages.MatchEndMsg
        {
            playerWon = !hostWon,
            xpGain = !hostWon ? Mathf.FloorToInt(45 + GameManager.Instance.CurrentLevel * 6)
                              : Mathf.FloorToInt(18 + GameManager.Instance.CurrentLevel * 3),
            goldGain = !hostWon ? Mathf.FloorToInt(60 + GameManager.Instance.CurrentLevel * 10) : 0
        };
        LanManager.Instance.SendMessage(msg);
    }

    private void OnPeerConnected()
    {
        Debug.Log("[LAN] Peer connected!");
    }

    private void OnPeerDisconnected()
    {
        Debug.Log("[LAN] Peer disconnected!");
    }
}
