using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int playerScore = 0;

    void Awake()
    {
        instance = this;
    }

    public void Goal()
    {
        playerScore++;
        Debug.Log("Score: " + playerScore);
    }
}