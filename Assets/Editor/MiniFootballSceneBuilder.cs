namespace MiniFootball.EditorTools
{
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
        private const float FieldWidth = 12f;
        private const float FieldLength = 20f;
        private const float HalfFieldWidth = FieldWidth * 0.5f;
        private const float HalfFieldLength = FieldLength * 0.5f;
        private const float GoalOpeningHalfWidth = 3.2f;

        [MenuItem("MiniFootball/Build First Playable Scene")]
        public static void BuildFirstPlayableScene()
        {
            EnsureBallTag();
            ClearPreviousSetup();

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(FieldWidth, 0.1f, FieldLength);
            EnsureGroundMarker(ground);
            SetMaterialColor(ground, new Color(0.12f, 0.55f, 0.2f));
            CreatePitchLines();
            CreateBoundaryWalls();

            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball";
            ball.transform.position = new Vector3(0f, 0.35f, 0f);
            ball.transform.localScale = Vector3.one * 0.5f;
            ball.tag = "Ball";
            Rigidbody ballRb = EnsureRigidbody(ball);
            ballRb.mass = 0.8f;
            ballRb.linearDamping = 0.9f;
            ballRb.angularDamping = 1.2f;
            ballRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            EnsureCollider(ball);
            SetPhysicsMaterial(ball, 0.6f, 0.05f);
            ball.AddComponent<LightningPoly.FootballEssentials3D.Ball>();
            AddPlayAreaLimiter(ball);
            SetMaterialColor(ball, Color.white);

            GameObject player1 = InstantiatePrefab(CharacterPrefabPath, "Player 1", new Vector3(0f, 1f, -4.5f), Quaternion.identity);
            player1.transform.localScale = Vector3.one * 1.15f;
            CleanDemoPlayerScript(player1);
            EnsureRigidbody(player1).constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            EnsureCollider(player1);
            SimplePlayerController player1Controller = player1.AddComponent<SimplePlayerController>();
            SetControlScheme(player1Controller, SimplePlayerController.ControlScheme.Arrows);
            AddPlayAreaLimiter(player1);
            SetMaterialColorRecursive(player1, new Color(0.1f, 0.35f, 1f));

            GameObject player2 = InstantiatePrefab(CharacterPrefabPath, "Player 2", new Vector3(0f, 1f, 4.5f), Quaternion.Euler(0f, 180f, 0f));
            player2.transform.localScale = Vector3.one * 1.15f;
            CleanDemoPlayerScript(player2);
            EnsureRigidbody(player2).constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            EnsureCollider(player2);
            SimplePlayerController player2Controller = player2.AddComponent<SimplePlayerController>();
            SetControlScheme(player2Controller, SimplePlayerController.ControlScheme.Wasd);
            AddPlayAreaLimiter(player2);
            SetMaterialColorRecursive(player2, new Color(1f, 0.18f, 0.12f));

            GameObject player1Goal = InstantiatePrefab(GoalPrefabPath, "Player 1 Goal", new Vector3(0f, 0f, -HalfFieldLength), Quaternion.identity);
            player1Goal.transform.localScale = new Vector3(2.6f, 2.2f, 2.2f);
            SetMaterialColorRecursive(player1Goal, Color.white);

            GameObject player2Goal = InstantiatePrefab(GoalPrefabPath, "Player 2 Goal", new Vector3(0f, 0f, HalfFieldLength), Quaternion.Euler(0f, 180f, 0f));
            player2Goal.transform.localScale = new Vector3(2.6f, 2.2f, 2.2f);
            SetMaterialColorRecursive(player2Goal, Color.white);

            Transform player1Spawn = CreateSpawn("Player1Spawn", player1.transform.position, player1.transform.rotation);
            Transform player2Spawn = CreateSpawn("Player2Spawn", player2.transform.position, player2.transform.rotation);
            Transform ballSpawn = CreateSpawn("BallSpawn", ball.transform.position, ball.transform.rotation);

            GameObject gameManagerObject = new GameObject("GameManager");
            MiniFootballGameManager gameManager = gameManagerObject.AddComponent<MiniFootballGameManager>();
            SetObjectReference(gameManager, "player1", player1.transform);
            SetObjectReference(gameManager, "player2", player2.transform);
            SetObjectReference(gameManager, "ball", ballRb);
            SetObjectReference(gameManager, "player1Spawn", player1Spawn);
            SetObjectReference(gameManager, "player2Spawn", player2Spawn);
            SetObjectReference(gameManager, "ballSpawn", ballSpawn);
            SetObjectReference(gameManager, "scoreboardText", CreateScoreboard());
            SetFloat(gameManager, "goalLineZ", HalfFieldLength - 1.2f);
            SetFloat(gameManager, "goalHalfWidth", GoalOpeningHalfWidth);

            CreateGoalTrigger("Player 1 Goal Trigger", new Vector3(0f, 1.5f, -HalfFieldLength + 0.35f), GoalSide.Player2, gameManager);
            CreateGoalTrigger("Player 2 Goal Trigger", new Vector3(0f, 1.5f, HalfFieldLength - 0.35f), GoalSide.Player1, gameManager);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = player1.transform.position + new Vector3(0f, 6.5f, -7.5f);
                mainCamera.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
                mainCamera.fieldOfView = 55f;
                SimpleFollowCamera followCamera = mainCamera.GetComponent<SimpleFollowCamera>();
                if (followCamera == null)
                {
                    followCamera = mainCamera.gameObject.AddComponent<SimpleFollowCamera>();
                }

                SetObjectReference(followCamera, "target", player1.transform);
                SetVector3(followCamera, "offset", new Vector3(0f, 6.5f, -7.5f));
            }

            Selection.activeGameObject = player1;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("MiniFootball local 1v1 scene is built and saved. Player 1: arrows + Enter/Right Shift. Player 2: WASD + Space.");
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
                "SM_Stadium"
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

        private static void CreateGoalTrigger(string objectName, Vector3 position, GoalSide scoringSide, MiniFootballGameManager gameManager)
        {
            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.name = objectName;
            trigger.transform.position = position;
            trigger.transform.localScale = new Vector3(GoalOpeningHalfWidth * 2f, 4.2f, 1.2f);

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
        }

        private static void CreatePitchLines()
        {
            GameObject parent = new GameObject("Pitch Lines");
            Color lineColor = new Color(0.94f, 0.94f, 0.88f);

            CreateLine(parent.transform, "Left Touchline", new Vector3(-HalfFieldWidth, 0.006f, 0f), new Vector3(0.12f, 0.012f, FieldLength), lineColor);
            CreateLine(parent.transform, "Right Touchline", new Vector3(HalfFieldWidth, 0.006f, 0f), new Vector3(0.12f, 0.012f, FieldLength), lineColor);
            CreateLine(parent.transform, "Player Goal Line", new Vector3(0f, 0.006f, -HalfFieldLength), new Vector3(FieldWidth, 0.012f, 0.12f), lineColor);
            CreateLine(parent.transform, "Computer Goal Line", new Vector3(0f, 0.006f, HalfFieldLength), new Vector3(FieldWidth, 0.012f, 0.12f), lineColor);
            CreateLine(parent.transform, "Halfway Line", new Vector3(0f, 0.007f, 0f), new Vector3(FieldWidth, 0.012f, 0.1f), lineColor);
            CreateLine(parent.transform, "Player Box", new Vector3(0f, 0.008f, -HalfFieldLength + 2f), new Vector3(5.4f, 0.012f, 0.1f), lineColor);
            CreateLine(parent.transform, "Player Box Left", new Vector3(-2.7f, 0.008f, -HalfFieldLength + 1f), new Vector3(0.1f, 0.012f, 2f), lineColor);
            CreateLine(parent.transform, "Player Box Right", new Vector3(2.7f, 0.008f, -HalfFieldLength + 1f), new Vector3(0.1f, 0.012f, 2f), lineColor);
            CreateLine(parent.transform, "Computer Box", new Vector3(0f, 0.008f, HalfFieldLength - 2f), new Vector3(5.4f, 0.012f, 0.1f), lineColor);
            CreateLine(parent.transform, "Computer Box Left", new Vector3(-2.7f, 0.008f, HalfFieldLength - 1f), new Vector3(0.1f, 0.012f, 2f), lineColor);
            CreateLine(parent.transform, "Computer Box Right", new Vector3(2.7f, 0.008f, HalfFieldLength - 1f), new Vector3(0.1f, 0.012f, 2f), lineColor);
            CreateCircle(parent.transform, "Center Circle", Vector3.zero, 1.8f, 40, lineColor);
        }

        private static void CreateBoundaryWalls()
        {
            GameObject parent = new GameObject("Boundary Walls");
            Color railColor = new Color(0.18f, 0.18f, 0.2f);
            float wallHeight = 1f;
            float wallThickness = 0.35f;
            float endWallLength = HalfFieldWidth - GoalOpeningHalfWidth;
            float endWallCenterX = GoalOpeningHalfWidth + endWallLength * 0.5f;

            CreateBox(parent.transform, "Left Wall", new Vector3(-HalfFieldWidth - wallThickness * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, FieldLength), railColor);
            CreateBox(parent.transform, "Right Wall", new Vector3(HalfFieldWidth + wallThickness * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, FieldLength), railColor);
            CreateBox(parent.transform, "Player End Wall Left", new Vector3(-endWallCenterX, wallHeight * 0.5f, -HalfFieldLength - wallThickness * 0.5f), new Vector3(endWallLength, wallHeight, wallThickness), railColor);
            CreateBox(parent.transform, "Player End Wall Right", new Vector3(endWallCenterX, wallHeight * 0.5f, -HalfFieldLength - wallThickness * 0.5f), new Vector3(endWallLength, wallHeight, wallThickness), railColor);
            CreateBox(parent.transform, "Computer End Wall Left", new Vector3(-endWallCenterX, wallHeight * 0.5f, HalfFieldLength + wallThickness * 0.5f), new Vector3(endWallLength, wallHeight, wallThickness), railColor);
            CreateBox(parent.transform, "Computer End Wall Right", new Vector3(endWallCenterX, wallHeight * 0.5f, HalfFieldLength + wallThickness * 0.5f), new Vector3(endWallLength, wallHeight, wallThickness), railColor);
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

        private static Text CreateScoreboard()
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
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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

            return text;
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

        private static void AddPlayAreaLimiter(GameObject target)
        {
            System.Type limiterType = System.Type.GetType("MiniFootball.PlayAreaLimiter, Assembly-CSharp");
            if (limiterType != null && target.GetComponent(limiterType) == null)
            {
                target.AddComponent(limiterType);
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
    }
}
