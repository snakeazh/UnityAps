using System;
using System.Collections;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Drives the 3D coin flip animation and exposes the settled face.
    /// </summary>
    public sealed class CoinController : MonoBehaviour
    {
        [SerializeField] float flipDuration = 1.35f;
        [SerializeField] float flipHeight = 2.4f;
        [SerializeField] int minSpins = 4;
        [SerializeField] int maxSpins = 7;
        [SerializeField] AnimationCurve heightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] AnimationCurve spinCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        Vector3 _restPosition;
        Quaternion _restRotation;
        bool _isFlipping;

        public bool IsFlipping => _isFlipping;
        public CoinSide CurrentSide { get; private set; } = CoinSide.Heads;

        public event Action<CoinSide> FlipCompleted;

        void Awake()
        {
            _restPosition = transform.localPosition;
            _restRotation = Quaternion.identity;
            ApplySideRotation(CurrentSide);
        }

        public bool TryFlip(CoinSide? forcedSide = null)
        {
            if (_isFlipping)
            {
                return false;
            }

            var result = forcedSide ?? (UnityEngine.Random.value < 0.5f ? CoinSide.Heads : CoinSide.Tails);
            StartCoroutine(FlipRoutine(result));
            return true;
        }

        IEnumerator FlipRoutine(CoinSide result)
        {
            _isFlipping = true;

            var spins = UnityEngine.Random.Range(minSpins, maxSpins + 1);
            // Extra half-turn when landing on tails (coin mesh: heads = 0°, tails = 180° around X).
            var totalDegrees = spins * 360f + (result == CoinSide.Tails ? 180f : 0f);
            var elapsed = 0f;

            while (elapsed < flipDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / flipDuration);
                var heightT = heightCurve.Evaluate(t);
                // Parabola via sin so the coin rises then falls.
                var height = Mathf.Sin(heightT * Mathf.PI) * flipHeight;
                var spinT = spinCurve.Evaluate(t);
                var angle = totalDegrees * spinT;

                transform.localPosition = _restPosition + Vector3.up * height;
                transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                yield return null;
            }

            CurrentSide = result;
            ApplySideRotation(result);
            transform.localPosition = _restPosition;
            _isFlipping = false;
            FlipCompleted?.Invoke(result);
        }

        void ApplySideRotation(CoinSide side)
        {
            transform.localRotation = side == CoinSide.Heads
                ? _restRotation
                : _restRotation * Quaternion.Euler(180f, 0f, 0f);
        }

        public void ResetToHeads()
        {
            StopAllCoroutines();
            _isFlipping = false;
            CurrentSide = CoinSide.Heads;
            transform.localPosition = _restPosition;
            ApplySideRotation(CoinSide.Heads);
        }
    }
}
