using TMPro;
using UnityEngine;

namespace Code.Networking
{
    public sealed class MessageWindow: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _winMessageField;
        [SerializeField] private TextMeshProUGUI _timerField;

        public TextMeshProUGUI WinMessageField => _winMessageField;
        public TextMeshProUGUI TimerField => _timerField;
    }
}