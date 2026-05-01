namespace MiniFootball
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "MiniFootball/Scene Settings")]
    public class MiniFootballSceneSettings : ScriptableObject
    {
        [Header("Field")]
        public float fieldWidth = 12f;
        public float fieldLength = 20f;
        public float groundY = -0.05f;
        public Color groundColor = new Color(0.12f, 0.55f, 0.2f);
        public Color lineColor = new Color(0.94f, 0.94f, 0.88f);

        [Header("Goals")]
        public float goalOpeningHalfWidth = 3.2f;
        public float goalDepth = 1.1f;
        public float goalTriggerHeight = 2.7f;
        public float goalTriggerDepth = 0.45f;
        public Vector3 goalScale = new Vector3(2.6f, 2.2f, 2.2f);

        [Header("Walls")]
        public float wallHeight = 3.2f;
        public float wallThickness = 0.45f;
        public Color wallColor = new Color(0.06f, 0.08f, 0.1f);
        public Color goalNetColor = new Color(0.86f, 0.9f, 0.92f);
        public Vector2 playAreaHalfSize = new Vector2(5.55f, 10.85f);

        [Header("Actors")]
        public Vector3 ballSpawnPosition = new Vector3(0f, 0.35f, 0f);
        public float ballScale = 0.5f;
        public Vector3 player1SpawnPosition = new Vector3(0f, 1f, -4.5f);
        public Vector3 player2SpawnPosition = new Vector3(0f, 1f, 4.5f);
        public float playerScale = 1.15f;
        public Color player1Color = new Color(0.1f, 0.35f, 1f);
        public Color player2Color = new Color(1f, 0.18f, 0.12f);

        [Header("Camera")]
        public Vector3 cameraOffset = new Vector3(0f, 6.5f, -7.5f);
        public Vector3 cameraEuler = new Vector3(48f, 0f, 0f);
        public float cameraFov = 55f;
        public Color skyColor = new Color(0.45f, 0.68f, 0.88f);

        [Header("Backdrop")]
        public bool createStands = true;
        public bool createAdvertisingBoards = true;
        public bool createFloodlights = true;
        public Color standDarkColor = new Color(0.07f, 0.09f, 0.13f);
        public Color standBlueColor = new Color(0.04f, 0.2f, 0.55f);
        public Color standRedColor = new Color(0.72f, 0.08f, 0.12f);
        public Color adLeftColor = new Color(0.95f, 0.88f, 0.18f);
        public Color adRightColor = new Color(0.18f, 0.82f, 0.95f);
        public Color floodlightColor = new Color(1f, 0.95f, 0.74f);

        public float HalfFieldWidth => fieldWidth * 0.5f;
        public float HalfFieldLength => fieldLength * 0.5f;
    }
}
