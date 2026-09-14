using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CoinFlip
{
    /// <summary>
    /// Diary-style UI feedback: punch-in result, toss hints, 花/字 labels, mute toggle.
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
        [SerializeField] Button muteButton;
        [SerializeField] CoinSparkBurst sparkBurst;

        GameServices _services;
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
            Button mute,
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
            muteButton = mute;
            sparkBurst = sparks;
            WireEvents();
            Refresh();
            RefreshMuteLabel();
            ClearResultVisuals();
        }

        public void BindServices(GameServices services)
        {
            if (_services != null && _services.Settings != null)
            {
                _services.Settings.MutedChanged -= OnMutedChanged;
            }

            _services = services;
            if (_services != null && _services.Settings != null)
            {
                _services.Settings.MutedChanged += OnMutedChanged;
            }

            RefreshMuteLabel();
        }

        void OnEnable()
        {
            WireEvents();
            Refresh();
            RefreshMuteLabel();
        }

        void OnDisable()
        {
            UnwireEvents();
            if (_services != null && _services.Settings != null)
            {
                _services.Settings.MutedChanged -= OnMutedChanged;
            }
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

            if (muteButton != null)
            {
                muteButton.onClick.AddListener(OnMuteClicked);
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

            if (muteButton != null)
            {
                muteButton.onClick.RemoveListener(OnMuteClicked);
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
            _services?.Audio?.PlayUiClick();
            gameManager?.TryFlip();
            Refresh();
        }

        void OnResetClicked()
        {
            _services?.Audio?.PlayUiClick();
            gameManager?.ResetStats();
            ClearResultVisuals();
            if (hintText != null)
            {
                hintText.text = "点一下，碰碰今天的运气";
            }
        }

        void OnMuteClicked()
        {
            _services?.Settings?.ToggleMuted();
            // Click SFX respects the new mute state (silent when muted).
            _services?.Audio?.PlayUiClick();
            RefreshMuteLabel();
        }

        void OnMutedChanged(bool _) => RefreshMuteLabel();

        void RefreshMuteLabel()
        {
            if (muteButton == null)
            {
                return;
            }

            var label = muteButton.GetComponentInChildren<Text>();
            if (label == null)
            {
                return;
            }

            var muted = _services != null && _services.IsMuted;
            label.text = muted ? "静音" : "声音";
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
                var tuning = _services != null ? _services.Tuning : null;
                rewardText.text = side == CoinSide.Heads
                    ? (tuning != null ? tuning.headsRewardText : "+¥1.88")
                    : (tuning != null ? tuning.tailsRewardText : "+¥0.88");
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
