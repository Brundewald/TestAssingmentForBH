using System;
using Code.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    internal class LobbyView : MonoBehaviour, IScreen
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private TMP_InputField _inputField;
        private bool _tryingToConnect;

        public event Action<bool> SwitchScreen = delegate {};
        public event Action HostGame = delegate { };
        public event Action<string> JoinGame = delegate { };
        
        public void ChangeScreenState(bool state)
        {
            gameObject.SetActive(state);
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(SwitchToMain);
            _hostButton.onClick.AddListener(StartHost);
            _joinButton.onClick.AddListener(JoinToServer);
        }

        private void OnEnable()
        {
            CustomNetworkManager.ClientConnected += OnClientConnected;
            CustomNetworkManager.ClientDisconnected += OnClientDisconnected;
        }

        private void FixedUpdate()
        {
            if (!_tryingToConnect)
                _joinButton.interactable = !_inputField.text.Equals("");
        }

        private void OnDisable()
        {
            _inputField.text = "";
            CustomNetworkManager.ClientConnected -= OnClientConnected;
            CustomNetworkManager.ClientDisconnected -= OnClientDisconnected;
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
            _hostButton.onClick.RemoveAllListeners();
            _joinButton.onClick.RemoveAllListeners();
        }

        private void StartHost()
        {
            HostGame.Invoke();
        }

        private void JoinToServer()
        {
            var networkAddress = _inputField.text;
            JoinGame.Invoke(networkAddress);
            _tryingToConnect = true;
            _joinButton.interactable = false;
        }

        private void SwitchToMain()
        {
            SwitchScreen.Invoke(true);
            ChangeScreenState(false);
        }

        public void OnClientConnected()
        {
            _joinButton.interactable = true;
            ChangeScreenState(false);
        }

        public void OnClientDisconnected()
        {
            _joinButton.interactable = true;
        }
    }
}