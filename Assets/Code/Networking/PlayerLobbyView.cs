using System;
using Code.Networking;
using Mirror;
using UnityEngine;

public class PlayerLobbyView : NetworkBehaviour
{
    [SerializeField] private PlayerLobbyUIView _uiPrefab;
    private CustomNetworkManager _networkManager;
    private PlayerLobbyUIView _playerLobbyUI;
        
    public string DisplayName { get; private set; }
    public event Action<PlayerLobbyView> PlayerReadyToSpawn = delegate { };
    
    public override void OnStartLocalPlayer()
    {
        CmdCreateUI();
        gameObject.SetActive(true);
        _networkManager.PlayersInLobby.Add(this);
    }


    public override void OnStopClient()
    {
        _networkManager.PlayersInLobby.Remove(this);
    }
    
    private void Awake()
    {
        _networkManager = CustomNetworkManager.Instance;
    }

    private void OnBackToMain()
    {
        _networkManager.Disconnect();
    }

    private void OnPlayerReady(string playerNickname)
    {
        CmdSetPlayerName(playerNickname);
        CmdDestroyUI();
    }
    
    [Command]
    private void CmdCreateUI()
    {
        TargetCreateUI();
    }

    [Command]
    private void CmdDestroyUI()
    {
        TargetDestroyUI();
    }
    
    [Command]
    private void CmdSetPlayerName(string playerNickname)
    {
        DisplayName = playerNickname;
        PlayerReadyToSpawn.Invoke(this);
    }

    [TargetRpc]
    private void TargetCreateUI()
    {
        _playerLobbyUI = Instantiate(_uiPrefab, transform);
        _playerLobbyUI.PlayerReady += OnPlayerReady;
        _playerLobbyUI.BackToMain += OnBackToMain;
    }
    
    [TargetRpc]
    private void TargetDestroyUI()
    {
        _playerLobbyUI.PlayerReady -= OnPlayerReady;
        _playerLobbyUI.BackToMain -= OnBackToMain;
        Destroy(_playerLobbyUI);
    }
}
