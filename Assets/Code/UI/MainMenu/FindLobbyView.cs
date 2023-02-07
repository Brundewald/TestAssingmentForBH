using System;
using Code.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public sealed class FindLobbyView : MonoBehaviour, IScreen
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private TMP_InputField _inputField;
        private bool _tryingToConnect;

        public event Action<bool> SwitchScreen = delegate {};
        public event Action HostGame = delegate { };
        public event Action<string> JoinGame = delegate { };
        
        /// <summary>
        /// Change screen state on screen switch event
        /// </summary>
        /// <param name="state"></param>
        public void ChangeScreenState(bool state)
        {
            gameObject.SetActive(state);
        }

        /// <summary>
        /// Add subscribers to buttons
        /// </summary>
        private void Awake()
        {
            _backButton.onClick.AddListener(SwitchToMain);
            _hostButton.onClick.AddListener(StartHost);
            _joinButton.onClick.AddListener(JoinToServer);
        }

        /// <summary>
        /// Subscribe to network manager events
        /// </summary>
        private void OnEnable()
        {
            _inputField.text = "localhost";
            CustomNetworkManager.ClientConnected += OnClientConnected;
            CustomNetworkManager.ClientDisconnected += OnClientDisconnected;
        }

        /// <summary>
        /// Checks input field if its empty button "Join" will be inactive
        /// </summary>
        private void FixedUpdate()
        {
            if (!_tryingToConnect)
                _joinButton.interactable = !_inputField.text.Equals("");
        }

        /// <summary>
        /// Unsubscribe from custom events and reset input field
        /// </summary>
        private void OnDisable()
        {
            _inputField.text = "";
            CustomNetworkManager.ClientConnected -= OnClientConnected;
            CustomNetworkManager.ClientDisconnected -= OnClientDisconnected;
        }

        /// <summary>
        /// Remove subscribers from buttons
        /// </summary>
        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
            _hostButton.onClick.RemoveAllListeners();
            _joinButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Start host
        /// </summary>
        private void StartHost()
        {
            HostGame.Invoke();
        }

        /// <summary>
        /// Trey to join to the server by IP
        /// Set Join button inactive while trying to connect
        /// </summary>
        private void JoinToServer()
        {
            var networkAddress = _inputField.text;
            JoinGame.Invoke(networkAddress);
            _tryingToConnect = true;
            _joinButton.interactable = false;
        }

        /// <summary>
        /// Switch to main menu screen
        /// </summary>
        private void SwitchToMain()
        {
            SwitchScreen.Invoke(true);
            ChangeScreenState(false);
        }

        /// <summary>
        /// If client successful connected switch of menu
        /// </summary>
        private void OnClientConnected()
        {
            _joinButton.interactable = true;
            ChangeScreenState(false);
        }

        /// <summary>
        /// Reset Join button on disconnected
        /// </summary>
        private void OnClientDisconnected()
        {
            _joinButton.interactable = true;
        }
    }
}