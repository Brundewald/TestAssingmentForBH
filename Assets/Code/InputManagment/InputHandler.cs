using System;
using Mirror;
using UnityEngine;

namespace Code.InputManagement
{
    public class InputHandler: NetworkBehaviour
    {
        private const string Vertical = "Vertical";
        private const string Horizontal = "Horizontal";
        private const string MouseHorizontal = "Mouse X";
        private const string MouseVertical = "Mouse Y";

        public event Action<float> HorizontalAxisChange = delegate { };
        public event Action<float> VerticalAxisChange = delegate { };
        public event Action<float> MouseHorizontalAxisChange = delegate { };
        public event Action<float> MouseVerticalAxisChange = delegate { };
        public event Action AttackButtonPressed = delegate { };

        public override void OnStartLocalPlayer()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        [ClientCallback]
        private void Update()
        {
            GetHorizontal();
            GetVertical();
            GetMouseHorizontal();
            GetMouseVertical();
            GetAttackButton();
        }

        [ClientCallback]
        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void GetHorizontal()
        {
            HorizontalAxisChange.Invoke(Input.GetAxis(Horizontal));
        }

        private void GetVertical()
        {
            VerticalAxisChange.Invoke(Input.GetAxis(Vertical));
        }

        private void GetMouseHorizontal()
        {
            MouseHorizontalAxisChange.Invoke(Input.GetAxis(MouseHorizontal));
        }

        private void GetMouseVertical()
        {
            MouseVerticalAxisChange.Invoke(Input.GetAxis(MouseVertical));
        }

        private void GetAttackButton()
        {
            if(Input.GetKeyDown(KeyCode.Mouse0))
                AttackButtonPressed.Invoke();
        }
    }
}