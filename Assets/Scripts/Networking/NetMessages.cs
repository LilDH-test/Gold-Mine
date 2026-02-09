using System;

/// <summary>
/// NetMessages — Serializable message types sent over LAN between host and client.
/// All messages are JSON-serialized with a "type" field for dispatching.
/// </summary>
public static class NetMessages
{
    // ---- Message type constants ----
    public const string PLAY_CARD = "play_card";
    public const string STATE_SYNC = "state_sync";
    public const string MATCH_START = "match_start";
    public const string MATCH_END = "match_end";
    public const string LOBBY_READY = "lobby_ready";
    public const string PING = "ping";
    public const string PONG = "pong";
    public const string SELECT_CARD = "select_card";

    // ---- Base wrapper ----
    [Serializable]
    public class NetMessage
    {
        public string type;
    }

    // ---- Client → Host: player played a card ----
    [Serializable]
    public class PlayCardMsg : NetMessage
    {
        public string cardId;
        public int lane;       // 0 or 1
        public int handIndex;  // which card slot was used

        public PlayCardMsg()
        {
            type = PLAY_CARD;
        }
    }

    // ---- Client → Host: player selected a card in hand ----
    [Serializable]
    public class SelectCardMsg : NetMessage
    {
        public int handIndex;

        public SelectCardMsg()
        {
            type = SELECT_CARD;
        }
    }

    // ---- Host → Client: periodic state snapshot ----
    [Serializable]
    public class StateSyncMsg : NetMessage
    {
        // Energy
        public float playerEnergy;
        public float enemyEnergy;

        // Tower HP (6 towers)
        public float pLeftHP;
        public float pRightHP;
        public float pKingHP;
        public float eLeftHP;
        public float eRightHP;
        public float eKingHP;

        // Hand (client's hand — serialized as card IDs)
        public string[] handCardIds;
        public int selectedIndex;
        public int deckCount;

        // Entity counts (for display)
        public int playerUnitCount;
        public int enemyUnitCount;

        public bool isGameOver;

        public StateSyncMsg()
        {
            type = STATE_SYNC;
        }
    }

    // ---- Host → Client: unit/pump spawned ----
    [Serializable]
    public class SpawnEntityMsg : NetMessage
    {
        public string entityType; // "unit" or "pump"
        public string side;
        public int lane;
        public string cardId;
        public float posX;
        public float posY;

        public SpawnEntityMsg()
        {
            type = "spawn_entity";
        }
    }

    // ---- Host → Client: damage applied to entity ----
    [Serializable]
    public class DamageMsg : NetMessage
    {
        public string targetId; // GameObject name
        public float amount;

        public DamageMsg()
        {
            type = "damage";
        }
    }

    // ---- Host → Client: match is starting ----
    [Serializable]
    public class MatchStartMsg : NetMessage
    {
        public string[] handCardIds; // client's initial hand
        public int deckCount;

        public MatchStartMsg()
        {
            type = MATCH_START;
        }
    }

    // ---- Host → Client: match ended ----
    [Serializable]
    public class MatchEndMsg : NetMessage
    {
        public bool playerWon; // from the CLIENT's perspective
        public int xpGain;
        public int goldGain;

        public MatchEndMsg()
        {
            type = MATCH_END;
        }
    }

    // ---- Lobby ----
    [Serializable]
    public class LobbyReadyMsg : NetMessage
    {
        public string playerName;

        public LobbyReadyMsg()
        {
            type = LOBBY_READY;
        }
    }

    // ---- Ping / Pong ----
    [Serializable]
    public class PingMsg : NetMessage
    {
        public float timestamp;
        public PingMsg() { type = PING; }
    }

    [Serializable]
    public class PongMsg : NetMessage
    {
        public float timestamp;
        public PongMsg() { type = PONG; }
    }

    // ---- Discovery broadcast (UDP) ----
    [Serializable]
    public class DiscoveryBroadcast
    {
        public string gameName;  // "GoldMine"
        public string hostName;
        public int port;
    }
}
