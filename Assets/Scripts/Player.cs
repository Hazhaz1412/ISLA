using System;
using UnityEngine;
using MadKnight.ScriptableObjects;
using MadKnight.Enums;

namespace MadKnight
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO _playerStats;

        [SerializeField] private Collider2D _headCollider;
        [SerializeField] private SpriteRenderer _srNormal;
        [SerializeField] private SpriteRenderer _srCrouch;
        [SerializeField] private Transform _groundCheck;

        private Rigidbody2D _rb;

        private PlayerState _state;

        private float _horizontalAxis;
        private bool _isOnFloor;
        private int _jumpRemaining;
        private bool _hasJumped;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            _state = PlayerState.Idle;
            _jumpRemaining = _playerStats.MaxJumpCount;
        }

        private void Update()
        {
            HandleStateTransition();
            HandleAnimation();
        }

        private void HandleStateTransition()
        {
            _horizontalAxis = Input.GetAxis("Horizontal");

            switch (_state)
            {
                case PlayerState.Idle:
                    if (_horizontalAxis != 0)
                    {
                        _state = PlayerState.Walking;
                    }
                    else if (CrouchCondition())
                    {
                        _state = PlayerState.Crouching;
                    }
                    else if (JumpCondition())
                    {
                        _state = PlayerState.Jumping;
                    }
                    else if (!_isOnFloor)
                    {
                        _state = PlayerState.Airborne;
                    }

                    break;
                case PlayerState.Walking:
                    if (_horizontalAxis == 0)
                    {
                        _state = PlayerState.Idle;
                    }
                    else if (CrouchCondition())
                    {
                        _state = PlayerState.Crouching;
                    }
                    else if (JumpCondition())
                    {
                        _state = PlayerState.Jumping;
                    }
                    else if (!_isOnFloor)
                    {
                        _state = PlayerState.Airborne;
                    }

                    break;
                case PlayerState.Jumping:
                    if (!_isOnFloor && _hasJumped)
                    {
                        _state = PlayerState.Airborne;
                        _hasJumped = false;
                    }

                    break;
                case PlayerState.Airborne:
                    if (JumpCondition())
                    {
                        _state = PlayerState.Jumping;
                    }
                    else if (_isOnFloor)
                    {
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (_rb.linearVelocityX == 0)
                        {
                            _state = PlayerState.Idle;
                        }
                        else
                        {
                            _state = PlayerState.Walking;
                        }

                        _jumpRemaining = _playerStats.MaxJumpCount;
                    }

                    break;
                case PlayerState.Crouching:
                    if (!CrouchCondition())
                    {
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (_rb.linearVelocityX == 0)
                        {
                            _state = PlayerState.Idle;
                        }
                        else
                        {
                            _state = PlayerState.Walking;
                        }
                    }
                    else if (JumpCondition())
                    {
                        _state = PlayerState.Jumping;
                    }

                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (_state == PlayerState.Crouching)
                    {
                        _headCollider.enabled = false;
                    }
                    else
                    {
                        _headCollider.enabled = true;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleAnimation()
        {
            switch (_state)
            {
                case PlayerState.Crouching:
                    _srNormal.gameObject.SetActive(false);
                    _srCrouch.gameObject.SetActive(true);
                    break;
                case PlayerState.Idle:
                case PlayerState.Walking:
                case PlayerState.Jumping:
                case PlayerState.Airborne:
                    _srNormal.gameObject.SetActive(true);
                    _srCrouch.gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {
            _isOnFloor = Physics2D.Raycast(
                _groundCheck.position,
                Vector2.up,
                Mathf.Abs(_groundCheck.position.y - transform.position.y),
                LayerMask.GetMask(nameof(LayerMaskEnum.Ground)));
            var moveXVelocity = _playerStats.Speed * _horizontalAxis;

            switch (_state)
            {
                case PlayerState.Jumping:
                    if (!_hasJumped)
                    {
                        _rb.linearVelocity = new Vector2(_rb.linearVelocityX, 0);
                        _rb.AddForce(Vector2.up * _playerStats.JumpForce, ForceMode2D.Impulse);
                        _jumpRemaining--;
                        _hasJumped = true;
                    }

                    break;
                case PlayerState.Airborne:
                case PlayerState.Walking:
                    _rb.linearVelocity = new Vector2(moveXVelocity, _rb.linearVelocityY);
                    break;
                case PlayerState.Crouching:
                    _rb.linearVelocity = new Vector2(
                        moveXVelocity * _playerStats.CrouchSpeedMultiplier, _rb.linearVelocityY);
                    break;
                case PlayerState.Idle:
                default:
                    break;
            }
        }

        private bool JumpCondition()
        {
            return _jumpRemaining > 0 && Input.GetButtonDown("Jump");
        }

        private bool CrouchCondition()
        {
            return _isOnFloor && Input.GetButton("Fire1");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Kiểm tra xem có va chạm với Wall layer không
            if (collision.gameObject.layer == LayerMask.NameToLayer(nameof(LayerMaskEnum.Wall)))
            {
                // Kiểm tra xem va chạm có phải từ bên cạnh không (không phải từ trên xuống)
                Vector2 contactDirection = collision.GetContact(0).normal;
                
                // Nếu va chạm từ bên cạnh (normal vector có thành phần x lớn)
                if (Mathf.Abs(contactDirection.x) > Mathf.Abs(contactDirection.y))
                {
                    // Nếu đang ở trạng thái airborne và có jump remaining, giảm 1 lần jump
                    if (_state == PlayerState.Airborne && _jumpRemaining > 0)
                    {
                        _jumpRemaining++;
                    }
                }
            }
        }

        private enum PlayerState
        {
            Idle,
            Walking,
            Crouching,
            Jumping,
            Airborne,
        }
    }
}