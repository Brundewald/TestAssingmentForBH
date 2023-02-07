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
        #region Properties
        private const string Spawnable = "SpawnablePrefabs";
        
        [Scene, SerializeField] private string LobbyScene = string.Empty;
        [Header("Room"), SerializeField] private PlayerLobbyView _playerLobbyPrefab = null;
        [Header("Game"), SerializeField] private PlayerGameView _playerGamePrefab;
        [SerializeField] private CustomSpawnSystem _customSpawnSystemPrefab;
        
        private static CustomNetworkManager _instance;
        private CustomSpawnSystem _spawnSystem;

        #endregion
        
        #region PublicProperties
        public List<PlayerLobbyView> PlayersInLobby { get; } = new List<PlayerLobbyView>();
        public List<PlayerGameView> PlayersInGame { get; } = new List<PlayerGameView>();
        public static CustomNetworkManager Instance => _instance;
        #endregion        
        
        #region Events
        public static event Action ClientConnected = delegate {  };
        public static event Action ClientDisconnected = delegate {  };
        public static event Action PlayersCountChange = delegate {  };
        #endregion
        
        public override void OnStartServer()
        {
            spawnPrefabs = Resources.LoadAll<GameObject>(Spawnable).ToList();
            _spawnSystem = Instantiate(_customSpawnSystemPrefab);
            NetworkServer.Spawn(_spawnSystem.gameObject);
        }

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

        public override void OnServerReady(NetworkConnectionToClient connection)
        {
            base.OnServerReady(connection);
        }

        public override void Awake()
        {
            _instance ??= this;
            base.Awake();
        }

        public void AddPlayerOnMap(PlayerGameView player)
        {
            PlayersInGame.Add(player);
            PlayersCountChange.Invoke();
        }

        public void RemovePlayerFromMap(PlayerGameView player)
        {
            PlayersInGame.Remove(player);
            PlayersCountChange.Invoke();
        }

        public void PlayerHit(uint attackerId, uint targetId)
        {
            foreach (var player in PlayersInGame)
            {
                if (player.netId.Equals(attackerId))
                {
                    player.AddScore();
                }

                if (player.netId.Equals(targetId))
                {
                    //player.Hit(targetId);
                }
            }
        }

        private void OnPlayerReadyToSpawn(PlayerLobbyView player)
        {
            player.PlayerReadyToSpawn -= OnPlayerReadyToSpawn;
            var connection = player.connectionToClient;
            NetworkServer.Destroy(connection.identity.gameObject);
            _spawnSystem.SpawnPlayer(player.DisplayName, _playerGamePrefab, connection);
        }
    }
}