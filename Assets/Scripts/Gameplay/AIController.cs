using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AIController — Enemy AI decision maker. Mirrors the HTML prototype logic.
/// Picks an affordable card, prefers placing a pump if none exist, and picks the
/// weaker-health-tower lane to attack.
/// </summary>
public class AIController : MonoBehaviour
{
    public void Tick(GameState state, float dt)
    {
        if (state.isGameOver) return;

        state.aiTimer += dt;
        if (state.aiTimer < state.aiInterval) return;

        state.aiTimer = 0f;
        state.aiInterval = Random.Range(GameConstants.AI_MIN_INTERVAL, GameConstants.AI_MAX_INTERVAL);

        var party = state.enemy;
        if (party.hand == null || party.hand.Count == 0) return;

        // Find affordable cards
        var affordable = new List<(CardDatabase.CardDef card, int index)>();
        for (int i = 0; i < party.hand.Count; i++)
        {
            if (party.energy >= party.hand[i].cost)
                affordable.Add((party.hand[i], i));
        }

        if (affordable.Count == 0) return;

        // Default: random pick
        var pick = affordable[Random.Range(0, affordable.Count)];

        // Bias: prefer placing a pump if none exist
        bool hasPump = false;
        var pumps = FindObjectsByType<PumpController>(FindObjectsSortMode.None);
        foreach (var p in pumps)
        {
            if (p.IsAlive && p.Side == "enemy") { hasPump = true; break; }
        }

        if (!hasPump)
        {
            var pumpPick = affordable.FirstOrDefault(x => x.card.type == CardDatabase.CardType.Pump);
            if (pumpPick.card != null && Random.value < GameConstants.AI_PUMP_BIAS)
                pick = pumpPick;
        }

        // Choose lane (bias toward weaker player tower)
        int lane = ChooseLane(state);

        // Spend
        party.selectedIndex = pick.index;
        var card = party.hand[party.selectedIndex];

        if (!party.TrySpend(card.cost)) return;

        // Deploy
        if (card.type == CardDatabase.CardType.Unit)
        {
            MatchController.Instance.SpawnUnit("enemy", lane, card);
        }
        else if (card.type == CardDatabase.CardType.Pump)
        {
            MatchController.Instance.SpawnPump("enemy", lane, card);
        }

        // Rotate hand
        DeckHelper.RotateHand(party);
    }

    private int ChooseLane(GameState state)
    {
        float leftHP = state.playerLeftTower.IsAlive
            ? state.playerLeftTower.GetComponent<Damageable>().HpRatio : 0f;
        float rightHP = state.playerRightTower.IsAlive
            ? state.playerRightTower.GetComponent<Damageable>().HpRatio : 0f;

        float biasLeft = (leftHP < rightHP) ? 0.62f : 0.38f;
        return Random.value < biasLeft ? 0 : 1;
    }
}
