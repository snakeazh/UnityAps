using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoinFlip
{
    /// <summary>
    /// Runtime scene builder styled after casual "暴富日记" fortune-coin H5 games:
    /// warm paper desk, cartoon gold coin with 花/字 faces, soft UI chrome.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        // Warm diary palette — paper cream, soft coral gold, ink brown.
        static readonly Color Paper = new Color(0.97f, 0.93f, 0.84f, 1f);
        static readonly Color PaperDeep = new Color(0.93f, 0.86f, 0.72f, 1f);
        static readonly Color Desk = new Color(0.78f, 0.62f, 0.42f, 1f);
        static readonly Color Ink = new Color(0.35f, 0.22f, 0.14f, 1f);
        static readonly Color Gold = new Color(0.98f, 0.78f, 0.28f, 1f);
        static readonly Color GoldDeep = new Color(0.86f, 0.58f, 0.12f, 1f);
        static readonly Color Coral = new Color(0.92f, 0.42f, 0.28f, 1f);
        static readonly Color Panel = new Color(1f, 0.98f, 0.93f, 0.92f);
        static readonly Color Muted = new Color(0.55f, 0.42f, 0.30f, 1f);

        void Awake()
        {
            // Ensure a flow controller exists so legacy scenes that only place GameBootstrap still boot.
            if (GetComponent<GameFlowController>() == null)
            {
                gameObject.AddComponent<GameFlowController>();
            }
        }

        /// <summary>
        /// Builds camera / coin / UI / splash. Called by <see cref="GameFlowController"/> during Booting.
        /// </summary>
        public GameBuildContext Build()
        {
            SetupCamera();
            SetupLights();
            var coin = CreateCoin();
            var sparks = gameObject.GetComponent<CoinSparkBurst>() ?? gameObject.AddComponent<CoinSparkBurst>();
            sparks.EnsureBuilt(coin.transform);

            var manager = gameObject.GetComponent<GameManager>() ?? gameObject.AddComponent<GameManager>();
            manager.Bind(coin);

            var canvas = BuildUi(manager, sparks, out var ui, out var splash);
            return new GameBuildContext
            {
                Manager = manager,
                Ui = ui,
                Coin = coin,
                Sparks = sparks,
                Splash = splash,
                RootCanvas = canvas
            };
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Paper;
            cam.transform.position = new Vector3(0f, 0.85f, -4.6f);
            cam.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
            cam.fieldOfView = 48f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 50f;
        }

        void SetupLights()
        {
            var existing = FindObjectOfType<Light>();
            if (existing == null)
            {
                var lightGo = new GameObject("Key Light");
                existing = lightGo.AddComponent<Light>();
            }

            existing.type = LightType.Directional;
            existing.color = new Color(1f, 0.96f, 0.88f);
            existing.intensity = 1.05f;
            existing.shadows = LightShadows.Soft;
            existing.transform.rotation = Quaternion.Euler(48f, -24f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.72f, 0.66f, 0.55f);
        }

        CoinController CreateCoin()
        {
            var root = new GameObject("Coin");
            root.transform.position = new Vector3(0f, 0.2f, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "CoinBody";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(1.55f, 0.055f, 1.55f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Destroy(body.GetComponent<Collider>());
            body.GetComponent<MeshRenderer>().sharedMaterial = CreateCoinMaterial(Gold, 0.55f, 0.78f);

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "Rim";
            rim.transform.SetParent(root.transform, false);
            rim.transform.localScale = new Vector3(1.62f, 0.04f, 1.62f);
            rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Destroy(rim.GetComponent<Collider>());
            rim.GetComponent<MeshRenderer>().sharedMaterial = CreateCoinMaterial(GoldDeep, 0.7f, 0.65f);

            CreateFace(root.transform, "HeadsFace", new Vector3(0f, 0f, -0.06f), Gold, "花");
            CreateFace(root.transform, "TailsFace", new Vector3(0f, 0f, 0.06f), GoldDeep, "字", flipFacing: true);

            var desk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            desk.name = "Desk";
            desk.transform.position = new Vector3(0f, -0.62f, 0.35f);
            desk.transform.localScale = new Vector3(3.6f, 0.03f, 3.6f);
            Destroy(desk.GetComponent<Collider>());
            desk.GetComponent<MeshRenderer>().sharedMaterial = CreateCoinMaterial(Desk, 0f, 0.25f);

            var blotter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blotter.name = "Blotter";
            blotter.transform.position = new Vector3(0f, -0.58f, 0.2f);
            blotter.transform.localScale = new Vector3(2.4f, 0.01f, 2.0f);
            Destroy(blotter.GetComponent<Collider>());
            blotter.GetComponent<MeshRenderer>().sharedMaterial = CreateCoinMaterial(PaperDeep, 0f, 0.15f);

            return root.AddComponent<CoinController>();
        }

        static void CreateFace(Transform parent, string name, Vector3 localPos, Color color, string label, bool flipFacing = false)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            disc.transform.localPosition = localPos;
            disc.transform.localScale = new Vector3(1.38f, 0.012f, 1.38f);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, flipFacing ? 180f : 0f);
            Destroy(disc.GetComponent<Collider>());
            disc.GetComponent<MeshRenderer>().sharedMaterial = CreateCoinMaterial(color, 0.5f, 0.8f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(disc.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            labelGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            labelGo.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);

            var text = labelGo.AddComponent<TextMesh>();
            text.text = label;
            text.fontSize = 90;
            text.characterSize = 0.38f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Ink;
            text.fontStyle = FontStyle.Bold;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        static Material CreateCoinMaterial(Color color, float metallic, float gloss)
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            var mat = new Material(shader);
            if (mat.HasProperty("_Color"))
            {
                mat.color = color;
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", metallic);
            }

            if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", gloss);
            }

            return mat;
        }

        Canvas BuildUi(GameManager manager, CoinSparkBurst sparks, out GameUI ui, out SplashView splash)
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.55f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var safe = CreateUiObject("SafeArea", canvasGo.transform);
            StretchFull(safe);
            ApplySafeArea(safe);

            var top = CreateUiObject("Top", safe);
            SetAnchors(top, 0f, 0.78f, 1f, 1f);

            var title = CreateText(top.transform, "Title", "我的暴富日记", 70, FontStyle.Bold, TextAnchor.MiddleCenter, Ink);
            SetAnchors(title.gameObject, 0.06f, 0.45f, 0.94f, 0.95f);

            var slogan = CreateText(top.transform, "Slogan", "每天抛抛小硬币，早晚开上法拉利", 30, FontStyle.Normal, TextAnchor.MiddleCenter, Muted);
            SetAnchors(slogan.gameObject, 0.08f, 0.08f, 0.92f, 0.45f);

            var mid = CreateUiObject("Mid", safe);
            SetAnchors(mid, 0f, 0.34f, 1f, 0.78f);

            var result = CreateText(mid.transform, "Result", string.Empty, 120, FontStyle.Bold, TextAnchor.MiddleCenter, Coral);
            SetAnchors(result.gameObject, 0.2f, 0.35f, 0.8f, 0.85f);

            var reward = CreateText(mid.transform, "Reward", string.Empty, 42, FontStyle.Bold, TextAnchor.MiddleCenter, GoldDeep);
            SetAnchors(reward.gameObject, 0.25f, 0.15f, 0.75f, 0.4f);

            var bottom = CreateUiObject("Bottom", safe);
            SetAnchors(bottom, 0.06f, 0.03f, 0.94f, 0.32f);
            bottom.AddComponent<Image>().color = Panel;

            var hint = CreateText(bottom.transform, "Hint", "点一下，碰碰今天的运气", 34, FontStyle.Normal, TextAnchor.MiddleCenter, Muted);
            SetAnchors(hint.gameObject, 0.06f, 0.72f, 0.94f, 0.95f);

            var stats = CreateText(bottom.transform, "Stats", "已抛 0 次  ·  花 0  ·  字 0", 28, FontStyle.Normal, TextAnchor.MiddleCenter, Ink);
            SetAnchors(stats.gameObject, 0.06f, 0.48f, 0.94f, 0.72f);

            var flipBtn = CreateButton(bottom.transform, "FlipButton", "抛一次", Coral, 0.08f, 0.08f, 0.58f, 0.42f);
            var resetBtn = CreateButton(bottom.transform, "ResetButton", "清零", new Color(0.72f, 0.62f, 0.5f), 0.62f, 0.08f, 0.92f, 0.42f);

            // Splash sits on top of gameplay UI and is driven by GameFlowController.
            splash = SplashView.Create(canvasGo.transform);
            splash.transform.SetAsLastSibling();

            ui = gameObject.GetComponent<GameUI>() ?? gameObject.AddComponent<GameUI>();
            ui.Bind(manager, title, slogan, hint, result, reward, stats, flipBtn, resetBtn, sparks);
            return canvas;
        }

        static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void StretchFull(GameObject go) => SetAnchors(go, 0f, 0f, 1f, 1f);

        static void SetAnchors(GameObject go, float minX, float minY, float maxX, float maxY)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void ApplySafeArea(GameObject safe)
        {
            var rt = safe.GetComponent<RectTransform>();
            var area = Screen.safeArea;
            var min = area.position;
            var max = area.position + area.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static Text CreateText(Transform parent, string name, string content, int size, FontStyle style, TextAnchor anchor, Color color)
        {
            var go = CreateUiObject(name, parent);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return text;
        }

        static Button CreateButton(Transform parent, string name, string label, Color color, float minX, float minY, float maxX, float maxY)
        {
            var go = CreateUiObject(name, parent);
            SetAnchors(go, minX, minY, maxX, maxY);

            var image = go.AddComponent<Image>();
            image.color = color;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.45f);
            button.colors = colors;

            var text = CreateText(go.transform, "Label", label, 36, FontStyle.Bold, TextAnchor.MiddleCenter, Paper);
            StretchFull(text.gameObject);
            return button;
        }
    }
}
