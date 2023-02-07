using Code.Networking;
using Code.UI;
using UnityEngine;

namespace Code
{
    public class Root : MonoBehaviour
    {
        [SerializeField] private MainMenuView _mainMenu;
        [SerializeField] private FindLobbyView findLobby;
        [SerializeField] private CustomNetworkManager _networkManager;

        private void Awake()
        {
            _mainMenu.SwitchScreen += findLobby.ChangeScreenState;
            findLobby.SwitchScreen += _mainMenu.ChangeScreenState;
            findLobby.HostGame += _networkManager.StartHost;
            findLobby.JoinGame += _networkManager.JoinToServer;
        }

        private void OnDestroy()
        {
            _mainMenu.SwitchScreen -= findLobby.ChangeScreenState;
            findLobby.SwitchScreen -= _mainMenu.ChangeScreenState;
            findLobby.HostGame -= _networkManager.StartHost;
            findLobby.JoinGame -= _networkManager.JoinToServer;
        }
    }
}

