using UnityEngine;
using MiniFootball;

public class GoalDetector : MonoBehaviour
{
    [SerializeField] private GoalSide scoringSide;
    [SerializeField] private MiniFootballGameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Debug.Log("GOAL!!!");

            if (gameManager != null)
            {
                gameManager.ScoreGoal(scoringSide);
            }
        }
    }
}
