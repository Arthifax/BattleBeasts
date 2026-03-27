using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BattleBeasts
{
    public class BattleCommentaryLog : MonoBehaviour
    {
        [SerializeField] private TMP_Text commentaryText;
        [SerializeField] private int maxLines = 8;

        private readonly Queue<string> lines = new Queue<string>();

        public void AddLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            lines.Enqueue(line);
            while (lines.Count > maxLines)
            {
                lines.Dequeue();
            }

            RefreshText();
        }

        private void RefreshText()
        {
            if (commentaryText == null)
            {
                return;
            }

            commentaryText.text = string.Join("\n", lines);
        }
    }
}
