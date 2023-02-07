using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Networking
{
    public class CustomSpawnSystem: NetworkBehaviour
    {
        [SerializeField] private List<Transform> _spawnPoints;
        private List<Transform> _roundSpawnPoints;

        public override void OnStartServer() => ResetSpawnPoints();

        [Server]
        public void SpawnPlayer(string name, PlayerGameView player, NetworkConnectionToClient conn)
        {
           var spawnPosition = GetPosition();
           var newPlayer = Instantiate(player, spawnPosition.position, Quaternion.identity);
           newPlayer.transform.Rotate(spawnPosition.eulerAngles);
           newPlayer.SetPlayerName(name);
           NetworkServer.ReplacePlayerForConnection(conn, newPlayer.gameObject);
        }

        private Transform GetPosition()
        {
            var randomIndex = Random.Range(0, _roundSpawnPoints.Count);
            var spawnPosition = _roundSpawnPoints[randomIndex];
            _roundSpawnPoints.Remove(spawnPosition);
            return spawnPosition;
        }

        public void ResetSpawnPoints()
        {
            _roundSpawnPoints = _spawnPoints;
        }
    }
}