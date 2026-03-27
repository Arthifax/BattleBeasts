using UnityEngine;
using UnityEngine.UI;

namespace BattleBeasts
{
    public class BattleUIManager : MonoBehaviour
    {
        [Header("Player UI")]
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private Slider playerCastSlider;

        [Header("Enemy UI")]
        [SerializeField] private Slider enemyHealthSlider;
        [SerializeField] private Slider enemyCastSlider;

        private BeastController playerBeast;
        private BeastController enemyBeast;

        public void Bind(BeastController player, BeastController enemy)
        {
            playerBeast = player;
            enemyBeast = enemy;
            RefreshInstant();
        }

        private void Update()
        {
            RefreshInstant();
        }

        private void RefreshInstant()
        {
            UpdateHealth(playerHealthSlider, playerBeast);
            UpdateHealth(enemyHealthSlider, enemyBeast);
            UpdateCast(playerCastSlider, playerBeast);
            UpdateCast(enemyCastSlider, enemyBeast);
        }

        private void UpdateHealth(Slider slider, BeastController beast)
        {
            if (slider == null || beast?.BeastData == null)
            {
                return;
            }

            slider.maxValue = beast.BeastData.baseStats.maxHP;
            slider.value = beast.State.currentHP;
        }

        private void UpdateCast(Slider slider, BeastController beast)
        {
            if (slider == null || beast == null)
            {
                return;
            }

            bool active = beast.State.currentBattleState == BeastBattleState.WindingUp || beast.State.currentBattleState == BeastBattleState.Executing;
            slider.value = active ? 1f : 0f;
        }
    }
}
