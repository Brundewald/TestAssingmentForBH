using System;
using Code.Networking;
using Code.UI.PlayerUI;
using Mirror;
using UnityEngine;

public sealed class PlayerLobbyView : NetworkBehaviour
{
    [SerializeField] private PlayerLobbyUIView _uiPrefab;
    private CustomNetworkManager _networkManager;
    private PlayerLobbyUIView _playerLobbyUI;
        
    public string DisplayName { get; private set; }
    public event Action<PlayerLobbyView> PlayerReadyToSpawn = delegate { };

    /// <summary>
    /// Create local lobby UI and add player in lobby list
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        CmdCreateUI();
        gameObject.SetActive(true);
        _networkManager.PlayersInLobby.Add(this);
    }

    /// <summary>
    /// Remove player from lobby list
    /// </summary>
    public override void OnStopClient()
    {
        _networkManager.PlayersInLobby.Remove(this);
    }

    /// <summary>
    /// Disconnect player if he pressed back button
    /// </summary>
    public void OnBackToMain()
    {
        _networkManager.Disconnect();
    }

    /// <summary>
    /// Send command to server on player Ready
    /// </summary>
    /// <param name="playerNickname"></param>
    public void OnPlayerReady(string playerNickname)
    {
        CmdSetPlayerName(playerNickname);
    }

    /// <summary>
    /// Get custom network manager instance
    /// </summary>
    private void Awake()
    {
        _networkManager = CustomNetworkManager.Instance;
    }

    /// <summary>
    /// Send command to create local player UI
    /// </summary>
    [Command]
    private void CmdCreateUI()
    {
        TargetCreateUI();
    }

    /// <summary>
    /// Set command to server to set display name and GameView
    /// </summary>
    /// <param name="playerNickname"></param>
    [Command]
    private void CmdSetPlayerName(string playerNickname)
    {
        DisplayName = playerNickname.ToUpper();
        PlayerReadyToSpawn.Invoke(this);
    }

    /// <summary>
    /// Instantiate local player UI and set it up
    /// </summary>
    [TargetRpc]
    private void TargetCreateUI()
    {
        _playerLobbyUI = Instantiate(_uiPrefab, transform);
        _playerLobbyUI.SetupLobbyObject(this);
    }
}
