using UnityEngine;
using UnityEngine.UI;

namespace CoinFlip
{
    /// <summary>
    /// Wires UI controls to <see cref="GameManager"/> and refreshes labels.
    /// </summary>
    public sealed class GameUI : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        [SerializeField] Text titleText;
        [SerializeField] Text hintText;
        [SerializeField] Text resultText;
        [SerializeField] Text statsText;
        [SerializeField] Button flipButton;
        [SerializeField] Button resetButton;

        public void Bind(
            GameManager manager,
            Text title,
            Text hint,
            Text result,
            Text stats,
            Button flip,
            Button reset)
        {
            gameManager = manager;
            titleText = title;
            hintText = hint;
            resultText = result;
            statsText = stats;
            flipButton = flip;
            resetButton = reset;
            WireEvents();
            Refresh();
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

            // Tap / click anywhere (except while busy) also flips — friendly for mobile.
            if (!gameManager.IsBusy && WasPrimaryTap())
            {
                // Ignore taps that hit UI buttons; those go through Button.onClick.
                if (!IsPointerOverInteractiveUi())
                {
                    gameManager.TryFlip();
                    Refresh();
                }
            }

            if (flipButton != null)
            {
                flipButton.interactable = !gameManager.IsBusy;
            }
        }

        void OnFlipClicked()
        {
            if (gameManager != null && gameManager.TryFlip())
            {
                if (hintText != null)
                {
                    hintText.text = "翻转中…";
                }

                if (resultText != null)
                {
                    resultText.text = string.Empty;
                }
            }

            Refresh();
        }

        void OnResetClicked()
        {
            gameManager?.ResetStats();
            if (resultText != null)
            {
                resultText.text = string.Empty;
            }

            if (hintText != null)
            {
                hintText.text = "点击屏幕或按钮抛硬币";
            }
        }

        void OnFlipResolved(CoinSide side)
        {
            if (resultText != null)
            {
                resultText.text = side == CoinSide.Heads ? "正面" : "反面";
            }

            if (hintText != null)
            {
                hintText.text = "再点一次继续";
            }

            Refresh();
        }

        void Refresh()
        {
            if (titleText != null)
            {
                titleText.text = "抛硬币";
            }

            if (statsText != null && gameManager != null)
            {
                statsText.text =
                    $"总计 {gameManager.TotalFlips}  ·  正面 {gameManager.HeadsCount}  ·  反面 {gameManager.TailsCount}";
            }

            if (hintText != null && gameManager != null && !gameManager.IsBusy &&
                string.IsNullOrEmpty(resultText != null ? resultText.text : null))
            {
                hintText.text = "点击屏幕或按钮抛硬币";
            }
        }

        static bool WasPrimaryTap()
        {
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                return true;
            }

            return false;
        }

        static bool IsPointerOverInteractiveUi()
        {
            // Lightweight check: if EventSystem reports UI under pointer, skip world tap flip.
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
