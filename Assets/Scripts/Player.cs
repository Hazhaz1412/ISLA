using System;
using MadKnight.Enums;
using MadKnight.ScriptableObjects;
using UnityEngine;

namespace MadKnight
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public class Player : MonoBehaviour
    {
        private enum PlayerState
        {
            Idle,
            Walking,
            Crouching,
            Jumping,
            Airborne,
            WallSlide,
            WallBounce,
        }

        [SerializeField] private PlayerStatsSO _stats;

        [SerializeField] private Collider2D _headCollider;
        [SerializeField] private SpriteRenderer _srNormal;
        [SerializeField] private SpriteRenderer _srCrouch;
        [SerializeField] private Transform _ceilCheck;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _wallCheck;

        private Rigidbody2D _rb;
        private Animator _anim;

        private PlayerState _state;
        private PlayerState _prevState;

        private float _horizontalAxis;
        private int _jumpRemaining;
        private bool _hasJumped;
        private bool _hasWallBounced;
        private bool _isOnCeil;
        private bool _isOnFloor;
        private bool _isOnWall;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();

            _state = PlayerState.Idle;
            _jumpRemaining = _stats.MaxJumpCount;
        }

        private void Update()
        {
            _horizontalAxis = Input.GetAxis("Horizontal");

            _anim.SetFloat(
                    nameof(PlayerAnimationEnum.FHorizontalVelocity),
                    Mathf.Abs(_rb.linearVelocityX)
            );
            _anim.SetFloat(
                    nameof(PlayerAnimationEnum.FVerticalVelocity),
                    _rb.linearVelocityY
            );
            _anim.SetBool(
                    nameof(PlayerAnimationEnum.BIsOnFloor),
                    _isOnFloor
            );

            if (Camera.main)
            {
                Camera.main.transform.position = new Vector3(
                        transform.position.x,
                        transform.position.y,
                        Camera.main.transform.position.z
                );
            }

            HandleStateTransition();
            HandleAnimation();
        }

        private void HandleStateTransition()
        {
            var initState = _state;

            bool isJumpClicked, isWallBounceClicked, isCrouchClicked;
            isJumpClicked = isWallBounceClicked = Input.GetButtonDown("Jump");
            isCrouchClicked = Input.GetButton("Fire1");

            var crouchable = _isOnFloor && isCrouchClicked;
            var jumpAble = _jumpRemaining > 0 && isJumpClicked;

            switch (_state)
            {
                case PlayerState.Idle:
                    {
                        if (_horizontalAxis != 0)
                        {
                            _state = PlayerState.Walking;
                        }
                        else if (crouchable)
                        {
                            _state = PlayerState.Crouching;
                        }
                        else if (jumpAble)
                        {
                            _state = PlayerState.Jumping;
                        }
                        else if (!_isOnFloor)
                        {
                            _state = PlayerState.Airborne;
                        }
                    }
                    break;
                case PlayerState.Walking:
                    {
                        if (_rb.linearVelocityX == 0)
                        {
                            _state = PlayerState.Idle;
                        }
                        else if (crouchable)
                        {
                            _state = PlayerState.Crouching;
                        }
                        else if (jumpAble)
                        {
                            _state = PlayerState.Jumping;
                        }
                        else if (!_isOnFloor)
                        {
                            _state = PlayerState.Airborne;
                        }
                    }
                    break;
                case PlayerState.Jumping:
                    {
                        if (!_isOnFloor && _hasJumped)
                        {
                            _state = PlayerState.Airborne;
                            _hasJumped = false;
                            _anim.SetTrigger(nameof(PlayerAnimationEnum.TJump));
                        }
                    }
                    break;
                case PlayerState.Airborne:
                    {
                        if (jumpAble && _prevState != PlayerState.WallBounce)
                        {
                            _state = PlayerState.Jumping;
                        }
                        else if (_isOnFloor)
                        {
                            if (_rb.linearVelocityX == 0)
                            {
                                _state = PlayerState.Idle;
                            }
                            else
                            {
                                _state = PlayerState.Walking;
                            }

                            _jumpRemaining = _stats.MaxJumpCount;
                        }
                        else if (_isOnWall)
                        {
                            _state = PlayerState.WallSlide;
                        }
                    }
                    break;
                case PlayerState.Crouching:
                    if (!_isOnCeil && !crouchable)
                    {
                        if (_rb.linearVelocityX == 0)
                        {
                            _state = PlayerState.Idle;
                        }
                        else
                        {
                            _state = PlayerState.Walking;
                        }
                    }
                    else if (!_isOnCeil && jumpAble)
                    {
                        _state = PlayerState.Jumping;
                    }

                    if (_state == PlayerState.Crouching)
                    {
                        _headCollider.enabled = false;
                    }
                    else
                    {
                        _headCollider.enabled = true;
                    }

                    break;
                case PlayerState.WallSlide:
                    {
                        if (_isOnFloor)
                        {
                            _state = PlayerState.Idle;
                            _jumpRemaining = _stats.MaxJumpCount;
                        }
                        else if (!_isOnWall)
                        {
                            _state = PlayerState.Airborne;
                        }
                        else if (isWallBounceClicked)
                        {
                            _state = PlayerState.WallBounce;
                        }
                    }
                    break;
                case PlayerState.WallBounce:
                    {
                        if (!_isOnWall && _hasWallBounced)
                        {
                            _state = PlayerState.Airborne;
                            _hasWallBounced = false;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (initState != _state)
            {
                _prevState = initState;
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
                case PlayerState.WallSlide:
                case PlayerState.WallBounce:
                    _srNormal.gameObject.SetActive(true);
                    _srCrouch.gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {
            _isOnFloor = Physics2D.OverlapCircle(
                    _groundCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );
            _isOnCeil = Physics2D.OverlapCircle(
                    _ceilCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );
            _isOnWall = Physics2D.OverlapCircle(
                    _wallCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );

            HandleState();
        }

        private void HandleState()
        {
            var movementSmoothing = 0.1f;
            var wallBounceYForce = _stats.JumpForce * 1.2f;

            var horizontalXVelocity = _stats.Speed * _horizontalAxis;
            var direction = transform.localScale.x;

            switch (_state)
            {
                case PlayerState.Jumping:
                    {
                        if (!_hasJumped)
                        {
                            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, 0);
                            _rb.AddForceY(_stats.JumpForce, ForceMode2D.Impulse);
                            _jumpRemaining--;
                            _hasJumped = true;
                        }
                    }
                    break;
                case PlayerState.Airborne:
                case PlayerState.Walking:
                    {
                        if (_prevState != PlayerState.WallBounce)
                        {
                            if (horizontalXVelocity > 0 && transform.localScale.x != 1)
                            {
                                transform.localScale = new Vector2(1, transform.localScale.y);
                            }
                            else if (horizontalXVelocity < 0 && transform.localScale.x != -1)
                            {
                                transform.localScale = new Vector2(-1, transform.localScale.y);
                            }

                            _rb.linearVelocity = new Vector2(
                                    Mathf.Lerp(
                                        _rb.linearVelocityX,
                                        horizontalXVelocity,
                                        movementSmoothing
                                    ),
                                    _rb.linearVelocityY
                            );
                        }
                    }
                    break;
                case PlayerState.Crouching:
                    {
                        _rb.linearVelocity = new Vector2(
                                horizontalXVelocity * _stats.CrouchSpeedMultiplier,
                                -direction * _rb.linearVelocityY
                        );
                    }
                    break;
                case PlayerState.WallSlide:
                    {
                        _rb.linearVelocity = new Vector2(
                                0,
                                Mathf.Max(
                                     _rb.linearVelocityY,
                                    -_stats.WallSlideMaxSpeed
                                )
                        );
                    }
                    break;
                case PlayerState.WallBounce:
                    {
                        if (!_hasWallBounced)
                        {
                            _rb.AddForce(
                                    new Vector2(
                                       -direction * _stats.WallBounceForce,
                                       wallBounceYForce
                                    ),
                                    ForceMode2D.Impulse
                            );
                            transform.localScale = new Vector2(-direction, transform.localScale.y);
                            _hasWallBounced = true;
                        }
                    }
                    break;
                case PlayerState.Idle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
