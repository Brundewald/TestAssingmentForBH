using TMPro;
using UnityEngine;

namespace Code.UI.PlayerUI
{
    public sealed class TableField : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerNameField;
        [SerializeField] private TextMeshProUGUI _scoreField;

        /// <summary>
        /// Set name on players table
        /// </summary>
        /// <param name="name"></param>
        public void UpdateName(string name)
        {
            _playerNameField.text = name;
        }
        
        /// <summary>
        /// Set score on players table
        /// </summary>
        /// <param name="score"></param>
        public void UpdateScores(int score)
        {
            _scoreField.text = score.ToString();
        }
    }
}