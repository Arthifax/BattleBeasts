using UnityEngine;

namespace BattleBeasts
{
    [CreateAssetMenu(fileName = "Battle Rules", menuName = "Battle Beasts/Grid Prototype/Battle Rules")]
    public class BattleRulesData : ScriptableObject
    {
        [Header("Grid")]
        [Min(1)] public int gridWidth = 35;
        [Min(1)] public int gridHeight = 17;
        [Min(0.1f)] public float tileSize = 1f;
        public Vector3 gridOrigin = Vector3.zero;

        [Header("Time")]
        [Range(0.05f, 1f)] public float commandSlowMoScale = 0.4f;

        [Header("Recovery")]
        [Min(0f)] public float defaultMoveRecovery = 0.15f;
        [Min(0f)] public float dodgeRecovery = 0.35f;

        [Header("Enemy AI")]
        [Min(0f)] public float enemyDecisionInterval = 1.2f;
        [Min(0f)] public float enemyPostActionDelay = 1.8f;

        [Header("Telegraph")]
        [Min(0f)] public float telegraphHeight = 0.03f;
    }
}
