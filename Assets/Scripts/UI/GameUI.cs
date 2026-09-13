using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CoinFlip
{
    /// <summary>
    /// Diary-style UI feedback: punch-in result, toss hints, 花/字 labels.
    /// </summary>
    public sealed class GameUI : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        [SerializeField] Text titleText;
        [SerializeField] Text sloganText;
        [SerializeField] Text hintText;
        [SerializeField] Text resultText;
        [SerializeField] Text rewardText;
        [SerializeField] Text statsText;
        [SerializeField] Button flipButton;
        [SerializeField] Button resetButton;
        [SerializeField] CoinSparkBurst sparkBurst;

        Coroutine _resultPunch;
        Coroutine _rewardFloat;

        public void Bind(
            GameManager manager,
            Text title,
            Text slogan,
            Text hint,
            Text result,
            Text reward,
            Text stats,
            Button flip,
            Button reset,
            CoinSparkBurst sparks)
        {
            gameManager = manager;
            titleText = title;
            sloganText = slogan;
            hintText = hint;
            resultText = result;
            rewardText = reward;
            statsText = stats;
            flipButton = flip;
            resetButton = reset;
            sparkBurst = sparks;
            WireEvents();
            Refresh();
            ClearResultVisuals();
        }

        void OnEnable()
        {
            WireEvents();
            Refresh();
        }

        void OnDisable()
        {
            UnwireEvents();
        }

        void WireEvents()
        {
            UnwireEvents();
            if (gameManager != null)
            {
                gameManager.StatsChanged += Refresh;
                gameManager.FlipStarted += OnFlipStarted;
                gameManager.Landed += OnLanded;
                gameManager.FlipResolved += OnFlipResolved;
            }

            if (flipButton != null)
            {
                flipButton.onClick.AddListener(OnFlipClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetClicked);
            }
        }

        void UnwireEvents()
        {
            if (gameManager != null)
            {
                gameManager.StatsChanged -= Refresh;
                gameManager.FlipStarted -= OnFlipStarted;
                gameManager.Landed -= OnLanded;
                gameManager.FlipResolved -= OnFlipResolved;
            }

            if (flipButton != null)
            {
                flipButton.onClick.RemoveListener(OnFlipClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(OnResetClicked);
            }
        }

        void Update()
        {
            if (gameManager == null)
            {
                return;
            }

            var canPlay = gameManager.CanAcceptGameplayInput;
            if (canPlay && WasPrimaryTap() && !IsPointerOverInteractiveUi())
            {
                gameManager.TryFlip();
                Refresh();
            }

            if (flipButton != null)
            {
                flipButton.interactable = canPlay;
            }

            if (resetButton != null)
            {
                resetButton.interactable = canPlay;
            }
        }

        void OnFlipClicked()
        {
            gameManager?.TryFlip();
            Refresh();
        }

        void OnResetClicked()
        {
            gameManager?.ResetStats();
            ClearResultVisuals();
            if (hintText != null)
            {
                hintText.text = "点一下，碰碰今天的运气";
            }
        }

        void OnFlipStarted()
        {
            ClearResultVisuals();
            if (hintText != null)
            {
                hintText.text = "硬币飞起来了…";
            }
        }

        void OnLanded()
        {
            sparkBurst?.Play();
            if (hintText != null)
            {
                hintText.text = "揭晓！";
            }
        }

        void OnFlipResolved(CoinSide side)
        {
            // Match Chinese 1-yuan coin slang used in casual fortune diaries: 花 / 字.
            var label = side == CoinSide.Heads ? "花" : "字";
            if (resultText != null)
            {
                resultText.text = label;
                if (_resultPunch != null)
                {
                    StopCoroutine(_resultPunch);
                }

                _resultPunch = StartCoroutine(PunchIn(resultText.rectTransform, 0.35f));
            }

            if (rewardText != null)
            {
                var bonus = side == CoinSide.Heads ? "+¥1.88" : "+¥0.88";
                rewardText.text = bonus;
                rewardText.color = side == CoinSide.Heads
                    ? new Color(0.86f, 0.45f, 0.12f, 1f)
                    : new Color(0.55f, 0.42f, 0.28f, 1f);
                if (_rewardFloat != null)
                {
                    StopCoroutine(_rewardFloat);
                }

                _rewardFloat = StartCoroutine(FloatReward(rewardText, 0.7f));
            }

            if (hintText != null)
            {
                hintText.text = side == CoinSide.Heads ? "花面！今日手气不错" : "字面！再抛一次碰运气";
            }

            Refresh();
        }

        void ClearResultVisuals()
        {
            if (resultText != null)
            {
                resultText.text = string.Empty;
                resultText.rectTransform.localScale = Vector3.one;
            }

            if (rewardText != null)
            {
                rewardText.text = string.Empty;
                var c = rewardText.color;
                c.a = 0f;
                rewardText.color = c;
            }
        }

        void Refresh()
        {
            if (titleText != null)
            {
                titleText.text = "我的暴富日记";
            }

            if (sloganText != null)
            {
                sloganText.text = "每天抛抛小硬币，早晚开上法拉利";
            }

            if (statsText != null && gameManager != null)
            {
                statsText.text =
                    $"已抛 {gameManager.TotalFlips} 次  ·  花 {gameManager.HeadsCount}  ·  字 {gameManager.TailsCount}";
            }

            if (hintText != null && gameManager != null && !gameManager.IsBusy &&
                string.IsNullOrEmpty(resultText != null ? resultText.text : null))
            {
                hintText.text = "点一下，碰碰今天的运气";
            }
        }

        static IEnumerator PunchIn(RectTransform target, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                // Overshoot punch: 0 → 1.25 → 1.
                var scale = t < 0.55f
                    ? Mathf.Lerp(0f, 1.28f, t / 0.55f)
                    : Mathf.Lerp(1.28f, 1f, (t - 0.55f) / 0.45f);
                target.localScale = Vector3.one * scale;
                yield return null;
            }

            target.localScale = Vector3.one;
        }

        static IEnumerator FloatReward(Text reward, float duration)
        {
            var rt = reward.rectTransform;
            var start = rt.anchoredPosition;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                rt.anchoredPosition = start + Vector2.up * (48f * t);
                var c = reward.color;
                c.a = t < 0.2f ? t / 0.2f : 1f - (t - 0.2f) / 0.8f;
                reward.color = c;
                yield return null;
            }

            var end = reward.color;
            end.a = 0f;
            reward.color = end;
            rt.anchoredPosition = start;
        }

        static bool WasPrimaryTap()
        {
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }

            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        }

        static bool IsPointerOverInteractiveUi()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null)
            {
                return false;
            }

            if (Input.touchCount > 0)
            {
                return es.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }

            return es.IsPointerOverGameObject();
        }
    }
}
