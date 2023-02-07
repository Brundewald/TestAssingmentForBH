using Code.InputManagement;
using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public class CameraHandler: NetworkBehaviour
    {
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField, Range(0, 1)] private float _sensitivity = .5f;
        private Camera _camera;
        private readonly Vector3 _offset = new Vector3(0, 2f, -6);
        private float _mouseHorizontal;
        private float _mouseVertical;
        private bool _cameraSet;
        private float _xRotation;
        private float _yRotation;


        public override void OnStartLocalPlayer()
        {
            if (!isOwned) return;
            _inputHandler.MouseHorizontalAxisChange += GetMouseHorizontal;
            _inputHandler.MouseVerticalAxisChange += GetMouseVertical;
            SetUpCamera();
        }

        [Client]
        private void SetUpCamera()
        {
            _camera = Camera.main;
            _camera.transform.SetParent(transform);
            _camera.transform.localPosition = Vector3.zero + _offset;
            _camera.transform.localRotation = Quaternion.Euler(Vector3.zero);
            _camera.transform.localScale = Vector3.one;
            _cameraSet = true;
        }

        [ClientCallback]
        private void Update()
        {
            if(isServer) RotateCamera();
            else CmdRotateCamera();
        }

        [Client]
        private void CmdRotateCamera()
        {
            RotateCamera();
        }

        private void RotateCamera()
        {
            if (!isOwned) return;
            if (!_cameraSet) return;
            _xRotation += _mouseVertical * _sensitivity;
            _yRotation = _mouseHorizontal * _sensitivity;
            _camera.transform.localRotation = Quaternion.Euler(Mathf.Clamp(-_xRotation, -15, 20), 0f, 0f);
            transform.Rotate(Vector3.up * _yRotation, Space.World);
        }

        private void GetMouseHorizontal(float value)
        {
            _mouseHorizontal = value;
        }

        private void GetMouseVertical(float value)
        {
            _mouseVertical = value;
        }
    }
}