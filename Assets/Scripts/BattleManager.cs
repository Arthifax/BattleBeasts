using UnityEngine;

namespace BattleBeasts
{
    public class BattleManager : MonoBehaviour
    {
        [Header("Rules")]
        [SerializeField] private BattleRulesData rules;

        [Header("Scene References")]
        [SerializeField] private BattleGridManager grid;
        [SerializeField] private AttackTelegraphSystem telegraphs;
        [SerializeField] private BattleUIManager uiManager;
        [SerializeField] private BattleCommentaryLog commentaryLog;
        [SerializeField] private PlayerCommandInput playerInput;

        [Header("Beasts")]
        [SerializeField] private BeastController playerBeast;
        [SerializeField] private BeastController enemyBeast;
        [SerializeField] private EnemyBeastAI enemyAI;

        private bool _battleActive;
        private float _currentTimeScale = 1f;

        public BattleRulesData Rules => rules;
        public BattleGridManager Grid => grid;
        public AttackTelegraphSystem Telegraphs => telegraphs;
        public BattleUIManager UIManager => uiManager;
        public bool BattleActive => _battleActive;

        private void Start()
        {
            if (rules == null)
            {
                Debug.LogError("BattleManager needs BattleRulesData.");
                enabled = false;
                return;
            }

            grid.Initialize(rules);

            if (telegraphs != null)
                telegraphs.Initialize(grid);

            playerBeast.Initialize(this, grid, enemyBeast, grid.PlayerStartTile);
            enemyBeast.Initialize(this, grid, playerBeast, grid.EnemyStartTile);

            if (enemyAI == null)
                enemyAI = enemyBeast.GetComponent<EnemyBeastAI>();

            if (enemyAI != null)
                enemyAI.Initialize(this);

            if (playerInput != null)
                playerInput.Initialize(this, playerBeast);

            _battleActive = true;
            Log("Battle started.");
        }

        private void Update()
        {
            if (uiManager != null)
            {
                uiManager.UpdateBeastUI(playerBeast, true);
                uiManager.UpdateBeastUI(enemyBeast, false);
                uiManager.SetTimeScale(_currentTimeScale);
            }
        }

        public void SetBattleTimeScale(float scale)
        {
            _currentTimeScale = Mathf.Clamp(scale, 0.05f, 1f);
            Time.timeScale = _currentTimeScale;
        }

        public void NotifyBeastDefeated(BeastController defeated)
        {
            if (!_battleActive)
                return;

            _battleActive = false;
            SetBattleTimeScale(1f);

            string winner = defeated == playerBeast ? enemyBeast.Data.speciesName : playerBeast.Data.speciesName;
            Log($"{defeated.Data.speciesName} was defeated. Winner: {winner}");
        }

        public void Log(string message)
        {
            commentaryLog?.AddEntry(message);
        }
    }
}
