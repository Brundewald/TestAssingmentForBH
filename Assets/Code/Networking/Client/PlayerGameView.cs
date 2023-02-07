using System.Collections;
using Code.InputManagement;
using Code.UI.PlayerUI;
using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public class PlayerGameView: NetworkBehaviour
    {
        private const string WaitingForPlayer = "Waiting for player...";
        [SerializeField] private PlayerTable _playerTable;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private MovementHandler _movementHandler;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private float _timeColored;

        private CustomNetworkManager _networkManager;
        public bool colored { get; private set; }
        private CustomNetworkManager NetworkManager
        {
            get
            {
                if (_networkManager is null)
                {
                    return _networkManager = CustomNetworkManager.Instance;
                }
                return _networkManager;
            }
        }

        [SyncVar(hook = nameof(UpdateName))] public string DisplayName;
        [SyncVar(hook = nameof(UpdateScore))] public int Scores;

        public void SetPlayerName(string playerName)
        {
            DisplayName = playerName;
        }

        public override void OnStartClient()
        {
            CustomNetworkManager.PlayersCountChange += UpdateTable;
            NetworkManager.AddPlayerOnMap(this);
        }

        public override void OnStartAuthority()
        {
            _playerTable.gameObject.SetActive(true);
            if(isOwned) CmdUpdateTable();
        }

        public override void OnStopClient()
        {
            NetworkManager.RemovePlayerFromMap(this);
            CustomNetworkManager.PlayersCountChange -= UpdateTable;
        }

        public override void OnStartLocalPlayer()
        {
            _inputHandler.AttackButtonPressed += Attack;
            _movementHandler.HitPlayer += OnPlayerHit;
        }

        [Command]
        private void OnPlayerHit(PlayerGameView collided)
        {
            if(isServer)
            {
                PlayerHit(collided);
            }
            else
            {
                ServerOnPlayerHit(collided);
            }
            
        }

        [Server]
        private void ServerOnPlayerHit(PlayerGameView collided)
        {
            PlayerHit(collided);
        }

        private void PlayerHit(PlayerGameView collided)
        {
            if (!collided.colored)
            {
                collided.colored = true;
                Debug.Log("Command Reached");
                collided.RpcChangeColor();
                AddScore();
            }
        }

        [Command]
        private void CmdUpdateTable()
        {
            UpdateTable();
        }
        
        [ClientRpc]
        private void RpcChangeColor()
        {
            Debug.Log("Change Color called");
            StartCoroutine(ChangeColorRoutine());
        }

        private IEnumerator ChangeColorRoutine()
        {
            _renderer.material.color = Color.red;
            var time = _timeColored;
            while (time > 0)
            {
                time -= Time.deltaTime;
                yield return null;
            }
            colored = false;
            _renderer.material.color = Color.white;
        }

        [Client]
        private void Attack()
        {
            if (isOwned)
            {
                _movementHandler.DoDash();
            }
        }

        private void UpdateName(string oldValue, string newValue) => UpdateTable();
        private void UpdateScore(int oldValue, int newValue) => UpdateTable();

        private void UpdateTable()
        {
            var playersInGame = NetworkManager.PlayersInGame;
            if(playersInGame.Count > 0 )
            {
                if (!isOwned)
                {
                    foreach (var player in playersInGame)
                    {
                        if (player.isOwned)
                        {
                            player.UpdateTable();
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < _playerTable.TableFields.Count; i++)
                    {
                        _playerTable.TableFields[i].UpdateName(i < playersInGame.Count ? playersInGame[i].DisplayName : WaitingForPlayer);
                        _playerTable.TableFields[i].UpdateScores(i < playersInGame.Count ? playersInGame[i].Scores : 0);
                    }
                }
            }
        }

        //[Server]
        public void AddScore()
        {
            Scores++;
        }
    }
}