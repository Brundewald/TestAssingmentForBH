using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.PlayerUI
{
    public sealed class PlayerLobbyUIView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_InputField _nicknameInput;
        [SerializeField] private Button _spawnButton;
    
        private int _minNicknameLength = 3;
        private PlayerLobbyView _lobbyView;

        public event Action BackToMain = delegate {  };

        public event Action<string> PlayerReady = delegate {  };

        /// <summary>
        /// Set example of NetworkBehavior and subscribe to it events
        /// </summary>
        /// <param name="playerLobbyView"></param>
        public void SetupLobbyObject(PlayerLobbyView playerLobbyView)
        {
            _lobbyView = playerLobbyView;
            BackToMain += _lobbyView.OnBackToMain;
            PlayerReady += _lobbyView.OnPlayerReady;
        }

        /// <summary>
        /// Add listeners to back and ready buttons 
        /// </summary>
        private void Awake()
        {
            _backButton.onClick.AddListener(BackButtonPressed);
            _spawnButton.onClick.AddListener(OnPlayerReady);
        }

        /// <summary>
        /// Checks entered name to min size 
        /// </summary>
        private void FixedUpdate()
        {
            _spawnButton.interactable = !_nicknameInput.text.Equals("") && _nicknameInput.text.Length >= _minNicknameLength;
        }

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
            _spawnButton.onClick.RemoveAllListeners();
            BackToMain -= _lobbyView.OnBackToMain;
            PlayerReady -= _lobbyView.OnPlayerReady;
        }

        /// <summary>
        /// Rise event on back button pressed
        /// </summary>
        private void BackButtonPressed()
        {
            BackToMain.Invoke();
        }

        /// <summary>
        /// Set player name, rise event on player ready and deactivate UI
        /// </summary>
        private void OnPlayerReady()
        {
            var playerNickname = _nicknameInput.text;
            PlayerReady.Invoke(playerNickname);
            gameObject.SetActive(false);
        }
    }
}
