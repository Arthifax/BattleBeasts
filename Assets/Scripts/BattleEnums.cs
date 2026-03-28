using System;
using UnityEngine;

namespace BattleBeasts
{
    public enum BeastType
    {
        Fire,
        Water,
        Grass,
        Neutral
    }

    public enum DamageCategory
    {
        Physical,
        Special,
        Status
    }

    public enum TeamAlignment
    {
        Player,
        Enemy
    }

    public enum BeastActionState
    {
        Idle,
        Moving,
        Windup,
        Recovering,
        Disabled
    }

    public enum CommandType
    {
        None,
        MoveToTile,
        UseBasicAttack,
        UseSpecialAttack,
        DodgeAway
    }

    [Serializable]
    public struct GridCoord : IEquatable<GridCoord>
    {
        public int x;
        public int y;

        public GridCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(GridCoord other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is GridCoord other && Equals(other);
        public override int GetHashCode() => (x * 397) ^ y;
        public static bool operator ==(GridCoord a, GridCoord b) => a.Equals(b);
        public static bool operator !=(GridCoord a, GridCoord b) => !a.Equals(b);
        public override string ToString() => $"({x},{y})";

        public static float Distance(GridCoord a, GridCoord b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static int ChebyshevDistance(GridCoord a, GridCoord b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }
    }

    [Serializable]
    public struct BeastStats
    {
        [Min(1)] public int maxHP;
        [Min(1)] public int attack;
        [Min(1)] public int defense;
        [Min(1)] public int specialAttack;
        [Min(1)] public int specialDefense;
        [Min(1)] public int reflexes;
        [Min(1)] public int agility;
    }
}
