using UnityEngine;
using UnityEngine.UI;

namespace BattleBeasts
{
    public class BattleUIManager : MonoBehaviour
    {
        [Header("Player UI")]
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private Slider playerWindupSlider;
        [SerializeField] private Text playerStateText;

        [Header("Enemy UI")]
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private Slider enemyWindupSlider;
        [SerializeField] private Text enemyStateText;

        [Header("General UI")]
        [SerializeField] private Text commandHintText;
        [SerializeField] private Text hoveredTileText;
        [SerializeField] private Text timeScaleText;

        public void UpdateBeastUI(BeastController beast, bool isPlayer)
        {
            if (beast == null || beast.Data == null)
                return;

            Slider hp = isPlayer ? playerHpSlider : enemyHpSlider;
            Slider windup = isPlayer ? playerWindupSlider : enemyWindupSlider;
            Text state = isPlayer ? playerStateText : enemyStateText;

            if (hp != null)
            {
                hp.maxValue = beast.Data.baseStats.maxHP;
                hp.value = beast.Runtime.currentHP;
            }

            if (windup != null)
            {
                float duration = beast.Runtime.currentWindupDuration;
                windup.maxValue = duration <= 0f ? 1f : duration;
                windup.value = beast.Runtime.currentWindupElapsed;
            }

            if (state != null)
            {
                state.text = $"{beast.Data.speciesName}\nHP {beast.Runtime.currentHP}/{beast.Data.baseStats.maxHP}\n" +
                             $"State: {beast.Runtime.actionState}\n" +
                             $"Basic CD: {beast.Runtime.basicCooldownRemaining:0.0}  " +
                             $"Special CD: {beast.Runtime.specialCooldownRemaining:0.0}  " +
                             $"Dodge CD: {beast.Runtime.dodgeCooldownRemaining:0.0}";
            }
        }

        public void SetHoveredTile(GridCoord? tile)
        {
            if (hoveredTileText == null)
                return;

            hoveredTileText.text = tile.HasValue ? $"Hover Tile: {tile.Value.x},{tile.Value.y}" : "Hover Tile: -";
        }

        public void SetCommandHint(string text)
        {
            if (commandHintText != null)
                commandHintText.text = text;
        }

        public void SetTimeScale(float scale)
        {
            if (timeScaleText != null)
                timeScaleText.text = $"Time Scale: {scale:0.00}";
        }
    }
}
