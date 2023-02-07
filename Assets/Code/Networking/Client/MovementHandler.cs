using System;
using System.Collections;
using Code.InputManagement;
using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public class MovementHandler: NetworkBehaviour
    {
        private const string IgnoreByPlayer = "IgnoreByPlayer";
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _dashForce;
        [SerializeField] private float _dashTime;
        [SerializeField] private bool _physicalMov = false;
        [SerializeField] private float _velocity;
        [SyncVar] private float _moveSpeed = 2;
        private float _horizontal;
        private float _vertical;
        public bool isDashing { get; private set; }

        public event Action<PlayerGameView> HitPlayer = delegate { }; 

        [ClientCallback]
        private void Start()
        {
            if(isOwned)
            {
                _inputHandler.HorizontalAxisChange += GetHorizontal;
                _inputHandler.VerticalAxisChange += GetVertical;
            }
        }

        [ClientCallback]
        private void Update()
        {
            
            if(isOwned&&!isDashing&&!_physicalMov)
            {
                Move();
            }
        }

        [ClientCallback]
        private void FixedUpdate()
        {
            if(isOwned&&!isDashing&&_physicalMov)
            {
                PhysicalMove();
            }
        }

        [ClientCallback]
        private void OnDestroy()
        {
            if(isOwned)
            {
                _inputHandler.HorizontalAxisChange -= GetHorizontal;
                _inputHandler.VerticalAxisChange -= GetVertical;
            }
        }

        private void GetHorizontal(float value)
        {
            _horizontal = value;
        }

        private void GetVertical(float value)
        {
            _vertical = value;
        }

        [Client]
        private void Move()
        {
            var forwardVector = transform.right * _horizontal + transform.forward * _vertical;
            transform.position += forwardVector * _moveSpeed * Time.fixedDeltaTime;
        }

        [Client]
        private void PhysicalMove()
        {
            if(Mathf.Approximately(_rigidbody.velocity.y, 0))
            {
                var forwardVector = transform.right * _horizontal + transform.forward * _vertical;
                _rigidbody.velocity = forwardVector * _velocity;
            }
        }

        [Client]
        public void DoDash()
        {
            if(!isDashing) StartCoroutine(Dash());
        }

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

        [ClientCallback]
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.tag.Equals(IgnoreByPlayer) && isDashing && isOwned)
            {
                if (collision.gameObject.TryGetComponent(out PlayerGameView collided))
                {
                    Debug.Log("HitPlayer");
                    HitPlayer.Invoke(collided);
                }
            }
        }
    }
}