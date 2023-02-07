using System;
using System.Collections;
using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public sealed class GameOverScreenView: NetworkBehaviour
    {
        private const string NameMessage = "Winner is: %value%";
        private const string TimeMessage = "Next round in %value%";
        
        [SerializeField] private MessageWindow _messageWindowPrefab;

        private MessageWindow _messageWindow;
        
        public event Action TimerExpired = delegate {  };

        /// <summary>
        /// Register custom network message handler
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            NetworkClient.RegisterHandler<WinMessage>(ShowGameOver);
        }

        /// <summary>
        /// Instantiate end game window with winner name and countdown to restart.
        /// </summary>
        /// <param name="winMessage">Network message with countdown time and winner name</param>
        private void ShowGameOver(WinMessage winMessage)
        {
            _messageWindow = Instantiate(_messageWindowPrefab, transform);
            SetVictoryMessage(winMessage.Name);
            RunCountDownTimer(winMessage.TimeToReset);
        }

        /// <summary>
        /// Setup victory message
        /// </summary>
        /// <param name="name">winnerName</param>
        private void SetVictoryMessage(string name)
        {
            _messageWindow.WinMessageField.text = NameMessage.Replace("%value%", name);
        }

        /// <summary>
        /// Start countdown corutine
        /// </summary>
        /// <param name="countDown"></param>
        private void RunCountDownTimer(float countDown)
        {
            StartCoroutine(CountDownRoutine(countDown));
        }

        /// <summary>
        /// Run countdown to restart and rise event on timer elapsing
        /// </summary>
        /// <param name="countDown">time</param>
        /// <returns>null</returns>
        private IEnumerator CountDownRoutine(float countDown)
        {
            while (countDown > 0)
            {
                countDown -= Time.deltaTime;
                _messageWindow.TimerField.text = TimeMessage.Replace("%value%", $"{countDown:0}");
                yield return null;
            }
            Destroy(_messageWindow.gameObject);
            TimerExpired.Invoke();
        }
    }
}