using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoinFlip
{
    /// <summary>
    /// Builds the playable scene at runtime so the project works without hand-authored scene content.
    /// Attach this to any GameObject in Main.unity (or let the Editor menu create the scene).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        static readonly Color BgDeep = new Color(0.07f, 0.09f, 0.12f, 1f);
        static readonly Color Gold = new Color(0.93f, 0.74f, 0.28f, 1f);
        static readonly Color GoldDark = new Color(0.72f, 0.52f, 0.14f, 1f);
        static readonly Color Cream = new Color(0.96f, 0.93f, 0.86f, 1f);
        static readonly Color Accent = new Color(0.2f, 0.55f, 0.62f, 1f);
        static readonly Color Panel = new Color(0.1f, 0.12f, 0.16f, 0.72f);

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            BuildWorld();
        }

        void BuildWorld()
        {
            SetupCamera();
            SetupLights();
            var coin = CreateCoin();
            var manager = gameObject.GetComponent<GameManager>() ?? gameObject.AddComponent<GameManager>();
            manager.Bind(coin);
            BuildUi(manager);
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
            cam.backgroundColor = BgDeep;
            cam.transform.position = new Vector3(0f, 1.1f, -4.2f);
            cam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 50f;
        }

        void SetupLights()
        {
            if (FindObjectOfType<Light>() == null)
            {
                var lightGo = new GameObject("Key Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.95f, 0.88f);
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                lightGo.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.28f, 0.3f, 0.34f);
        }

        CoinController CreateCoin()
        {
            var root = new GameObject("Coin");
            root.transform.position = new Vector3(0f, 0.35f, 0f);
            root.transform.localScale = Vector3.one;

            // Flattened cylinder: faces point along local ±Y after rotation so X-spin flips faces toward camera.
            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "CoinBody";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(1.35f, 0.06f, 1.35f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Destroy(body.GetComponent<Collider>());

            var bodyMat = CreateLitMaterial(Gold);
            body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

            CreateFaceDisc(root.transform, "HeadsFace", new Vector3(0f, 0f, -0.065f), Gold, "正");
            CreateFaceDisc(root.transform, "TailsFace", new Vector3(0f, 0f, 0.065f), GoldDark, "反", flipFacing: true);

            // Soft ground disc for depth.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.55f, 0.4f);
            ground.transform.localScale = new Vector3(3.2f, 0.02f, 3.2f);
            Destroy(ground.GetComponent<Collider>());
            ground.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial(new Color(0.12f, 0.14f, 0.18f));

            var controller = root.AddComponent<CoinController>();
            return controller;
        }

        static void CreateFaceDisc(Transform parent, string name, Vector3 localPos, Color color, string label, bool flipFacing = false)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            disc.transform.localPosition = localPos;
            disc.transform.localScale = new Vector3(1.2f, 0.01f, 1.2f);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, flipFacing ? 180f : 0f);
            Destroy(disc.GetComponent<Collider>());
            disc.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial(color);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(disc.transform, false);
            // Slightly above disc surface along local up.
            labelGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            labelGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            labelGo.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

            var text = labelGo.AddComponent<TextMesh>();
            text.text = label;
            text.fontSize = 80;
            text.characterSize = 0.35f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Cream;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        static Material CreateLitMaterial(Color color)
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse");
            var mat = new Material(shader);
            if (mat.HasProperty("_Color"))
            {
                mat.color = color;
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0.65f);
            }

            if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", 0.72f);
            }

            return mat;
        }

        void BuildUi(GameManager manager)
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var safe = CreateUiObject("SafeArea", canvasGo.transform);
            StretchFull(safe);
            ApplySafeArea(safe);

            var top = CreateUiObject("TopBar", safe);
            var topRt = top.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 0.86f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.offsetMin = Vector2.zero;
            topRt.offsetMax = Vector2.zero;

            var title = CreateText(top.transform, "Title", "抛硬币", 72, FontStyle.Bold, TextAnchor.MiddleCenter);
            StretchFull(title.gameObject);
            title.color = Cream;

            var bottom = CreateUiObject("BottomPanel", safe);
            var bottomRt = bottom.GetComponent<RectTransform>();
            bottomRt.anchorMin = new Vector2(0.06f, 0.04f);
            bottomRt.anchorMax = new Vector2(0.94f, 0.34f);
            bottomRt.offsetMin = Vector2.zero;
            bottomRt.offsetMax = Vector2.zero;
            var panelImg = bottom.AddComponent<Image>();
            panelImg.color = Panel;

            var result = CreateText(bottom.transform, "Result", string.Empty, 64, FontStyle.Bold, TextAnchor.MiddleCenter);
            var resultRt = result.rectTransform;
            resultRt.anchorMin = new Vector2(0f, 0.55f);
            resultRt.anchorMax = new Vector2(1f, 0.95f);
            resultRt.offsetMin = Vector2.zero;
            resultRt.offsetMax = Vector2.zero;
            result.color = Gold;

            var hint = CreateText(bottom.transform, "Hint", "点击屏幕或按钮抛硬币", 36, FontStyle.Normal, TextAnchor.MiddleCenter);
            var hintRt = hint.rectTransform;
            hintRt.anchorMin = new Vector2(0.05f, 0.38f);
            hintRt.anchorMax = new Vector2(0.95f, 0.55f);
            hintRt.offsetMin = Vector2.zero;
            hintRt.offsetMax = Vector2.zero;
            hint.color = new Color(Cream.r, Cream.g, Cream.b, 0.85f);

            var stats = CreateText(bottom.transform, "Stats", "总计 0  ·  正面 0  ·  反面 0", 30, FontStyle.Normal, TextAnchor.MiddleCenter);
            var statsRt = stats.rectTransform;
            statsRt.anchorMin = new Vector2(0.05f, 0.22f);
            statsRt.anchorMax = new Vector2(0.95f, 0.38f);
            statsRt.offsetMin = Vector2.zero;
            statsRt.offsetMax = Vector2.zero;
            stats.color = new Color(Cream.r, Cream.g, Cream.b, 0.7f);

            var flipBtn = CreateButton(bottom.transform, "FlipButton", "抛一次", Accent, new Vector2(0.08f, 0.04f), new Vector2(0.58f, 0.2f));
            var resetBtn = CreateButton(bottom.transform, "ResetButton", "清零", new Color(0.35f, 0.38f, 0.44f), new Vector2(0.62f, 0.04f), new Vector2(0.92f, 0.2f));

            var ui = gameObject.GetComponent<GameUI>() ?? gameObject.AddComponent<GameUI>();
            ui.Bind(manager, title, hint, result, stats, flipBtn, resetBtn);
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

        static void StretchFull(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
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

        static Text CreateText(Transform parent, string name, string content, int size, FontStyle style, TextAnchor anchor)
        {
            var go = CreateUiObject(name, parent);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = Cream;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return text;
        }

        static Button CreateButton(Transform parent, string name, string label, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = CreateUiObject(name, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = color;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.45f);
            button.colors = colors;

            var text = CreateText(go.transform, "Label", label, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
            StretchFull(text.gameObject);
            text.color = Cream;
            return button;
        }
    }
}
