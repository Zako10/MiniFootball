namespace MiniFootball.EditorTools
{
    using System.Collections.Generic;
    using MiniFootball;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    public static class MiniFootballSceneBuilder
    {
        private const string CharacterPrefabPath = "Assets/Lightning Poly/Football Essentials 3D/Prefabs/Character.prefab";
        private const string BallPrefabPath = "Assets/Lightning Poly/Football Essentials 3D/Prefabs/Ball.prefab";
        private const string GroundPrefabPath = "Assets/Lightning Poly/Football Essentials 3D/Prefabs/Ground.prefab";
        private const string GoalPrefabPath = "Assets/Lightning Poly/Football Essentials 3D/Prefabs/Goal.prefab";
        private const string SettingsAssetPath = "Assets/Settings/MiniFootballSceneSettings.asset";
        [MenuItem("MiniFootball/Build First Playable Scene")]
        public static void BuildFirstPlayableScene()
        {
            MiniFootballSceneSettings settings = LoadOrCreateSettings();
            EnsureBallTag();
            ClearPreviousSetup();

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, settings.groundY, 0f);
            ground.transform.localScale = new Vector3(settings.fieldWidth, 0.1f, settings.fieldLength);
            EnsureGroundMarker(ground);
            SetMaterialColor(ground, settings.groundColor);
            CreatePitchLines(settings);
            CreateBoundaryWalls(settings);
            CreateSportsBackdrop(settings);

            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball";
            ball.transform.position = settings.ballSpawnPosition;
            ball.transform.localScale = Vector3.one * settings.ballScale;
            ball.tag = "Ball";
            Rigidbody ballRb = EnsureRigidbody(ball);
            ballRb.mass = 0.8f;
            ballRb.linearDamping = 0.9f;
            ballRb.angularDamping = 1.2f;
            ballRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            EnsureCollider(ball);
            SetPhysicsMaterial(ball, 0.6f, 0.05f);
            ball.AddComponent<LightningPoly.FootballEssentials3D.Ball>();
            AddPlayAreaLimiter(ball, settings.playAreaHalfSize);
            SetMaterialColor(ball, Color.white);

            GameObject player1 = InstantiatePrefab(CharacterPrefabPath, "Player 1", settings.player1SpawnPosition, Quaternion.identity);
            player1.transform.localScale = Vector3.one * settings.playerScale;
            CleanDemoPlayerScript(player1);
            EnsureRigidbody(player1).constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            EnsureCollider(player1);
            SimplePlayerController player1Controller = player1.AddComponent<SimplePlayerController>();
            SetControlScheme(player1Controller, SimplePlayerController.ControlScheme.Arrows);
            AddPlayAreaLimiter(player1, settings.playAreaHalfSize);
            SetMaterialColorRecursive(player1, settings.player1Color);

            GameObject player2 = InstantiatePrefab(CharacterPrefabPath, "Player 2", settings.player2SpawnPosition, Quaternion.Euler(0f, 180f, 0f));
            player2.transform.localScale = Vector3.one * settings.playerScale;
            CleanDemoPlayerScript(player2);
            EnsureRigidbody(player2).constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            EnsureCollider(player2);
            SimplePlayerController player2Controller = player2.AddComponent<SimplePlayerController>();
            SetControlScheme(player2Controller, SimplePlayerController.ControlScheme.Wasd);
            AddPlayAreaLimiter(player2, settings.playAreaHalfSize);
            SetMaterialColorRecursive(player2, settings.player2Color);

            GameObject player1Goal = InstantiatePrefab(GoalPrefabPath, "Player 1 Goal", new Vector3(0f, 0f, -settings.HalfFieldLength), Quaternion.identity);
            player1Goal.transform.localScale = settings.goalScale;
            SetMaterialColorRecursive(player1Goal, Color.white);

            GameObject player2Goal = InstantiatePrefab(GoalPrefabPath, "Player 2 Goal", new Vector3(0f, 0f, settings.HalfFieldLength), Quaternion.Euler(0f, 180f, 0f));
            player2Goal.transform.localScale = settings.goalScale;
            SetMaterialColorRecursive(player2Goal, Color.white);

            Transform player1Spawn = CreateSpawn("Player1Spawn", player1.transform.position, player1.transform.rotation);
            Transform player2Spawn = CreateSpawn("Player2Spawn", player2.transform.position, player2.transform.rotation);
            Transform ballSpawn = CreateSpawn("BallSpawn", ball.transform.position, ball.transform.rotation);

            GameObject gameManagerObject = new GameObject("GameManager");
            MiniFootballGameManager gameManager = gameManagerObject.AddComponent<MiniFootballGameManager>();
            Text[] hudTexts = CreateHud();
            SetObjectReference(gameManager, "player1", player1.transform);
            SetObjectReference(gameManager, "player2", player2.transform);
            SetObjectReference(gameManager, "ball", ballRb);
            SetObjectReference(gameManager, "player1Spawn", player1Spawn);
            SetObjectReference(gameManager, "player2Spawn", player2Spawn);
            SetObjectReference(gameManager, "ballSpawn", ballSpawn);
            SetObjectReference(gameManager, "scoreboardText", hudTexts[0]);
            SetObjectReference(gameManager, "goalMessageText", hudTexts[1]);

            CreateGoalTrigger("Player 1 Goal Trigger", new Vector3(0f, settings.goalTriggerHeight * 0.5f, -settings.HalfFieldLength - 0.1f), GoalSide.Player2, gameManager, settings);
            CreateGoalTrigger("Player 2 Goal Trigger", new Vector3(0f, settings.goalTriggerHeight * 0.5f, settings.HalfFieldLength + 0.1f), GoalSide.Player1, gameManager, settings);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = player1.transform.position + settings.cameraOffset;
                mainCamera.transform.rotation = Quaternion.Euler(settings.cameraEuler);
                mainCamera.fieldOfView = settings.cameraFov;
                SimpleFollowCamera followCamera = mainCamera.GetComponent<SimpleFollowCamera>();
                if (followCamera == null)
                {
                    followCamera = mainCamera.gameObject.AddComponent<SimpleFollowCamera>();
                }

                SetObjectReference(followCamera, "target", player1.transform);
                SetVector3(followCamera, "offset", settings.cameraOffset);
            }

            ApplyCapturedObjectOverrides(settings);

            Selection.activeGameObject = player1;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("MiniFootball local 1v1 scene is built and saved. Player 1: arrows + Space. Player 2: WASD + Left Shift.");
        }

        [MenuItem("MiniFootball/Capture Current Scene Settings")]
        public static void CaptureCurrentSceneSettings()
        {
            MiniFootballSceneSettings settings = LoadOrCreateSettings();

            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                settings.fieldWidth = ground.transform.localScale.x;
                settings.fieldLength = ground.transform.localScale.z;
                settings.groundY = ground.transform.position.y;
                settings.groundColor = ReadColor(ground, settings.groundColor);
            }

            CaptureTransform("Ball", value => settings.ballSpawnPosition = value.position, value => settings.ballScale = value.localScale.x);
            CaptureTransform("Player 1", value => settings.player1SpawnPosition = value.position, value => settings.playerScale = value.localScale.x);
            CaptureTransform("Player 2", value => settings.player2SpawnPosition = value.position, value => settings.playerScale = value.localScale.x);

            GameObject player1 = GameObject.Find("Player 1");
            if (player1 != null)
            {
                settings.player1Color = ReadColorInChildren(player1, settings.player1Color);
            }

            GameObject player2 = GameObject.Find("Player 2");
            if (player2 != null)
            {
                settings.player2Color = ReadColorInChildren(player2, settings.player2Color);
            }

            GameObject player1Goal = GameObject.Find("Player 1 Goal");
            if (player1Goal != null)
            {
                settings.goalScale = player1Goal.transform.localScale;
            }

            GameObject trigger = GameObject.Find("Player 2 Goal Trigger") ?? GameObject.Find("Player 1 Goal Trigger");
            if (trigger != null)
            {
                settings.goalOpeningHalfWidth = trigger.transform.localScale.x * 0.5f;
                settings.goalTriggerHeight = trigger.transform.localScale.y;
                settings.goalTriggerDepth = trigger.transform.localScale.z;
            }

            GameObject leftWall = GameObject.Find("Left Wall");
            if (leftWall != null)
            {
                settings.wallThickness = leftWall.transform.localScale.x;
                settings.wallHeight = leftWall.transform.localScale.y;
                settings.wallColor = ReadColor(leftWall, settings.wallColor);
            }

            GameObject goalSideWall = GameObject.Find("Player Goal Left Wall");
            if (goalSideWall != null)
            {
                settings.goalDepth = Mathf.Max(0.1f, goalSideWall.transform.localScale.z - settings.wallThickness);
                settings.goalNetColor = ReadColor(goalSideWall, settings.goalNetColor);
            }

            CapturePlayArea(settings);
            CaptureBackdrop(settings);
            CaptureCamera(settings);
            CaptureGeneratedObjectOverrides(settings);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"Captured current scene settings only into {SettingsAssetPath}. No scene objects were rebuilt or deleted.");
        }

        private static GameObject InstantiatePrefab(string path, string objectName, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new System.InvalidOperationException($"Missing prefab at {path}");
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = objectName;
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        private static void CaptureGeneratedObjectOverrides(MiniFootballSceneSettings settings)
        {
            settings.objectOverrides.Clear();

            foreach (string objectName in GetGeneratedObjectNames())
            {
                MiniFootballObjectOverride captured = new MiniFootballObjectOverride
                {
                    objectName = objectName
                };

                GameObject target = GameObject.Find(objectName);
                if (target == null)
                {
                    captured.exists = false;
                    settings.objectOverrides.Add(captured);
                    continue;
                }

                captured.exists = true;
                captured.active = target.activeSelf;
                captured.position = target.transform.position;
                captured.eulerAngles = target.transform.rotation.eulerAngles;
                captured.scale = target.transform.localScale;

                Renderer renderer = target.GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    captured.hasColor = true;
                    captured.color = renderer.sharedMaterial.color;
                }

                settings.objectOverrides.Add(captured);
            }
        }

        private static void ApplyCapturedObjectOverrides(MiniFootballSceneSettings settings)
        {
            if (settings.objectOverrides == null || settings.objectOverrides.Count == 0)
            {
                return;
            }

            foreach (MiniFootballObjectOverride captured in settings.objectOverrides)
            {
                if (string.IsNullOrEmpty(captured.objectName))
                {
                    continue;
                }

                GameObject target = GameObject.Find(captured.objectName);
                if (target == null)
                {
                    continue;
                }

                if (!captured.exists)
                {
                    Object.DestroyImmediate(target);
                    continue;
                }

                target.SetActive(captured.active);
                target.transform.position = captured.position;
                target.transform.rotation = Quaternion.Euler(captured.eulerAngles);
                target.transform.localScale = captured.scale;

                if (captured.hasColor)
                {
                    SetMaterialColorRecursive(target, captured.color);
                }
            }
        }

        private static void CaptureTransform(string objectName, System.Action<Transform> capturePosition, System.Action<Transform> captureScale)
        {
            GameObject target = GameObject.Find(objectName);
            if (target == null)
            {
                return;
            }

            capturePosition?.Invoke(target.transform);
            captureScale?.Invoke(target.transform);
        }

        private static IEnumerable<string> GetGeneratedObjectNames()
        {
            string[] fixedNames =
            {
                "Ground",
                "Ball",
                "Player 1",
                "Player 2",
                "Player 1 Goal",
                "Player 2 Goal",
                "Player 1 Goal Trigger",
                "Player 2 Goal Trigger",
                "GameManager",
                "Player1Spawn",
                "Player2Spawn",
                "BallSpawn",
                "Pitch Lines",
                "Boundary Walls",
                "HUD Canvas",
                "Scoreboard Panel",
                "Scoreboard",
                "Goal Message",
                "Sports Backdrop",
                "Left Touchline",
                "Right Touchline",
                "Player Goal Line",
                "Computer Goal Line",
                "Halfway Line",
                "Player Box",
                "Player Box Left",
                "Player Box Right",
                "Computer Box",
                "Computer Box Left",
                "Computer Box Right",
                "Left Wall",
                "Right Wall",
                "Player End Wall Left",
                "Player End Wall Right",
                "Computer End Wall Left",
                "Computer End Wall Right",
                "Player Goal Back Wall",
                "Computer Goal Back Wall",
                "Player Goal Left Wall",
                "Player Goal Right Wall",
                "Computer Goal Left Wall",
                "Computer Goal Right Wall",
                "North Stand Base",
                "South Stand Base",
                "North Stand Seats",
                "South Stand Seats",
                "Left Advertising Board",
                "Right Advertising Board",
                "Left Floodlight",
                "Right Floodlight"
            };

            foreach (string objectName in fixedNames)
            {
                yield return objectName;
            }

            for (int i = 1; i <= 40; i++)
            {
                yield return $"Center Circle Segment {i}";
            }
        }

        private static void CapturePlayArea(MiniFootballSceneSettings settings)
        {
            PlayAreaLimiter limiter = Object.FindAnyObjectByType<PlayAreaLimiter>();
            if (limiter == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(limiter);
            SerializedProperty halfSize = serializedObject.FindProperty("halfSize");
            if (halfSize != null)
            {
                settings.playAreaHalfSize = halfSize.vector2Value;
            }
        }

        private static void CaptureBackdrop(MiniFootballSceneSettings settings)
        {
            settings.createStands = ExistsAny("North Stand Base", "South Stand Base", "North Stand Seats", "South Stand Seats");
            settings.createAdvertisingBoards = ExistsAny("Left Advertising Board", "Right Advertising Board");
            settings.createFloodlights = ExistsAny("Left Floodlight", "Right Floodlight");

            settings.standDarkColor = ReadColorByName("North Stand Base", settings.standDarkColor);
            settings.standBlueColor = ReadColorByName("North Stand Seats", settings.standBlueColor);
            settings.standRedColor = ReadColorByName("South Stand Seats", settings.standRedColor);
            settings.adLeftColor = ReadColorByName("Left Advertising Board", settings.adLeftColor);
            settings.adRightColor = ReadColorByName("Right Advertising Board", settings.adRightColor);
            settings.floodlightColor = ReadColorByName("Left Floodlight", settings.floodlightColor);
        }

        private static void CaptureCamera(MiniFootballSceneSettings settings)
        {
            Camera mainCamera = Camera.main;
            GameObject player1 = GameObject.Find("Player 1");
            if (mainCamera == null)
            {
                return;
            }

            settings.skyColor = mainCamera.backgroundColor;
            settings.cameraEuler = mainCamera.transform.rotation.eulerAngles;
            settings.cameraFov = mainCamera.fieldOfView;

            if (player1 != null)
            {
                settings.cameraOffset = mainCamera.transform.position - player1.transform.position;
            }
        }

        private static bool ExistsAny(params string[] objectNames)
        {
            foreach (string objectName in objectNames)
            {
                if (GameObject.Find(objectName) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static Color ReadColorByName(string objectName, Color fallback)
        {
            GameObject target = GameObject.Find(objectName);
            return target == null ? fallback : ReadColor(target, fallback);
        }

        private static Color ReadColorInChildren(GameObject target, Color fallback)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return fallback;
            }

            return renderer.sharedMaterial.color;
        }

        private static Color ReadColor(GameObject target, Color fallback)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return fallback;
            }

            return renderer.sharedMaterial.color;
        }

        private static void ClearPreviousSetup()
        {
            string[] names =
            {
                "Ground",
                "Ball",
                "Player",
                "Computer",
                "Player 1",
                "Player 2",
                "Player Goal",
                "Computer Goal",
                "Player 1 Goal",
                "Player 2 Goal",
                "Player Goal Trigger",
                "Computer Goal Trigger",
                "Player 1 Goal Trigger",
                "Player 2 Goal Trigger",
                "GameManager",
                "PlayerSpawn",
                "ComputerSpawn",
                "Player1Spawn",
                "Player2Spawn",
                "BallSpawn",
                "ComputerHome",
                "Pitch Lines",
                "Boundary Walls",
                "HUD Canvas",
                "SM_Stadium",
                "Sports Backdrop"
            };

            foreach (string objectName in names)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static Transform CreateSpawn(string objectName, Vector3 position, Quaternion rotation)
        {
            GameObject spawn = new GameObject(objectName);
            spawn.transform.SetPositionAndRotation(position, rotation);
            return spawn.transform;
        }

        private static void CreateGoalTrigger(string objectName, Vector3 position, GoalSide scoringSide, MiniFootballGameManager gameManager, MiniFootballSceneSettings settings)
        {
            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.name = objectName;
            trigger.transform.position = position;
            trigger.transform.localScale = new Vector3(settings.goalOpeningHalfWidth * 2f, settings.goalTriggerHeight, settings.goalTriggerDepth);

            MeshRenderer renderer = trigger.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Object.DestroyImmediate(renderer);
            }

            BoxCollider collider = trigger.GetComponent<BoxCollider>();
            collider.isTrigger = true;

            GoalDetector goalDetector = trigger.AddComponent<GoalDetector>();
            SetEnum(goalDetector, "scoringSide", scoringSide);
            SetObjectReference(goalDetector, "gameManager", gameManager);
            SetFloat(goalDetector, "goalHalfWidth", settings.goalOpeningHalfWidth);
            SetFloat(goalDetector, "goalZ", settings.HalfFieldLength + 0.1f);
            SetFloat(goalDetector, "goalTriggerDepth", settings.goalTriggerDepth);
            SetFloat(goalDetector, "maximumBallHeight", settings.goalTriggerHeight);
        }

        private static void CreatePitchLines(MiniFootballSceneSettings settings)
        {
            GameObject parent = new GameObject("Pitch Lines");
            Color lineColor = settings.lineColor;

            CreateLine(parent.transform, "Left Touchline", new Vector3(-settings.HalfFieldWidth, 0.006f, 0f), new Vector3(0.12f, 0.012f, settings.fieldLength), lineColor);
            CreateLine(parent.transform, "Right Touchline", new Vector3(settings.HalfFieldWidth, 0.006f, 0f), new Vector3(0.12f, 0.012f, settings.fieldLength), lineColor);
            CreateLine(parent.transform, "Player Goal Line", new Vector3(0f, 0.006f, -settings.HalfFieldLength), new Vector3(settings.fieldWidth, 0.012f, 0.12f), lineColor);
            CreateLine(parent.transform, "Computer Goal Line", new Vector3(0f, 0.006f, settings.HalfFieldLength), new Vector3(settings.fieldWidth, 0.012f, 0.12f), lineColor);
            CreateLine(parent.transform, "Halfway Line", new Vector3(0f, 0.007f, 0f), new Vector3(settings.fieldWidth, 0.012f, 0.1f), lineColor);
            CreateLine(parent.transform, "Player Box", new Vector3(0f, 0.008f, -settings.HalfFieldLength + 2f), new Vector3(5.4f, 0.012f, 0.1f), lineColor);
            CreateLine(parent.transform, "Player Box Left", new Vector3(-2.7f, 0.008f, -settings.HalfFieldLength + 1f), new Vector3(0.1f, 0.012f, 2f), lineColor);
            CreateLine(parent.transform, "Player Box Right", new Vector3(2.7f, 0.008f, -settings.HalfFieldLength + 1f), new Vector3(0.1f, 0.012f, 2f), lineColor);
            CreateLine(parent.transform, "Computer Box", new Vector3(0f, 0.008f, settings.HalfFieldLength - 2f), new Vector3(5.4f, 0.012f, 0.1f), lineColor);
            CreateLine(parent.transform, "Computer Box Left", new Vector3(-2.7f, 0.008f, settings.HalfFieldLength - 1f), new Vector3(0.1f, 0.012f, 2f), lineColor);
            CreateLine(parent.transform, "Computer Box Right", new Vector3(2.7f, 0.008f, settings.HalfFieldLength - 1f), new Vector3(0.1f, 0.012f, 2f), lineColor);
            CreateCircle(parent.transform, "Center Circle", Vector3.zero, 1.8f, 40, lineColor);
        }

        private static void CreateBoundaryWalls(MiniFootballSceneSettings settings)
        {
            GameObject parent = new GameObject("Boundary Walls");
            Color railColor = settings.wallColor;
            Color netColor = settings.goalNetColor;
            float wallHeight = settings.wallHeight;
            float wallThickness = settings.wallThickness;
            float endWallLength = settings.HalfFieldWidth - settings.goalOpeningHalfWidth;
            float endWallCenterX = settings.goalOpeningHalfWidth + endWallLength * 0.5f;
            float goalSideX = settings.goalOpeningHalfWidth + wallThickness * 0.5f;
            float goalBackZ = settings.HalfFieldLength + settings.goalDepth;
            float goalSideZ = settings.HalfFieldLength + settings.goalDepth * 0.5f;

            CreateBox(parent.transform, "Left Wall", new Vector3(-settings.HalfFieldWidth - wallThickness * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, settings.fieldLength + settings.goalDepth * 2f), railColor);
            CreateBox(parent.transform, "Right Wall", new Vector3(settings.HalfFieldWidth + wallThickness * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, settings.fieldLength + settings.goalDepth * 2f), railColor);
            CreateBox(parent.transform, "Player End Wall Left", new Vector3(-endWallCenterX, wallHeight * 0.5f, -settings.HalfFieldLength - wallThickness * 0.5f), new Vector3(endWallLength, wallHeight, wallThickness), railColor);
            CreateBox(parent.transform, "Player End Wall Right", new Vector3(endWallCenterX, wallHeight * 0.5f, -settings.HalfFieldLength - wallThickness * 0.5f), new Vector3(endWallLength, wallHeight, wallThickness), railColor);
            CreateBox(parent.transform, "Computer End Wall Left", new Vector3(-endWallCenterX, wallHeight * 0.5f, settings.HalfFieldLength + wallThickness * 0.5f), new Vector3(endWallLength, wallHeight, wallThickness), railColor);
            CreateBox(parent.transform, "Computer End Wall Right", new Vector3(endWallCenterX, wallHeight * 0.5f, settings.HalfFieldLength + wallThickness * 0.5f), new Vector3(endWallLength, wallHeight, wallThickness), railColor);
            CreateBox(parent.transform, "Player Goal Back Wall", new Vector3(0f, wallHeight * 0.5f, -goalBackZ), new Vector3(settings.goalOpeningHalfWidth * 2f + wallThickness * 2f, wallHeight, wallThickness), netColor);
            CreateBox(parent.transform, "Computer Goal Back Wall", new Vector3(0f, wallHeight * 0.5f, goalBackZ), new Vector3(settings.goalOpeningHalfWidth * 2f + wallThickness * 2f, wallHeight, wallThickness), netColor);
            CreateBox(parent.transform, "Player Goal Left Wall", new Vector3(-goalSideX, wallHeight * 0.5f, -goalSideZ), new Vector3(wallThickness, wallHeight, settings.goalDepth + wallThickness), netColor);
            CreateBox(parent.transform, "Player Goal Right Wall", new Vector3(goalSideX, wallHeight * 0.5f, -goalSideZ), new Vector3(wallThickness, wallHeight, settings.goalDepth + wallThickness), netColor);
            CreateBox(parent.transform, "Computer Goal Left Wall", new Vector3(-goalSideX, wallHeight * 0.5f, goalSideZ), new Vector3(wallThickness, wallHeight, settings.goalDepth + wallThickness), netColor);
            CreateBox(parent.transform, "Computer Goal Right Wall", new Vector3(goalSideX, wallHeight * 0.5f, goalSideZ), new Vector3(wallThickness, wallHeight, settings.goalDepth + wallThickness), netColor);
        }

        private static void CreateSportsBackdrop(MiniFootballSceneSettings settings)
        {
            GameObject parent = new GameObject("Sports Backdrop");
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = settings.skyColor;
            }

            Light directionalLight = Object.FindAnyObjectByType<Light>();
            if (directionalLight != null)
            {
                directionalLight.intensity = 1.35f;
                directionalLight.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            }

            if (settings.createStands)
            {
                CreateDecorBox(parent.transform, "North Stand Base", new Vector3(0f, 1f, settings.HalfFieldLength + 4f), new Vector3(15f, 2f, 1.2f), settings.standDarkColor);
                CreateDecorBox(parent.transform, "South Stand Base", new Vector3(0f, 1f, -settings.HalfFieldLength - 4f), new Vector3(15f, 2f, 1.2f), settings.standDarkColor);
                CreateDecorBox(parent.transform, "North Stand Seats", new Vector3(0f, 2.3f, settings.HalfFieldLength + 4.35f), new Vector3(14.2f, 1.2f, 0.45f), settings.standBlueColor);
                CreateDecorBox(parent.transform, "South Stand Seats", new Vector3(0f, 2.3f, -settings.HalfFieldLength - 4.35f), new Vector3(14.2f, 1.2f, 0.45f), settings.standRedColor);
            }

            if (settings.createAdvertisingBoards)
            {
                CreateDecorBox(parent.transform, "Left Advertising Board", new Vector3(-settings.HalfFieldWidth - 1.15f, 0.45f, 0f), new Vector3(0.18f, 0.9f, settings.fieldLength), settings.adLeftColor);
                CreateDecorBox(parent.transform, "Right Advertising Board", new Vector3(settings.HalfFieldWidth + 1.15f, 0.45f, 0f), new Vector3(0.18f, 0.9f, settings.fieldLength), settings.adRightColor);
            }

            if (settings.createFloodlights)
            {
                CreateDecorBox(parent.transform, "Left Floodlight", new Vector3(-settings.HalfFieldWidth - 2f, 5.8f, -settings.HalfFieldLength - 2f), new Vector3(0.4f, 0.25f, 1.8f), settings.floodlightColor);
                CreateDecorBox(parent.transform, "Right Floodlight", new Vector3(settings.HalfFieldWidth + 2f, 5.8f, settings.HalfFieldLength + 2f), new Vector3(0.4f, 0.25f, 1.8f), settings.floodlightColor);
            }
        }

        private static void CreateCircle(Transform parent, string objectName, Vector3 center, float radius, int segments, Color color)
        {
            for (int i = 0; i < segments; i++)
            {
                float startAngle = (Mathf.PI * 2f / segments) * i;
                float endAngle = (Mathf.PI * 2f / segments) * (i + 1);
                Vector3 start = center + new Vector3(Mathf.Cos(startAngle) * radius, 0.008f, Mathf.Sin(startAngle) * radius);
                Vector3 end = center + new Vector3(Mathf.Cos(endAngle) * radius, 0.008f, Mathf.Sin(endAngle) * radius);
                Vector3 midpoint = (start + end) * 0.5f;
                float length = Vector3.Distance(start, end);

                GameObject segment = CreateLine(parent, $"{objectName} Segment {i + 1}", midpoint, new Vector3(0.08f, 0.012f, length), color);
                segment.transform.rotation = Quaternion.LookRotation(end - start);
            }
        }

        private static GameObject CreateLine(Transform parent, string objectName, Vector3 position, Vector3 scale, Color color)
        {
            GameObject line = CreateBox(parent, objectName, position, scale, color);
            Collider collider = line.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return line;
        }

        private static GameObject CreateBox(Transform parent, string objectName, Vector3 position, Vector3 scale, Color color)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.localScale = scale;
            SetMaterialColor(box, color);
            return box;
        }

        private static GameObject CreateDecorBox(Transform parent, string objectName, Vector3 position, Vector3 scale, Color color)
        {
            GameObject box = CreateBox(parent, objectName, position, scale, color);
            Collider collider = box.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return box;
        }

        private static Text[] CreateHud()
        {
            GameObject canvasObject = new GameObject("HUD Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panelObject = new GameObject("Scoreboard Panel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            Image panel = panelObject.AddComponent<Image>();
            panel.color = new Color(0.03f, 0.05f, 0.07f, 0.72f);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(18f, -18f);
            panelRect.sizeDelta = new Vector2(170f, 78f);

            GameObject textObject = new GameObject("Scoreboard");
            textObject.transform.SetParent(panelObject.transform, false);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = "P1  0 - 0  P2\n60s";

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            GameObject goalObject = new GameObject("Goal Message");
            goalObject.transform.SetParent(canvasObject.transform, false);
            Text goalText = goalObject.AddComponent<Text>();
            goalText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            goalText.fontSize = 46;
            goalText.fontStyle = FontStyle.Bold;
            goalText.alignment = TextAnchor.MiddleCenter;
            goalText.color = new Color(1f, 0.92f, 0.25f);
            goalText.text = string.Empty;
            goalText.enabled = false;

            RectTransform goalRect = goalText.GetComponent<RectTransform>();
            goalRect.anchorMin = new Vector2(0f, 0.62f);
            goalRect.anchorMax = new Vector2(1f, 0.82f);
            goalRect.pivot = new Vector2(0.5f, 0.5f);
            goalRect.offsetMin = Vector2.zero;
            goalRect.offsetMax = Vector2.zero;

            return new[] { text, goalText };
        }

        private static Rigidbody EnsureRigidbody(GameObject target)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = target.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            return rb;
        }

        private static void EnsureCollider(GameObject target)
        {
            if (target.GetComponentInChildren<Collider>() != null)
            {
                return;
            }

            CapsuleCollider collider = target.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.9f, 0f);
            collider.height = 1.8f;
            collider.radius = 0.4f;
        }

        private static void EnsureGroundMarker(GameObject ground)
        {
            if (ground != null && ground.GetComponent<LightningPoly.FootballEssentials3D.Ground>() == null)
            {
                ground.AddComponent<LightningPoly.FootballEssentials3D.Ground>();
            }
        }

        private static void SetMaterialColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }

        private static void SetMaterialColorRecursive(GameObject target, Color color)
        {
            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            {
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = color;
                renderer.sharedMaterial = material;
            }
        }

        private static void SetPhysicsMaterial(GameObject target, float dynamicFriction, float bounciness)
        {
            PhysicsMaterial material = new PhysicsMaterial($"{target.name} Physics")
            {
                dynamicFriction = dynamicFriction,
                staticFriction = dynamicFriction,
                bounciness = bounciness,
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };

            foreach (Collider collider in target.GetComponentsInChildren<Collider>())
            {
                collider.sharedMaterial = material;
            }
        }

        private static void AddPlayAreaLimiter(GameObject target, Vector2 halfSize)
        {
            System.Type limiterType = System.Type.GetType("MiniFootball.PlayAreaLimiter, Assembly-CSharp");
            if (limiterType != null && target.GetComponent(limiterType) == null)
            {
                Component limiter = target.AddComponent(limiterType);
                SetVector2(limiter, "halfSize", halfSize);
            }
        }

        private static void CleanDemoPlayerScript(GameObject character)
        {
            LightningPoly.FootballEssentials3D.Player demoPlayer = character.GetComponent<LightningPoly.FootballEssentials3D.Player>();
            if (demoPlayer != null)
            {
                Object.DestroyImmediate(demoPlayer);
            }
        }

        private static void EnsureBallTag()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == "Ball")
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Ball";
            tagManager.ApplyModifiedProperties();
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetEnum(Object target, string propertyName, GoalSide value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.enumValueIndex = (int)value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetVector3(Object target, string propertyName, Vector3 value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.vector3Value = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetVector2(Object target, string propertyName, Vector2 value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.vector2Value = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.floatValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetControlScheme(SimplePlayerController target, SimplePlayerController.ControlScheme value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty("controlScheme");
            property.enumValueIndex = (int)value;
            serializedObject.ApplyModifiedProperties();
        }

        private static MiniFootballSceneSettings LoadOrCreateSettings()
        {
            MiniFootballSceneSettings settings = AssetDatabase.LoadAssetAtPath<MiniFootballSceneSettings>(SettingsAssetPath);
            if (settings != null)
            {
                return settings;
            }

            settings = ScriptableObject.CreateInstance<MiniFootballSceneSettings>();
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
