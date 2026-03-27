using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleBeasts
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Battle Setup")]
        [SerializeField] private List<BeastController> participants = new List<BeastController>();
        [SerializeField] private bool autoFindParticipants = true;
        [SerializeField] private bool autoStartOnPlay = true;

        [Header("References")]
        [SerializeField] private BattleCommentaryLog commentaryLog;
        [SerializeField] private BattleUIManager battleUIManager;

        public bool BattleActive { get; private set; }
        public BeastController PlayerBeast { get; private set; }
        public BeastController EnemyBeast { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (autoFindParticipants)
            {
                participants = FindObjectsByType<BeastController>(FindObjectsSortMode.None).ToList();
            }

            AssignTeamsAndOpponents();

            if (autoStartOnPlay)
            {
                StartBattle();
            }
        }

        private void Update()
        {
            if (!BattleActive)
            {
                return;
            }

            if (PlayerBeast == null || EnemyBeast == null)
            {
                return;
            }

            if (PlayerBeast.State.IsDefeated || EnemyBeast.State.IsDefeated)
            {
                EndBattle();
            }
        }

        public void StartBattle()
        {
            if (PlayerBeast == null || EnemyBeast == null)
            {
                Debug.LogWarning("BattleManager could not start battle. A player beast and enemy beast are required.");
                return;
            }

            BattleActive = true;
            PlayerBeast.BeginBattle();
            EnemyBeast.BeginBattle();
            Log($"Battle start! {PlayerBeast.DisplayName} engages {EnemyBeast.DisplayName}!");
        }

        public void EndBattle()
        {
            if (!BattleActive)
            {
                return;
            }

            BattleActive = false;

            string result;
            if (PlayerBeast.State.IsDefeated && EnemyBeast.State.IsDefeated)
            {
                result = "Draw! Both beasts are down.";
            }
            else if (EnemyBeast.State.IsDefeated)
            {
                result = $"{EnemyBeast.DisplayName} is defeated! {PlayerBeast.DisplayName} wins!";
            }
            else
            {
                result = $"{PlayerBeast.DisplayName} is defeated! {EnemyBeast.DisplayName} wins!";
            }

            Log(result);
        }

        public void RegisterParticipant(BeastController beastController)
        {
            if (beastController != null && !participants.Contains(beastController))
            {
                participants.Add(beastController);
            }
        }

        public void Log(string message)
        {
            if (commentaryLog != null)
            {
                commentaryLog.AddLine(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        private void AssignTeamsAndOpponents()
        {
            PlayerBeast = participants.FirstOrDefault(p => p != null && p.TeamAlignment == TeamAlignment.Player);
            EnemyBeast = participants.FirstOrDefault(p => p != null && p.TeamAlignment == TeamAlignment.Enemy);

            if (PlayerBeast != null && EnemyBeast != null)
            {
                PlayerBeast.SetOpponent(EnemyBeast);
                EnemyBeast.SetOpponent(PlayerBeast);
            }

            if (battleUIManager != null)
            {
                battleUIManager.Bind(PlayerBeast, EnemyBeast);
            }
        }
    }
}
