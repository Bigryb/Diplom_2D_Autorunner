using RusRunner.Core;
using UnityEngine;

namespace RusRunner.Game.Runner
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class RunnerController : MonoBehaviour
    {
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float gravityScale = 3f;
        [SerializeField] private float slideDuration = 0.45f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;

        private Rigidbody2D _rb;
        private float _slideLeft;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = gravityScale;
        }

        public void Tick(GameContext context, float deltaTime)
        {
            context.IsGrounded = IsGrounded();

            if (_slideLeft > 0f)
            {
                _slideLeft -= deltaTime;
            }
        }

        public void MoveForward(float speed)
        {
            var p = transform.position;
            p.x += speed * Time.deltaTime;
            transform.position = p;
        }

        public void Jump(GameContext context)
        {
            if (context.IsGrounded)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, 0f);
                _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                return;
            }

            if (context.AirJumpsLeft <= 0)
            {
                return;
            }

            context.AirJumpsLeft -= 1;
            _rb.velocity = new Vector2(_rb.velocity.x, 0f);
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        public void Slide()
        {
            _slideLeft = slideDuration;
            // Add collider resize animation here for production build.
        }

        private bool IsGrounded()
        {
            if (groundCheck == null)
            {
                return false;
            }

            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask) != null;
        }
    }
}
