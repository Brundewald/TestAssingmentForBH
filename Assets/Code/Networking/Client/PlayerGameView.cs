using System.Collections;
using Code.InputManagement;
using Code.UI.PlayerUI;
using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public sealed class PlayerGameView: NetworkBehaviour
    {
        #region Properties
        private const string WaitingForPlayer = "Waiting for player...";
        [SerializeField] private PlayerTable _playerTable;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private MovementHandler _movementHandler;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private GameOverScreenView _gameOverScreen;
        [SerializeField] private float _timeColored = 3;

        private CustomNetworkManager _networkManager;

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

        private bool colored { get; set; }

        [SyncVar(hook = nameof(UpdateName))] public string DisplayName;
        [SyncVar(hook = nameof(UpdateScore))] public int Score;

        #endregion

        #region OverridedFunctions
        /// <summary>
        /// Set player display name
        /// </summary>
        /// <param name="playerName">playerDisplayName</param>
        public void SetPlayerName(string playerName)
        {
            DisplayName = playerName;
        }
        
        /// <summary>
        /// Subscribe player to PlayerCountChange event to update players table and add player to "active" list 
        /// </summary>
        public override void OnStartClient()
        {
            CustomNetworkManager.PlayersCountChange += UpdateTable;
            NetworkManager.AddPlayerOnMap(this);
        }

        /// <summary>
        /// Activate local players table GO and force update it
        /// </summary>
        public override void OnStartAuthority()
        {
            _playerTable.gameObject.SetActive(true);
            if(isOwned) CmdUpdateTable();
        }

        /// <summary>
        /// Unsubscribe from PlayerCountChange event and remove player from active list
        /// </summary>
        public override void OnStopClient()
        {
            CustomNetworkManager.PlayersCountChange -= UpdateTable;
            NetworkManager.RemovePlayerFromMap(this);
        }
        
        /// <summary>
        /// Register message handler for position update after respawn, subscribe to handlers event and
        /// subscribe server on TimeExpired event to reset local game
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            NetworkClient.RegisterHandler<MessageTransform>(SetPlayerPosition);
            _inputHandler.AttackButtonPressed += OnAttack;
            _movementHandler.HitPlayer += OnPlayerHit;
            if(isServer)_gameOverScreen.TimerExpired += RestartGame;
        }

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        public override void OnStopLocalPlayer()
        {
            _inputHandler.AttackButtonPressed -= OnAttack;
            _movementHandler.HitPlayer -= OnPlayerHit;
            if(isServer)_gameOverScreen.TimerExpired -= RestartGame;
        }

        #endregion

        #region Custom methods
/// <summary>
        /// Send command to server on successful attack on another player
        /// </summary>
        /// <param name="collided"></param>
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

        /// <summary>
        /// If attacker is a client it ask server to perform PlayerHit method
        /// </summary>
        /// <param name="collided">attacked player</param>
        [Server]
        private void ServerOnPlayerHit(PlayerGameView collided)
        {
            PlayerHit(collided);
        }

        /// <summary>
        /// If one player successful attack another the
        /// pray becomes red for specified amount of time and attacker receives score.
        /// If pray already colored so no ones get score before it gets to normal 
        /// </summary>
        /// <param name="collided"></param>
        private void PlayerHit(PlayerGameView collided)
        {
            if (!collided.colored)
            {
                collided.colored = true;
                collided.RpcChangeColor();
                AddScore();
            }
        }

        /// <summary>
        /// Command to force update table
        /// </summary>
        [Command]
        private void CmdUpdateTable()
        {
            UpdateTable();
        }

        /// <summary>
        /// Start coroutine to change color of attacked player on all clients
        /// </summary>
        [ClientRpc]
        private void RpcChangeColor()
        {
            StartCoroutine(ChangeColorRoutine());
        }

        /// <summary>
        /// Change player color to red for specified amount of time
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Player complete forward dash if left mouse button clicked
        /// </summary>
        [Client]
        private void OnAttack()
        {
            if (isOwned)
            {
                _movementHandler.DoDash();
            }
        }

        /// <summary>
        /// Update name by SyncVar
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private void UpdateName(string oldValue, string newValue) => UpdateTable();

        /// <summary>
        /// Update score by SyncVar
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private void UpdateScore(int oldValue, int newValue)
        {
            UpdateTable();
            CheckScore();
        }

        /// <summary>
        /// Checks is score enough to win the game
        /// </summary>
        private void CheckScore()
        {
            if (Score == 3)
            {
                if(isOwned)CmdEndGame();
            }
        }

        /// <summary>
        /// Starts end match sequence and send command to server to find the winner
        /// </summary>
        [Command]
        private void CmdEndGame()
        {
            NetworkManager.FindWinner();
        }

        /// <summary>
        /// Update table on client side, if called from other client reroute to owner
        /// </summary>
        [Client]
        private void UpdateTable()
        {
            var playersInGame = NetworkManager.ActivePlayers;
            if(playersInGame.Count > 0)
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
                        _playerTable.TableFields[i].UpdateScores(i < playersInGame.Count ? playersInGame[i].Score : 0);
                    }
                }
            }
        }

        /// <summary>
        /// Add score
        /// </summary>
        private void AddScore()
        {
            if (Score < 3)
            {
                Score++;
            }
        }

        /// <summary>
        /// Server only. Reset scores and reset players positions
        /// </summary>
        private void RestartGame()
        {
            _networkManager.RestartGame();
        }

        /// <summary>
        /// Set player position after respawn
        /// </summary>
        /// <param name="mTransform">Message Transform</param>
        private void SetPlayerPosition(MessageTransform mTransform)
        {
            if(!netId.Equals(mTransform.NetID)) return;
            transform.position = mTransform.Position;
            transform.Rotate(mTransform.EulerAngles);
        }
        
        #endregion
    }  
}