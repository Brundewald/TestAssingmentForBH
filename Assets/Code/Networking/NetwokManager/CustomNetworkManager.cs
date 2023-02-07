using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Code.Networking
{
    public sealed class CustomNetworkManager : NetworkManager
    {
        #region Properties

        private const string Spawnable = "SpawnablePrefabs";

        [Scene, SerializeField] private string LobbyScene = string.Empty;
        [Header("Room"), SerializeField] private PlayerLobbyView _playerLobbyPrefab = null;

        [Space] [Header("Game"), SerializeField] private PlayerGameView _playerGamePrefab;
        [SerializeField] private CustomSpawnSystem _customSpawnSystemPrefab;
        [SerializeField, Tooltip("How long player will stay colored after hit")] private int _timeToRestartGame;
        private static CustomNetworkManager _instance;
        private CustomSpawnSystem _spawnSystem;
        private bool _alreadyCalled;

        #endregion

        #region PublicProperties

        public List<PlayerLobbyView> PlayersInLobby { get; } = new List<PlayerLobbyView>();
        public List<PlayerGameView> ActivePlayers { get; } = new List<PlayerGameView>();
        public static CustomNetworkManager Instance => _instance;

        #endregion

        #region Events

        public static event Action ClientConnected = delegate { };
        public static event Action ClientDisconnected = delegate { };
        public static event Action PlayersCountChange = delegate { };

        #endregion

        #region NetworkManagerFunctions

        /// <summary>
        /// Register all spawnable prefabs on server and create spawn system
        /// </summary>
        public override void OnStartServer()
        {
            spawnPrefabs = Resources.LoadAll<GameObject>(Spawnable).ToList();
            _spawnSystem = Instantiate(_customSpawnSystemPrefab);
            NetworkServer.Spawn(_spawnSystem.gameObject);
        }

        /// <summary>
        /// Register all spawnable prefabs on client
        /// </summary>
        public override void OnStartClient()
        {
            var spawnable = Resources.LoadAll<GameObject>(Spawnable);
            foreach (var prefab in spawnable)
            {
                NetworkClient.RegisterPrefab(prefab);
            }
        }

        /// <summary>
        /// Rise static event on player connection
        /// </summary>
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            ClientConnected.Invoke();
        }

        /// <summary>
        /// Rise static event on player disconnection
        /// </summary>
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            ClientDisconnected.Invoke();
        }

        /// <summary>
        /// Prevent new player connection if server is full;
        /// </summary>
        /// <param name="connection"></param>
        public override void OnServerConnect(NetworkConnectionToClient connection)
        {
            if (numPlayers >= maxConnections)
            {
                connection.Disconnect();
                return;
            }
        }

        /// <summary>
        /// Create lobby view and add it to conn
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if (LobbyScene.Contains(SceneManager.GetActiveScene().name))
            {
                PlayerLobbyView roomPlayerView = Instantiate(_playerLobbyPrefab);
                roomPlayerView.PlayerReadyToSpawn += OnPlayerReadyToSpawn;

                NetworkServer.AddPlayerForConnection(conn, roomPlayerView.gameObject);
            }
        }

        /// <summary>
        /// Clear list of players in lobby and active players
        /// </summary>
        public override void OnStopServer()
        {
            PlayersInLobby.Clear();
            ActivePlayers.Clear();
        }

        /// <summary>
        /// Create singleton of the custom manager 
        /// </summary>
        public override void Awake()
        {
            _instance ??= this;
            base.Awake();
        }
        #endregion

        #region CustomMethods
        /// <summary>
        /// Add client to server
        /// </summary>
        /// <param name="IP"></param>
        public void JoinToServer(string IP)
        {
            networkAddress = IP;
            StartClient();
        }

        /// <summary>
        /// Stop client or server
        /// </summary>
        public void Disconnect()
        {
            if (NetworkServer.active && NetworkClient.active)
            {
                StopHost();
            }

            if (NetworkClient.active)
            {
                StopClient();
            }
        }

        /// <summary>
        /// Reset player score and set new rando 
        /// </summary>
        public void RestartGame()
        {
            _spawnSystem.ResetSpawnPoints();
            foreach (var player in ActivePlayers)
            {
                player.Score = 0;
                var spawnPoint = _spawnSystem.GetRandomPosition();
                var message = new MessageTransform();
                var netID = player.netId;
                message.SetData(spawnPoint, netID);
                NetworkServer.SendToReady(message);
            }
        }
        
        /// <summary>
        /// Add player instance in active players list
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayerOnMap(PlayerGameView player)
        {
            ActivePlayers.Add(player);
            PlayersCountChange.Invoke();
        }

        /// <summary>
        /// Remove player from active players list
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayerFromMap(PlayerGameView player)
        {
            ActivePlayers.Remove(player);
            PlayersCountChange.Invoke();
        }

        /// <summary>
        /// Replace LobbyView for GameView and spawn interactable player instance
        /// </summary>
        /// <param name="player"></param>
        private void OnPlayerReadyToSpawn(PlayerLobbyView player)
        {
            player.PlayerReadyToSpawn -= OnPlayerReadyToSpawn;
            var conn = player.connectionToClient;
            NetworkServer.Destroy(conn.identity.gameObject);
            _spawnSystem.SpawnPlayer(player.DisplayName, _playerGamePrefab, conn);
        }

        /// <summary>
        /// Check all local players and get winners name
        /// </summary>
        public void FindWinner()
        {
            var name = "";

            foreach (var player in ActivePlayers)
            {
                if (player.Score == 3)
                {
                    name = player.DisplayName;
                    break;
                }
            }

            var winMessage = new WinMessage() {Name = name, TimeToReset = _timeToRestartGame};
            NetworkServer.SendToReady(winMessage);
        }
        #endregion
    }
}