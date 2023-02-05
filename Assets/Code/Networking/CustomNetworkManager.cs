using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Code.Networking
{
    public class CustomNetworkManager: NetworkManager
    {
        private const string Spawnable = "SpawnablePrefabs";
        
        [Scene, SerializeField] private string LobbyScene = string.Empty;
        
        [Header("Room"), SerializeField] private PlayerLobbyView _playerLobbyPrefab = null;
        [Header("Game"), SerializeField] private PlayerGameView _playerGamePrefab;
        
        private static CustomNetworkManager _instance;
        public List<PlayerLobbyView> PlayersInLobby { get; } = new List<PlayerLobbyView>();
        public List<PlayerGameView> PlayersInGame { get; } = new List<PlayerGameView>();

        public static CustomNetworkManager Instance => _instance;
        
        public static event Action ClientConnected = delegate {  };
        public static event Action ClientDisconnected = delegate {  };

        public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>(Spawnable).ToList();

        public override void OnStartClient()
        {
            var spawnable = Resources.LoadAll<GameObject>(Spawnable);
            foreach (var prefab in spawnable)
            {
                NetworkClient.RegisterPrefab(prefab);
            }
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            ClientConnected.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            ClientDisconnected.Invoke();
        }

        public override void OnServerConnect(NetworkConnectionToClient connection)
        {
            if (numPlayers >= maxConnections)
            {
                connection.Disconnect();
                return;
            }

            if (!LobbyScene.Contains(SceneManager.GetActiveScene().name))
            {
                connection.Disconnect();
                return;
            }
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient connection)
        {
            if (LobbyScene.Contains(SceneManager.GetActiveScene().name))
            {
                PlayerLobbyView roomPlayerView = Instantiate(_playerLobbyPrefab);
                roomPlayerView.PlayerReadyToSpawn += OnPlayerReadyToSpawn;
                
                NetworkServer.AddPlayerForConnection(connection, roomPlayerView.gameObject);
            }
        }

        public override void OnStopServer()
        {
            PlayersInLobby.Clear();
            PlayersInGame.Clear();
        }

        public void JoinToServer(string IP)
        {
            networkAddress = IP;
            StartClient();
        }

        public void Disconnect()
        {
            if (NetworkServer.active && NetworkClient.active)
            {
                StopHost();
            }
            if(NetworkClient.active)
            {
                StopClient();
            }
        }

        private void Awake()
        {
            if (_instance is null)
            {
                _instance = this;
            }
        }

        private void OnPlayerReadyToSpawn(PlayerLobbyView player)
        {
            player.PlayerReadyToSpawn -= OnPlayerReadyToSpawn;
            var connection = player.connectionToClient;
            NetworkServer.Destroy(connection.identity.gameObject);
            
            PlayerGameView playerGameInstance = Instantiate(_playerGamePrefab);
            NetworkServer.ReplacePlayerForConnection(connection, playerGameInstance.gameObject);
            playerGameInstance.CmdSetDisplayName(player);
        }
    }
}