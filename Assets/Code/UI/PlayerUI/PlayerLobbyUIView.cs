using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.PlayerUI
{
    public class PlayerLobbyUIView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_InputField _nicknameInput;
        [SerializeField] private Button _spawnButton;
    
        private int _minNicknameLength = 3;
        private PlayerLobbyView _lobbyView;

        public event Action BackToMain = delegate {  };

        public event Action<string> PlayerReady = delegate {  };

        public void SetupLobbyObject(PlayerLobbyView playerLobbyView)
        {
            _lobbyView = playerLobbyView;
            BackToMain += _lobbyView.OnBackToMain;
            PlayerReady += _lobbyView.OnPlayerReady;
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(BackButtonPressed);
            _spawnButton.onClick.AddListener(SpawnPlayer);
        }

        private void FixedUpdate()
        {
            _spawnButton.interactable = !_nicknameInput.text.Equals("") && _nicknameInput.text.Length >= _minNicknameLength;
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
            _spawnButton.onClick.RemoveAllListeners();
            BackToMain -= _lobbyView.OnBackToMain;
            PlayerReady -= _lobbyView.OnPlayerReady;
        }

        private void BackButtonPressed()
        {
            BackToMain.Invoke();
        }

        private void SpawnPlayer()
        {
            var playerNickname = _nicknameInput.text;
            PlayerReady.Invoke(playerNickname);
            gameObject.SetActive(false);
        }
    }
}
