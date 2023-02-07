using TMPro;
using UnityEngine;

namespace Code.UI.PlayerUI
{
    public class TableField : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerNameField;
        [SerializeField] private TextMeshProUGUI _scoreField;

        public void UpdateName(string name)
        {
            _playerNameField.text = name;
        }
        
        public void UpdateScores(int score)
        {
            _scoreField.text = score.ToString();
        }
    }
}