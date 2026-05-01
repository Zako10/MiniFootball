using UnityEngine;
using MiniFootball;

public class GoalDetector : MonoBehaviour
{
    private const float GoalZ = 10.1f;

    [SerializeField] private GoalSide scoringSide;
    [SerializeField] private MiniFootballGameManager gameManager;
    [SerializeField] private float goalHalfWidth = 3.2f;
    [SerializeField] private float goalZ = GoalZ;
    [SerializeField] private float goalTriggerDepth = 0.45f;
    [SerializeField] private float minimumBallHeight = 0.05f;
    [SerializeField] private float maximumBallHeight = 2.7f;

    private void Awake()
    {
        float z = scoringSide == GoalSide.Player1 ? Mathf.Abs(goalZ) : -Mathf.Abs(goalZ);
        transform.position = new Vector3(0f, 1.35f, z);
        transform.localScale = new Vector3(goalHalfWidth * 2f, maximumBallHeight, goalTriggerDepth);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball"))
        {
            return;
        }

        Vector3 localPosition = transform.InverseTransformPoint(other.bounds.center);
        if (Mathf.Abs(localPosition.x) > goalHalfWidth || other.bounds.center.y < minimumBallHeight || other.bounds.center.y > maximumBallHeight)
        {
            return;
        }

        Debug.Log("GOAL!!!");

        if (gameManager != null)
        {
            gameManager.ScoreGoal(scoringSide);
        }
    }
}
