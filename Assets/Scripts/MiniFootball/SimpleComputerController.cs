namespace MiniFootball
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class SimpleComputerController : MonoBehaviour
    {
        [SerializeField] private Transform ball;
        [SerializeField] private Transform homePosition;
        [SerializeField] private float moveSpeed = 6.4f;
        [SerializeField] private float turnSpeed = 14f;
        [SerializeField] private float chaseDistance = 18f;
        [SerializeField] private float kickForce = 3.4f;
        [SerializeField] private float kickLift = 0.06f;
        [SerializeField] private float approachOffset = 0.55f;
        [SerializeField] private float stoppingDistance = 0.12f;
        [SerializeField] private float kickCooldown = 0.22f;
        [SerializeField] private float directChaseDistance = 2.1f;

        private Rigidbody rb;
        private float nextKickTime;
        private static readonly Vector3 AttackDirection = Vector3.back;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            if (ball == null)
            {
                GameObject ballObject = GameObject.FindGameObjectWithTag("Ball");
                if (ballObject != null)
                {
                    ball = ballObject.transform;
                }
            }
        }

        private void FixedUpdate()
        {
            if (ball == null)
            {
                return;
            }

            rb.WakeUp();

            float ballDistance = Vector3.Distance(transform.position, ball.position);
            Vector3 destination = ballDistance <= directChaseDistance
                ? ball.position
                : ballDistance <= chaseDistance
                ? ball.position - AttackDirection * approachOffset
                : homePosition != null ? homePosition.position : transform.position;

            Vector3 toDestination = destination - transform.position;
            toDestination.y = 0f;

            Vector3 moveDirection = toDestination.sqrMagnitude > stoppingDistance * stoppingDistance
                ? toDestination.normalized
                : Vector3.zero;

            Vector3 targetVelocity = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.CompareTag("Ball") || collision.rigidbody == null || Time.time < nextKickTime)
            {
                return;
            }

            Vector3 kickDirection = AttackDirection;
            kickDirection.y = kickLift;
            collision.rigidbody.AddForce(kickDirection.normalized * kickForce, ForceMode.Impulse);
            nextKickTime = Time.time + kickCooldown;
        }
    }
}
