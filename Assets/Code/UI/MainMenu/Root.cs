using Code.Networking;
using Code.UI;
using UnityEngine;

namespace Code
{
    public class Root : MonoBehaviour
    {
        [SerializeField] private MainMenuView _mainMenu;
        [SerializeField] private LobbyView _lobby;
        [SerializeField] private CustomNetworkManager _networkManager;

        private void Awake()
        {
            _mainMenu.SwitchScreen += _lobby.ChangeScreenState;
            _lobby.SwitchScreen += _mainMenu.ChangeScreenState;
            _lobby.HostGame += _networkManager.StartHost;
            _lobby.JoinGame += _networkManager.JoinToServer;
        }

        private void OnDestroy()
        {
            _mainMenu.SwitchScreen -= _lobby.ChangeScreenState;
            _lobby.SwitchScreen -= _mainMenu.ChangeScreenState;
            _lobby.HostGame -= _networkManager.StartHost;
            _lobby.JoinGame -= _networkManager.JoinToServer;
        }
    }
}

