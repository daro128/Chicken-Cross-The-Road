using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

namespace ChickenRush.EditorTools
{
    /// <summary>
    /// Generates the whole Chicken Rush project setup: materials, the Main scene (lighting, camera, chicken,
    /// managers, UI), build settings and player settings. Runs automatically the first time the project is
    /// opened (if Main.unity does not exist) and can be re-run from the "Chicken Rush" menu.
    /// Batch mode: Unity -batchmode -quit -projectPath . -executeMethod ChickenRush.EditorTools.ChickenRushSceneBuilder.BuildSceneBatch
    /// </summary>
    public static class ChickenRushSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string MaterialDir = "Assets/Materials";
        private const string AutoKey = "ChickenRush.AutoBuildTried";

        // ---------- Menu & automation ----------

        [InitializeOnLoadMethod]
        private static void AutoBuildOnFirstOpen()
        {
            if (File.Exists(ScenePath)) return;
            if (SessionState.GetBool(AutoKey, false)) return;
            SessionState.SetBool(AutoKey, true);

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
                if (File.Exists(ScenePath)) return;
                if (SceneManager.GetActiveScene().isDirty)
                {
                    Debug.LogWarning("Chicken Rush: save your open scene, then run Chicken Rush > 1. Generate Game Scene.");
                    return;
                }
                BuildScene();
            };
        }

        [MenuItem("Chicken Rush/1. Generate Game Scene")]
        public static void BuildSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildScene();
        }

        [MenuItem("Chicken Rush/2. Build Windows 64-bit")]
        public static void BuildWindows()
        {
            BuildPlayerFor(BuildTarget.StandaloneWindows64, "Builds/Windows/ChickenRush.exe");
        }

        [MenuItem("Chicken Rush/3. Build macOS")]
        public static void BuildMac()
        {
            BuildPlayerFor(BuildTarget.StandaloneOSX, "Builds/macOS/ChickenRush.app");
        }

        public static void BuildSceneBatch()
        {
            BuildScene();
        }

        // ---------- Scene generation ----------

        public static void BuildScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder(MaterialDir);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureLighting();
            GamePalette palette = BuildPalette(out GameObject managersRoot);

            // Managers
            ScoreManager score = CreateChild<ScoreManager>(managersRoot.transform, "ScoreManager");
            TrafficSpawner traffic = CreateChild<TrafficSpawner>(managersRoot.transform, "TrafficSpawner");
            EnvironmentGenerator env = CreateChild<EnvironmentGenerator>(managersRoot.transform, "EnvironmentGenerator");
            GameManager gm = CreateChild<GameManager>(managersRoot.transform, "GameManager");

            Ref(traffic, "palette", palette);
            Ref(env, "palette", palette);
            Ref(env, "traffic", traffic);
            Ref(gm, "scoreManager", score);
            Ref(gm, "environment", env);
            Ref(gm, "traffic", traffic);

            // Chicken
            PlayerController player = BuildPlayer(palette);
            Ref(player, "gameManager", gm);
            Ref(player, "scoreManager", score);
            Ref(player, "environment", env);

            // Camera
            CameraFollow follow = BuildCamera();
            Ref(follow, "target", player.transform);

            // UI
            BuildUI(gm, score);

            EditorSceneManager.SaveScene(scene, ScenePath);
            ConfigureProject();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Chicken Rush: scene generated at " + ScenePath + ". Press Play to test.");
        }

        private static void ConfigureLighting()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.72f, 0.76f, 0.85f);
        }

        private static CameraFollow BuildCamera()
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            Camera cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.80f, 0.96f);
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 250f;
            go.AddComponent<AudioListener>();
            go.transform.position = new Vector3(0f, 10f, -9f);
            go.transform.rotation = Quaternion.Euler(48f, -18f, 0f);
            return go.AddComponent<CameraFollow>();
        }

        private static PlayerController BuildPlayer(GamePalette palette)
        {
            GameObject go = new GameObject("Chicken");
            go.tag = "Player";

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            BoxCollider box = go.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.4f, 0f);
            box.size = new Vector3(0.55f, 0.7f, 0.55f);

            Transform visual = new GameObject("Visual").transform;
            visual.SetParent(go.transform, false);
            PrimitiveFactory.BuildChicken(visual, palette);

            PlayerController pc = go.AddComponent<PlayerController>();
            Ref(pc, "visual", visual);
            return pc;
        }

        // ---------- Materials / palette ----------

        private static GamePalette BuildPalette(out GameObject managersRoot)
        {
            managersRoot = new GameObject("Managers");
            GamePalette p = CreateChild<GamePalette>(managersRoot.transform, "GamePalette");

            Ref(p, "grassLight", Mat("Grass_Light", Hex("7ED957")));
            Ref(p, "grassDark", Mat("Grass_Dark", Hex("6CC84A")));
            Ref(p, "road", Mat("Road", Hex("4A4E57")));
            Ref(p, "marking", Mat("Marking_White", Hex("FFFFFF")));
            Ref(p, "trunk", Mat("Tree_Trunk", Hex("8B5A2B")));
            Ref(p, "leaves", Mat("Tree_Leaves", Hex("2FA84F")));
            Ref(p, "leavesDark", Mat("Tree_LeavesDark", Hex("238C3F")));
            Ref(p, "flowerPink", Mat("Flower_Pink", Hex("FF7FB0")));
            Ref(p, "flowerYellow", Mat("Flower_Yellow", Hex("FFE066")));
            Ref(p, "finishTile", Mat("Finish_Gold", Hex("F7C948")));
            Ref(p, "finishPole", Mat("Finish_Pole", Hex("F2F2F2")));

            Ref(p, "chickenWhite", Mat("Chicken_White", Hex("FFFFFF")));
            Ref(p, "chickenWing", Mat("Chicken_Wing", Hex("EDEDED")));
            Ref(p, "beak", Mat("Chicken_Beak", Hex("FF9F1C")));
            Ref(p, "comb", Mat("Chicken_Comb", Hex("E63946")));
            Ref(p, "eye", Mat("Chicken_Eye", Hex("1B1B1B")));

            Ref(p, "carWindow", Mat("Car_Window", Hex("A8DADC")));
            Ref(p, "carWheel", Mat("Car_Wheel", Hex("222222")));
            Ref(p, "carLight", Mat("Car_Light", Hex("FFF3B0")));

            string[] carColors = { "E63946", "457B9D", "F4A261", "9B5DE5", "00BBF9", "FFD23F" };
            SerializedObject so = new SerializedObject(p);
            SerializedProperty arr = so.FindProperty("carBodies");
            arr.arraySize = carColors.Length;
            for (int i = 0; i < carColors.Length; i++)
            {
                arr.GetArrayElementAtIndex(i).objectReferenceValue = Mat("Car_Body_" + i, Hex(carColors[i]));
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return p;
        }

        private static Material Mat(string name, Color color)
        {
            string path = MaterialDir + "/" + name + ".mat";
            Shader shader = FindShader();
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            else
            {
                m.shader = shader;
            }

            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.1f);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Shader FindShader()
        {
            // Pick the shader that matches the project's render pipeline (URP/HDRP default material, or Standard).
            RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
            if (rp != null && rp.defaultMaterial != null && rp.defaultMaterial.shader != null)
            {
                return rp.defaultMaterial.shader;
            }
            Shader s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Sprites/Default");
            return s;
        }

        // ---------- UI ----------

        private static void BuildUI(GameManager gm, ScoreManager score)
        {
            Font font = GetFont();

            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            UIManager ui = canvasGo.AddComponent<UIManager>();

            // EventSystem (needed for buttons)
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif

            Color white = Color.white;
            Color gold = Hex("FFD23F");

            // ---- Menu ----
            RectTransform menu = Panel(canvasGo.transform, "MenuPanel", new Color(0.05f, 0.1f, 0.2f, 0.45f));
            Label(menu, font, "Title", "CHICKEN RUSH", 140, gold, new Vector2(0, 280), new Vector2(1500, 180));
            Label(menu, font, "Subtitle", "Cross the roads. Dodge the cars. Reach the finish line!", 44, white, new Vector2(0, 150), new Vector2(1500, 80));
            Button start = MakeButton(menu, font, "StartButton", "START GAME", new Vector2(0, 20), new Vector2(480, 120), Hex("2FA84F"));
            Label(menu, font, "Controls",
                "CONTROLS\nW / Up Arrow: forward    S / Down Arrow: back\nA / Left Arrow: left    D / Right Arrow: right\nSpace: hop in place    Enter: start    R: restart",
                34, white, new Vector2(0, -190), new Vector2(1500, 220));
            Button quit = MakeButton(menu, font, "QuitButton", "QUIT", new Vector2(0, -390), new Vector2(280, 80), Hex("6C757D"));

            // ---- HUD ----
            RectTransform hud = Container(canvasGo.transform, "HudPanel");
            Text scoreText = Label(hud, font, "ScoreText", "Score: 0", 60, white, new Vector2(40, -30), new Vector2(600, 90), new Vector2(0, 1), TextAnchor.UpperLeft);
            Text bestText = Label(hud, font, "BestText", "Best: 0", 44, gold, new Vector2(-40, -30), new Vector2(600, 90), new Vector2(1, 1), TextAnchor.UpperRight);
            Text targetText = Label(hud, font, "TargetText", "Goal: reach row 30", 40, white, new Vector2(0, -30), new Vector2(800, 80), new Vector2(0.5f, 1), TextAnchor.UpperCenter);
            Label(hud, font, "Hint", "WASD / Arrow keys: move    Space: hop", 32, white, new Vector2(0, 30), new Vector2(1200, 60), new Vector2(0.5f, 0), TextAnchor.LowerCenter);

            // ---- Game Over ----
            RectTransform over = Panel(canvasGo.transform, "GameOverPanel", new Color(0.45f, 0.05f, 0.05f, 0.6f));
            Label(over, font, "Title", "GAME OVER", 130, white, new Vector2(0, 220), new Vector2(1500, 170));
            Label(over, font, "Subtitle", "The chicken got hit by a car!", 46, white, new Vector2(0, 110), new Vector2(1500, 70));
            Text overScore = Label(over, font, "Score", "Score: 0", 60, gold, new Vector2(0, 20), new Vector2(1000, 90));
            Text overBest = Label(over, font, "Best", "Best: 0", 46, white, new Vector2(0, -60), new Vector2(1000, 70));
            Button overRestart = MakeButton(over, font, "RestartButton", "RESTART", new Vector2(0, -190), new Vector2(480, 120), Hex("E63946"));
            Label(over, font, "Hint", "or press R / Enter", 32, white, new Vector2(0, -290), new Vector2(800, 60));

            // ---- Victory ----
            RectTransform win = Panel(canvasGo.transform, "VictoryPanel", new Color(0.05f, 0.35f, 0.15f, 0.6f));
            Label(win, font, "Title", "VICTORY!", 140, gold, new Vector2(0, 220), new Vector2(1500, 180));
            Label(win, font, "Subtitle", "The chicken crossed to the other side!", 46, white, new Vector2(0, 110), new Vector2(1500, 70));
            Text winScore = Label(win, font, "Score", "Score: 0", 60, gold, new Vector2(0, 20), new Vector2(1000, 90));
            Text winBest = Label(win, font, "Best", "Best: 0", 46, white, new Vector2(0, -60), new Vector2(1000, 70));
            Button winRestart = MakeButton(win, font, "RestartButton", "PLAY AGAIN", new Vector2(0, -190), new Vector2(480, 120), Hex("2FA84F"));
            Label(win, font, "Hint", "or press R / Enter", 32, white, new Vector2(0, -290), new Vector2(800, 60));

            // Wire UIManager
            Ref(ui, "gameManager", gm);
            Ref(ui, "scoreManager", score);
            Ref(ui, "menuPanel", menu.gameObject);
            Ref(ui, "hudPanel", hud.gameObject);
            Ref(ui, "gameOverPanel", over.gameObject);
            Ref(ui, "victoryPanel", win.gameObject);
            Ref(ui, "scoreText", scoreText);
            Ref(ui, "bestText", bestText);
            Ref(ui, "targetText", targetText);
            Ref(ui, "gameOverScoreText", overScore);
            Ref(ui, "gameOverBestText", overBest);
            Ref(ui, "victoryScoreText", winScore);
            Ref(ui, "victoryBestText", winBest);
            Ref(ui, "startButton", start);
            Ref(ui, "quitButton", quit);
            Ref(ui, "gameOverRestartButton", overRestart);
            Ref(ui, "victoryRestartButton", winRestart);

            // Only the menu is visible at the start; UIManager toggles the rest at runtime.
            hud.gameObject.SetActive(false);
            over.gameObject.SetActive(false);
            win.gameObject.SetActive(false);
        }

        private static Font GetFont()
        {
#if UNITY_2022_2_OR_NEWER
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform Container(Transform parent, string name)
        {
            RectTransform rt = NewRect(name, parent);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private static RectTransform Panel(Transform parent, string name, Color color)
        {
            RectTransform rt = Container(parent, name);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return rt;
        }

        private static Text Label(Transform parent, Font font, string name, string content, int size, Color color,
            Vector2 position, Vector2 sizeDelta)
        {
            return Label(parent, font, name, content, size, color, position, sizeDelta, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
        }

        private static Text Label(Transform parent, Font font, string name, string content, int size, Color color,
            Vector2 position, Vector2 sizeDelta, Vector2 anchor, TextAnchor align)
        {
            RectTransform rt = NewRect(name, parent);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = sizeDelta;

            Text t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.text = content;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            Shadow shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(3f, -3f);
            return t;
        }

        private static Button MakeButton(Transform parent, Font font, string name, string label, Vector2 position, Vector2 size, Color color)
        {
            RectTransform rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            Button b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;

            Text t = Label(rt, font, "Label", label, Mathf.RoundToInt(size.y * 0.42f), Color.white, Vector2.zero, size);
            return b;
        }

        // ---------- Project settings / build ----------

        private static void ConfigureProject()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            PlayerSettings.productName = "Chicken Rush";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
        }

        private static void BuildPlayerFor(BuildTarget target, string outputPath)
        {
            if (!File.Exists(ScenePath)) BuildScene();
            ConfigureProject();

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Chicken Rush build succeeded: " + outputPath + " (" + report.summary.totalSize / (1024 * 1024) + " MB)");
            }
            else
            {
                Debug.LogError("Chicken Rush build failed: " + report.summary.result +
                    ". Make sure the " + target + " build support module is installed in Unity Hub.");
            }
        }

        // ---------- Helpers ----------

        private static T CreateChild<T>(Transform parent, string name) where T : Component
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        private static void Ref(Object target, string property, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(property);
            if (prop == null)
            {
                Debug.LogError("Chicken Rush builder: field '" + property + "' not found on " + target.GetType().Name);
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Color Hex(string hex)
        {
            Color c;
            return ColorUtility.TryParseHtmlString("#" + hex, out c) ? c : Color.magenta;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
