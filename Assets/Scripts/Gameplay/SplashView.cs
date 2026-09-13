using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CoinFlip
{
    /// <summary>
    /// Full-screen diary cover shown during startup splash.
    /// Built at runtime; fade in/out driven by <see cref="GameFlowController"/>.
    /// </summary>
    public sealed class SplashView : MonoBehaviour
    {
        static readonly Color CoverPaper = new Color(0.96f, 0.91f, 0.78f, 1f);
        static readonly Color CoverInk = new Color(0.32f, 0.2f, 0.12f, 1f);
        static readonly Color CoverGold = new Color(0.86f, 0.58f, 0.14f, 1f);
        static readonly Color CoverMuted = new Color(0.55f, 0.42f, 0.30f, 1f);

        CanvasGroup _group;
        bool _skipRequested;

        public static SplashView Create(Transform parent)
        {
            var root = new GameObject("Splash", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var view = root.AddComponent<SplashView>();
            view.Build();
            return view;
        }

        void Build()
        {
            _group = gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = true;
            _group.interactable = true;

            var bg = CreateUi("Background", transform);
            Stretch(bg);
            bg.AddComponent<Image>().color = CoverPaper;

            var title = CreateText(transform, "Title", "我的暴富日记", 78, FontStyle.Bold, CoverInk);
            SetAnchors(title.gameObject, 0.08f, 0.58f, 0.92f, 0.72f);

            var line = CreateUi("GoldLine", transform);
            SetAnchors(line, 0.28f, 0.55f, 0.72f, 0.555f);
            line.AddComponent<Image>().color = CoverGold;

            var slogan = CreateText(transform, "Slogan", "每天抛抛小硬币\n早晚开上法拉利", 36, FontStyle.Normal, CoverMuted);
            SetAnchors(slogan.gameObject, 0.12f, 0.38f, 0.88f, 0.54f);

            var tip = CreateText(transform, "Tip", "点击屏幕继续", 30, FontStyle.Normal, CoverGold);
            SetAnchors(tip.gameObject, 0.1f, 0.12f, 0.9f, 0.2f);

            // Capture taps on the splash itself.
            var button = gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => _skipRequested = true);
        }

        public IEnumerator Play(float minDuration, bool allowTapSkip)
        {
            _skipRequested = false;
            gameObject.SetActive(true);
            _group.blocksRaycasts = true;
            yield return Fade(0f, 1f, 0.35f);

            var elapsed = 0f;
            while (elapsed < minDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (allowTapSkip && (_skipRequested || WasPrimaryTap()))
                {
                    break;
                }

                yield return null;
            }

            // If skipped early, still wait a beat so the cover doesn't vanish mid-tap.
            if (elapsed < 0.25f)
            {
                yield return new WaitForSecondsRealtime(0.25f - elapsed);
            }
        }

        public IEnumerator Hide(float fadeDuration)
        {
            _group.blocksRaycasts = false;
            yield return Fade(_group.alpha, 0f, fadeDuration);
            gameObject.SetActive(false);
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                _group.alpha = to;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                // Smoothstep for a softer diary-cover feel.
                t = t * t * (3f - 2f * t);
                _group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _group.alpha = to;
        }

        static bool WasPrimaryTap()
        {
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }

            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        }

        static GameObject CreateUi(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Stretch(GameObject go) => SetAnchors(go, 0f, 0f, 1f, 1f);

        static void SetAnchors(GameObject go, float minX, float minY, float maxX, float maxY)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static Text CreateText(Transform parent, string name, string content, int size, FontStyle style, Color color)
        {
            var go = CreateUi(name, parent);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return text;
        }
    }
}
