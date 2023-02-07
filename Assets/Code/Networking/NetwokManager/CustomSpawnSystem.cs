using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public sealed class CustomSpawnSystem: NetworkBehaviour
    {
        [SerializeField] private List<Transform> _spawnPoints;
        private List<Transform> _roundSpawnPoints;

        
        public override void OnStartServer()
        {
            _roundSpawnPoints = new List<Transform>();
            ResetSpawnPoints();
        }

        /// <summary>
        /// Spawn interactable player object, set name and replace LobbyView
        /// </summary>
        /// <param name="name">playerName</param>
        /// <param name="player">playerGameView</param>
        /// <param name="conn">connectionToClient</param>
        [Server]
        public void SpawnPlayer(string name, PlayerGameView player, NetworkConnectionToClient conn)
        {
            var newPlayer = Instantiate(player);
            newPlayer.SetPlayerName(name);
            SetPosition(newPlayer);
            NetworkServer.ReplacePlayerForConnection(conn, newPlayer.gameObject);
        }

        /// <summary>
        /// Set initial player position and rotation
        /// </summary>
        /// <param name="newPlayer"></param>
        private void SetPosition(PlayerGameView newPlayer)
        {
            var spawnPosition = GetRandomPosition();
            newPlayer.transform.position = spawnPosition.position;
            newPlayer.transform.Rotate(spawnPosition.eulerAngles);
        }

        /// <summary>
        /// Get random position to spawn player and removes this position from list
        /// </summary>
        /// <returns>spawnPosition</returns>
        public Transform GetRandomPosition()
        {
            var randomIndex = Random.Range(0, _roundSpawnPoints.Count);
            var spawnPosition = _roundSpawnPoints[randomIndex];
            Debug.Log($"{randomIndex} {_roundSpawnPoints.Count}");
            _roundSpawnPoints.Remove(spawnPosition);
            return spawnPosition;
        }

        /// <summary>
        /// Refresh spawn positions list for new respawn
        /// </summary>
        public void ResetSpawnPoints()
        {
            _roundSpawnPoints.Clear();
            foreach (var spawnPoint in _spawnPoints)
            {
                var newPoint = spawnPoint;
                _roundSpawnPoints.Add(newPoint);
            }
        }
    }
}