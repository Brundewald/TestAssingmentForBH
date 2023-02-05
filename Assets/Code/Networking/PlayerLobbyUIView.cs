using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyUIView : MonoBehaviour
{
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private Button _spawnButton;
    
    private int _minNicknameLength = 3;
    
    public event Action BackToMain = delegate {  };
    public event Action<string> PlayerReady = delegate {  }; 

    private void Awake()
    {
        _backButton.onClick.AddListener(BackButtonPressed);
        _spawnButton.onClick.AddListener(SpawnPlayer);
    }
    
    private void FixedUpdate()
    {
        _spawnButton.interactable = !_nicknameInput.text.Equals("") && _nicknameInput.text.Length >= _minNicknameLength;
    }

    private void OnDestroy()
    {
        _backButton.onClick.RemoveAllListeners();
    }

    private void BackButtonPressed()
    {
        BackToMain.Invoke();
    }

    private void SpawnPlayer()
    {
        var playerNickname = _nicknameInput.text;
        PlayerReady.Invoke(playerNickname);
        gameObject.SetActive(false);
    }
}