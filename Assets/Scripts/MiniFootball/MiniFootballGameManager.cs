namespace MiniFootball
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MiniFootballGameManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform player1;
        [SerializeField] private Transform player2;
        [SerializeField] private Rigidbody ball;

        [Header("Spawn Points")]
        [SerializeField] private Transform player1Spawn;
        [SerializeField] private Transform player2Spawn;
        [SerializeField] private Transform ballSpawn;

        [Header("HUD")]
        [SerializeField] private Text scoreboardText;
        [SerializeField] private Text goalMessageText;
        [SerializeField] private float matchDuration = 60f;

        [Header("Goal Detection")]
        [SerializeField] private float goalCooldown = 0.75f;
        [SerializeField] private float goalMessageDuration = 1.35f;

        public int Player1Score { get; private set; }
        public int Player2Score { get; private set; }

        private float timeRemaining;
        private bool matchEnded;
        private GUIStyle hudStyle;
        private GUIStyle resultStyle;
        private Texture2D hudBackground;
        private float nextGoalTime;
        private float goalMessageUntil;
        private string goalMessage;

        private void Start()
        {
            timeRemaining = matchDuration;
            UpdateScoreboard();
        }

        private void Update()
        {
            if (matchEnded)
            {
                return;
            }

            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
            if (timeRemaining <= 0f)
            {
                matchEnded = true;
            }

            UpdateScoreboard();
            UpdateGoalMessage();
        }

        private void OnGUI()
        {
            hudStyle ??= CreateStyle(16, TextAnchor.MiddleCenter);
            resultStyle ??= CreateStyle(42, TextAnchor.MiddleCenter);
            hudBackground ??= CreateTexture(new Color(0.03f, 0.05f, 0.07f, 0.72f));

            Rect hudRect = new Rect(18f, 18f, 170f, 78f);
            GUI.DrawTexture(hudRect, hudBackground);
            GUI.Label(hudRect, BuildScoreText(), hudStyle);

            if (!string.IsNullOrEmpty(goalMessage) && Time.time < goalMessageUntil)
            {
                GUI.Label(new Rect(0f, Screen.height * 0.18f, Screen.width, 90f), goalMessage, resultStyle);
            }

            if (matchEnded)
            {
                GUI.Label(new Rect(0f, Screen.height * 0.4f, Screen.width, 120f), BuildResultText(), resultStyle);
            }
        }

        public void ScoreGoal(GoalSide scoringSide)
        {
            if (matchEnded)
            {
                return;
            }

            if (Time.time < nextGoalTime)
            {
                return;
            }

            if (scoringSide == GoalSide.Player1)
            {
                Player1Score++;
            }
            else
            {
                Player2Score++;
            }

            Debug.Log($"Score: Player 1 {Player1Score} - {Player2Score} Player 2");
            goalMessage = scoringSide == GoalSide.Player1 ? "GOAL! Player 1" : "GOAL! Player 2";
            goalMessageUntil = Time.time + goalMessageDuration;
            nextGoalTime = Time.time + goalCooldown;
            UpdateScoreboard();
            UpdateGoalMessage();
            ResetRoundAfterGoal(scoringSide);
        }

        [ContextMenu(nameof(ResetRound))]
        public void ResetRound()
        {
            MoveToSpawn(player1, player1Spawn);
            MoveToSpawn(player2, player2Spawn);
            ResetBall();
        }

        private void ResetRoundAfterGoal(GoalSide scoringSide)
        {
            MoveToSpawn(player1, player1Spawn);
            MoveToSpawn(player2, player2Spawn);

            Transform kickoffTarget = scoringSide == GoalSide.Player1 ? player2 : player1;
            GiveKickoffTo(kickoffTarget);
        }

        private void MoveToSpawn(Transform target, Transform spawn)
        {
            if (target == null || spawn == null)
            {
                return;
            }

            target.SetPositionAndRotation(spawn.position, spawn.rotation);

            if (target.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void ResetBall()
        {
            if (ball == null || ballSpawn == null)
            {
                return;
            }

            ball.transform.position = ballSpawn.position;
            ball.transform.rotation = ballSpawn.rotation;
            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
        }

        private void GiveKickoffTo(Transform target)
        {
            if (ball == null || target == null)
            {
                ResetBall();
                return;
            }

            Vector3 towardCenter = new Vector3(-target.position.x, 0f, -target.position.z).normalized;
            if (towardCenter.sqrMagnitude < 0.01f)
            {
                towardCenter = target.position.z >= 0f ? Vector3.back : Vector3.forward;
            }

            ball.transform.position = target.position + towardCenter * 1.25f + Vector3.up * 0.35f;
            ball.transform.rotation = Quaternion.identity;
            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
        }

        private void UpdateScoreboard()
        {
            if (scoreboardText == null)
            {
                return;
            }

            scoreboardText.text = BuildScoreText();
        }

        private void UpdateGoalMessage()
        {
            if (goalMessageText == null)
            {
                return;
            }

            bool shouldShow = !string.IsNullOrEmpty(goalMessage) && Time.time < goalMessageUntil;
            goalMessageText.enabled = shouldShow;
            goalMessageText.text = shouldShow ? goalMessage : string.Empty;
        }

        private string BuildScoreText()
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            return $"P1  {Player1Score} - {Player2Score}  P2\n{seconds:00}s";
        }

        private string BuildResultText()
        {
            if (Player1Score > Player2Score)
            {
                return "Full Time\nPlayer 1 Wins";
            }

            if (Player2Score > Player1Score)
            {
                return "Full Time\nPlayer 2 Wins";
            }

            return "Full Time\nDraw";
        }

        private static GUIStyle CreateStyle(int fontSize, TextAnchor alignment)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            return style;
        }

        private static Texture2D CreateTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
