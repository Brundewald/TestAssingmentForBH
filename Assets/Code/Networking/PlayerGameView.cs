using Mirror;
using TMPro;
using UnityEngine;

namespace Code.Networking
{
    public class PlayerGameView: NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameField;
        private Camera _camera;
        private Vector3 _offset = new Vector3(0, 2, -5);
        private CustomNetworkManager _networkManager;

        [SyncVar(hook = nameof(UpdateNameField))] private string _displayName;
        
        public string DisplayName => _displayName;

        public override void OnStartClient()
        {
            _networkManager.PlayersInGame.Add(this);
        }

        public override void OnStopClient()
        {
            _networkManager.PlayersInGame.Remove(this);
        }
        
        private void Awake()
        {
            _networkManager = CustomNetworkManager.Instance;
            _camera = Camera.main;
            var cameraTransform = _camera.transform;
            cameraTransform.rotation = Quaternion.identity;
            cameraTransform.position = gameObject.transform.position + _offset;
        }
        
        public void CmdSetDisplayName(PlayerLobbyView playerLobbyView)
        {
            _displayName = playerLobbyView.DisplayName;
            CmdUpdateNameField(_displayName);
        }

        [Command]
        private void CmdUpdateNameField(string displayName)
        {
            UpdateNameField("", displayName);
        }

        
        private void UpdateNameField(string oldValue, string newValue)
        {
            _nameField.text = newValue;
        }
    }
}