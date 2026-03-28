using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleBeasts
{
    public class BattleCommentaryLog : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private int maxEntries = 10;

        private readonly Queue<string> _entries = new Queue<string>();

        public void AddEntry(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            _entries.Enqueue(message);

            while (_entries.Count > maxEntries)
                _entries.Dequeue();

            Refresh();
            Debug.Log($"[BattleBeasts] {message}");
        }

        private void Refresh()
        {
            if (logText == null)
                return;

            logText.text = string.Join("\n", _entries.ToArray());
        }
    }
}
