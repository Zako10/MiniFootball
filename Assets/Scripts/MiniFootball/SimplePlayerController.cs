namespace MiniFootball
{
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class SimplePlayerController : MonoBehaviour
    {
        public enum ControlScheme
        {
            Wasd,
            Arrows
        }

        [SerializeField] private ControlScheme controlScheme = ControlScheme.Wasd;
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float kickForce = 2.2f;
        [SerializeField] private float kickLift = 0.08f;
        [SerializeField] private float jumpForce = 3.2f;
        [SerializeField] private float kickCooldown = 0.18f;

        private Rigidbody rb;
        private Vector3 moveInput;
        private bool jumpRequested;
        private bool grounded;
        private float nextKickTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                moveInput = Vector3.zero;
                return;
            }

            float x;
            float z;
            bool jumpPressed;

            if (controlScheme == ControlScheme.Arrows)
            {
                x = ReadAxis(keyboard.leftArrowKey, keyboard.rightArrowKey);
                z = ReadAxis(keyboard.downArrowKey, keyboard.upArrowKey);
                jumpPressed = keyboard.rightShiftKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame;
            }
            else
            {
                x = ReadAxis(keyboard.aKey, keyboard.dKey);
                z = ReadAxis(keyboard.sKey, keyboard.wKey);
                jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            }

            moveInput = new Vector3(x, 0f, z).normalized;

            if (jumpPressed && grounded)
            {
                jumpRequested = true;
            }
        }

        private void FixedUpdate()
        {
            Vector3 targetVelocity = moveInput * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            if (jumpRequested)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                grounded = false;
                jumpRequested = false;
            }

            if (moveInput.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveInput);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.collider.CompareTag("Ball"))
            {
                return;
            }

            grounded = true;
        }

        private void HandleCollision(Collision collision)
        {
            if (!collision.collider.CompareTag("Ball"))
            {
                grounded = true;
                return;
            }

            if (collision.rigidbody == null || Time.time < nextKickTime)
            {
                return;
            }

            Vector3 kickDirection = (collision.transform.position - transform.position).normalized;
            kickDirection.y = kickLift;
            collision.rigidbody.AddForce(kickDirection.normalized * kickForce, ForceMode.Impulse);
            nextKickTime = Time.time + kickCooldown;
        }

        private static float ReadAxis(KeyControl negative, KeyControl positive)
        {
            float value = 0f;
            if (negative.isPressed)
            {
                value -= 1f;
            }

            if (positive.isPressed)
            {
                value += 1f;
            }

            return value;
        }
    }
}
