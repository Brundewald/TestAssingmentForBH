using Code.InputManagement;
using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public sealed class CameraHandler: NetworkBehaviour
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

        /// <summary>
        /// Subscribe to local input handler events and setup local camera
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            _inputHandler.MouseHorizontalAxisChange += GetMouseHorizontal;
            _inputHandler.MouseVerticalAxisChange += GetMouseVertical;
            SetupCamera();
        }

        /// <summary>
        /// Set local parent, position, rotation and scale
        /// </summary>
        private void SetupCamera()
        {
            _camera = Camera.main;
            _camera.transform.SetParent(transform);
            _camera.transform.localPosition = Vector3.zero + _offset;
            _camera.transform.localRotation = Quaternion.Euler(Vector3.zero);
            _camera.transform.localScale = Vector3.one;
            _cameraSet = true;
        }

        /// <summary>
        /// Use client callback to rotate camera
        /// </summary>
        [ClientCallback]
        private void Update()
        {
            RotateCamera();
        }

        /// <summary>
        /// Rotate local camera by xAxis and player by yAxis
        /// </summary>
        private void RotateCamera()
        {
            if (!_cameraSet) return;
            _xRotation += _mouseVertical * _sensitivity;
            _yRotation = _mouseHorizontal * _sensitivity;
            _camera.transform.localRotation = Quaternion.Euler(Mathf.Clamp(-_xRotation, -15, 20), 0f, 0f);
            transform.Rotate(Vector3.up * _yRotation, Space.World);
        }

        /// <summary>
        /// Get mouse horizontal axis value
        /// </summary>
        /// <param name="value"></param>
        private void GetMouseHorizontal(float value)
        {
            _mouseHorizontal = value;
        }
        
        /// <summary>
        /// Get mouse vertical axis value
        /// </summary>
        /// <param name="value"></param>
        private void GetMouseVertical(float value)
        {
            _mouseVertical = value;
        }
    }
}