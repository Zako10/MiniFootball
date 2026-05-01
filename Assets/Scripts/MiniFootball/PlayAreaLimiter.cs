namespace MiniFootball
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class PlayAreaLimiter : MonoBehaviour
    {
        [SerializeField] private Vector2 halfSize = new Vector2(5.55f, 10.85f);
        [SerializeField] private float minimumY = -0.4f;
        [SerializeField] private float resetY = 0.8f;
        [SerializeField] private float maxHorizontalSpeed = 8f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            halfSize = new Vector2(Mathf.Min(halfSize.x, 5.55f), Mathf.Min(halfSize.y, 10.85f));
        }

        private void FixedUpdate()
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > maxHorizontalSpeed)
            {
                Vector3 limitedVelocity = horizontalVelocity.normalized * maxHorizontalSpeed;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }

            Vector3 position = rb.position;
            bool changed = false;

            if (position.x < -halfSize.x || position.x > halfSize.x)
            {
                position.x = Mathf.Clamp(position.x, -halfSize.x, halfSize.x);
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, rb.linearVelocity.z);
                changed = true;
            }

            if (position.z < -halfSize.y || position.z > halfSize.y)
            {
                position.z = Mathf.Clamp(position.z, -halfSize.y, halfSize.y);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
                changed = true;
            }

            if (position.y < minimumY)
            {
                position.y = resetY;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                changed = true;
            }

            if (changed)
            {
                rb.position = position;
            }
        }
    }
}
