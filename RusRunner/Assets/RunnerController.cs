using RusRunner.Core;
using UnityEngine;

namespace RusRunner.Game.Runner
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class RunnerController : MonoBehaviour
    {
        [SerializeField] private float jumpForce = 11.5f;
        [SerializeField] private float gravityScale = 3f;
        [SerializeField] private float slideDuration = 0.22f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.16f;
        [SerializeField] private float coyoteTime = 0.08f;
        [SerializeField] private float postJumpUngroundedTime = 0.12f;

        private float _coyoteLeft;
        private float _slideLeft;
        private float _postJumpGroundLockLeft;
        private float _lockedX;
        private bool _groundJumpConsumed;
        private float _defaultGravityScale;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _capsule;
        private Vector2 _defaultCapsuleSize;
        private Vector2 _defaultCapsuleOffset;
        private Vector2 _slideCapsuleSize;
        private Vector2 _slideCapsuleOffset;

        public bool IsSliding => _slideLeft > 0f;

        private void Awake()
        {
            _lockedX = transform.position.x;

            _rb = GetComponent<Rigidbody2D>();
            _defaultGravityScale = gravityScale;
            _rb.gravityScale = gravityScale;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            _capsule = GetComponent<CapsuleCollider2D>();
            if (_capsule != null)
            {
                if (_capsule.size.y < 0.9f)
                {
                    _capsule.size = new Vector2(0.7f, 1.2f);
                }

                _defaultCapsuleSize = _capsule.size;
                _defaultCapsuleOffset = _capsule.offset;
                _slideCapsuleSize = new Vector2(_defaultCapsuleSize.x + 0.2f, _defaultCapsuleSize.y * 0.5f);
                _slideCapsuleOffset = new Vector2(_defaultCapsuleOffset.x, _defaultCapsuleOffset.y - 0.3f);
            }

            if (groundCheck == null)
            {
                var existing = transform.Find("GroundCheck");
                if (existing != null)
                {
                    groundCheck = existing;
                }
                else
                {
                    var go = new GameObject("GroundCheck");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(0f, -0.72f, 0f);
                    groundCheck = go.transform;
                }
            }

            if (groundMask.value == 0)
            {
                var groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer >= 0)
                {
                    groundMask = 1 << groundLayer;
                }
                else
                {
                    groundMask = Physics2D.AllLayers;
                }
            }
        }

        public void Tick(GameContext context, float deltaTime)
        {
            if (_postJumpGroundLockLeft > 0f)
            {
                _postJumpGroundLockLeft -= deltaTime;
                context.IsGrounded = false;
            }
            else
            {
                context.IsGrounded = IsGrounded();
            }

            if (context.IsGrounded)
            {
                _coyoteLeft = coyoteTime;
                _groundJumpConsumed = false;
            }
            else
            {
                _coyoteLeft -= deltaTime;
            }

            if (_slideLeft > 0f)
            {
                _slideLeft -= deltaTime;
                ApplySlideCollider();
                context.IsSliding = true;
            }
            else
            {
                RestoreCollider();
                context.IsSliding = false;
            }

            var position = transform.position;
            position.x = _lockedX;
            transform.position = position;
        }

        public void MoveForward(float speed)
        {
            var velocity = _rb.linearVelocity;
            velocity.x = 0f;
            _rb.linearVelocity = velocity;
        }

        public void KeepFixedOnGround()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }

            var position = transform.position;
            position.x = _lockedX;
            transform.position = position;
            RestoreCollider();
        }

        public void RestoreAfterMenuOrRestart()
        {
            if (_rb == null)
            {
                return;
            }

            _rb.WakeUp();
            _rb.gravityScale = _defaultGravityScale;
            _rb.linearVelocity = Vector2.zero;
            _slideLeft = 0f;
            _postJumpGroundLockLeft = 0f;
            _groundJumpConsumed = false;
            RestoreCollider();
        }

        public bool TryGroundJump(GameContext context)
        {
            if (_groundJumpConsumed || !(context.IsGrounded || _coyoteLeft > 0f))
            {
                return false;
            }

            _groundJumpConsumed = true;
            _coyoteLeft = 0f;
            _postJumpGroundLockLeft = postJumpUngroundedTime;
            _slideLeft = 0f;
            context.IsSliding = false;
            context.AirJumpsLeft = 1;
            RestoreCollider();
            JumpImpulse(jumpForce);
            context.IsGrounded = false;
            return true;
        }

        public bool TryAirJump(GameContext context)
        {
            if (context.IsGrounded || context.AirJumpsLeft <= 0)
            {
                return false;
            }

            context.AirJumpsLeft -= 1;
            _postJumpGroundLockLeft = postJumpUngroundedTime;
            JumpImpulse(jumpForce * 0.94f);
            context.IsGrounded = false;
            return true;
        }

        public bool Slide(GameContext context)
        {
            if (!context.IsGrounded || _slideLeft > 0f)
            {
                return false;
            }

            _slideLeft = slideDuration;
            context.IsSliding = true;
            ApplySlideCollider();
            return true;
        }

        public void Halt()
        {
            if (_rb == null)
            {
                return;
            }

            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.Sleep();
        }

        private void JumpImpulse(float force)
        {
            _rb.gravityScale = _defaultGravityScale;
            _rb.WakeUp();
            _rb.linearVelocity = new Vector2(0f, 0f);
            _rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }

        private void ApplySlideCollider()
        {
            if (_capsule == null)
            {
                return;
            }

            _capsule.size = _slideCapsuleSize;
            _capsule.offset = _slideCapsuleOffset;
        }

        private void RestoreCollider()
        {
            if (_capsule == null)
            {
                return;
            }

            _capsule.size = _defaultCapsuleSize;
            _capsule.offset = _defaultCapsuleOffset;
        }

        private bool IsGrounded()
        {
            if (groundCheck == null)
            {
                return false;
            }

            var downwardVelocity = _rb != null ? _rb.linearVelocity.y : 0f;
            if (downwardVelocity > 0.25f)
            {
                return false;
            }

            var hit = Physics2D.Raycast(groundCheck.position + Vector3.up * 0.03f, Vector2.down, 0.16f, groundMask);
            return hit.collider != null && !hit.collider.isTrigger;
        }
    }
}
