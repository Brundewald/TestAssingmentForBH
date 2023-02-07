using System;
using System.Collections;
using Code.InputManagement;
using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public sealed class MovementHandler: NetworkBehaviour
    {
        private const string IgnoreByPlayer = "IgnoreByPlayer";
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField, Tooltip("Set dash force/speed")] private float _dashForce;
        [SerializeField, Tooltip("Set dash duration, after that time players velocity becomes to zero")] private float _dashTime;   
        [SerializeField, Tooltip("Player standard movement speed")] private float _velocity;
        private float _horizontal;
        private float _vertical;
        public bool isDashing { get; private set; }

        public event Action<PlayerGameView> HitPlayer = delegate { };

        /// <summary>
        /// Subscribe to local input handler events
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            _inputHandler.HorizontalAxisChange += GetHorizontal;
            _inputHandler.VerticalAxisChange += GetVertical;
        }
        
        /// <summary>
        /// Unsubscribe to local input handler events
        /// </summary>
        public override void OnStopLocalPlayer()
        {
            _inputHandler.HorizontalAxisChange -= GetHorizontal;
            _inputHandler.VerticalAxisChange -= GetVertical;
        }

        /// <summary>
        /// Use client callback of fixed update to update physical move
        /// </summary>
        [ClientCallback]
        private void FixedUpdate()
        {
            if(isOwned&&!isDashing)
            {
                PhysicalMove();
            }
        }
        
        /// <summary>
        /// If vertical velocity is approximately to 0 player can move by WASD
        /// </summary>
        private void PhysicalMove()
        {
            if(Mathf.Approximately(_rigidbody.velocity.y, 0))
            {
                var forwardVector = transform.right * _horizontal + transform.forward * _vertical;
                _rigidbody.velocity = forwardVector * _velocity;
            }
        }

        /// <summary>
        /// If player not dashing start dash coroutine
        /// </summary>
        public void DoDash()
        {
            if(!isDashing) StartCoroutine(Dash());
        }

        /// <summary>
        /// Use client callback to detect collision with other player when dashing;
        /// </summary>
        /// <param name="collision"></param>
        [ClientCallback]
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.tag.Equals(IgnoreByPlayer) && isDashing && isOwned)
            {
                if (collision.gameObject.TryGetComponent(out PlayerGameView collided))
                {
                    HitPlayer.Invoke(collided);
                }
            }
        }

        /// <summary>
        /// Dash coroutine when time elapsed set player velocity to zero
        /// </summary>
        /// <returns></returns>
        private IEnumerator Dash()
        {
            isDashing = true;
            
            var dashTime = _dashTime;
            var dashDirection = transform.forward * _dashForce;
            _rigidbody.AddForce(dashDirection, ForceMode.Impulse);

            while (dashTime > 0)
            {
                dashTime -= Time.deltaTime;
                yield return null;
            }
            
            _rigidbody.velocity = Vector3.zero;
            isDashing = false;
        }

        /// <summary>
        /// Get horizontal axis value
        /// </summary>
        /// <param name="value"></param>
        private void GetHorizontal(float value)
        {
            _horizontal = value;
        }

        /// <summary>
        /// Get vertical axis value
        /// </summary>
        /// <param name="value"></param>
        private void GetVertical(float value)
        {
            _vertical = value;
        }
    }
}