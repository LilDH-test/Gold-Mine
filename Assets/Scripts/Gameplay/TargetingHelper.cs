using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TargetingHelper — Static utility for finding valid targets for a given attacker side.
/// Scans all Damageables in the scene. Towers/Kings use GameState references,
/// units and pumps are found via FindObjectsByType.
/// </summary>
public static class TargetingHelper
{
    /// <summary>Get all living enemy Damageables for the given attacker side.</summary>
    public static List<Damageable> GetAllTargetsFor(string attackerSide)
    {
        var list = new List<Damageable>();
        var state = MatchController.Instance.State;
        string defSide = attackerSide == "player" ? "enemy" : "player";

        // Enemy towers
        TowerController[] towers = defSide == "player"
            ? state.AllPlayerTowers
            : state.AllEnemyTowers;

        foreach (var t in towers)
        {
            if (t == null) continue;
            var d = t.GetComponent<Damageable>();
            if (d == null || !d.IsAlive) continue;

            // King only targetable if unlocked
            if (t.towerKind == "king" && !state.KingUnlocked(defSide))
                continue;

            list.Add(d);
        }

        // Units
        var units = Object.FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        foreach (var u in units)
        {
            if (!u.IsAlive) continue;
            var d = u.GetComponent<Damageable>();
            if (d != null && d.side == defSide && d.IsAlive)
                list.Add(d);
        }

        // Pumps
        var pumps = Object.FindObjectsByType<PumpController>(FindObjectsSortMode.None);
        foreach (var p in pumps)
        {
            if (!p.IsAlive) continue;
            var d = p.GetComponent<Damageable>();
            if (d != null && d.side == defSide && d.IsAlive)
                list.Add(d);
        }

        return list;
    }

    /// <summary>Find the nearest living enemy within range of position.</summary>
    public static Damageable FindNearest(Vector2 position, string attackerSide, float range)
    {
        var targets = GetAllTargetsFor(attackerSide);
        Damageable best = null;
        float bestDist = float.MaxValue;
        float rangeSq = range * range;

        foreach (var t in targets)
        {
            float distSq = ((Vector2)t.transform.position - position).sqrMagnitude;
            if (distSq <= rangeSq && distSq < bestDist)
            {
                bestDist = distSq;
                best = t;
            }
        }

        return best;
    }
}
