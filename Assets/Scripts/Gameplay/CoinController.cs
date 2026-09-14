using System;
using System.Collections;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Coin toss aligned with casual "暴富日记" H5 feel:
    /// anticipation dip → high arc spin → elastic bounce land → settle wobble.
    /// </summary>
    public sealed class CoinController : MonoBehaviour
    {
        [SerializeField] float anticipation = 0.12f;
        [SerializeField] float flightDuration = 1.05f;
        [SerializeField] float bounceDuration = 0.55f;
        [SerializeField] float settleDuration = 0.22f;
        [SerializeField] float flipHeight = 3.1f;
        [SerializeField] int minSpins = 5;
        [SerializeField] int maxSpins = 8;
        [SerializeField] float landSquash = 0.18f;
        [SerializeField] float bounceHeight = 0.55f;

        AudioService _audio;
        Vector3 _restPosition;
        Vector3 _restScale;
        Quaternion _restRotation;
        bool _isFlipping;

        public bool IsFlipping => _isFlipping;
        public CoinSide CurrentSide { get; private set; } = CoinSide.Heads;

        public event Action FlipStarted;
        public event Action<CoinSide> FlipCompleted;
        public event Action Landed;

        void Awake()
        {
            _restPosition = transform.localPosition;
            _restScale = transform.localScale;
            _restRotation = Quaternion.identity;
            ApplySideRotation(CurrentSide);
        }

        public void ApplyTuning(GameTuning tuning)
        {
            if (tuning == null)
            {
                return;
            }

            anticipation = tuning.anticipation;
            flightDuration = tuning.flightDuration;
            bounceDuration = tuning.bounceDuration;
            settleDuration = tuning.settleDuration;
            flipHeight = tuning.flipHeight;
            minSpins = tuning.minSpins;
            maxSpins = tuning.maxSpins;
            landSquash = tuning.landSquash;
            bounceHeight = tuning.bounceHeight;
        }

        public void BindAudio(AudioService audio) => _audio = audio;

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
            FlipStarted?.Invoke();
            _audio?.PlayToss();

            var spins = UnityEngine.Random.Range(minSpins, maxSpins + 1);
            var totalDegrees = spins * 360f + (result == CoinSide.Tails ? 180f : 0f);

            yield return Animate(anticipation, t =>
            {
                var ease = EaseOutQuad(t);
                transform.localPosition = _restPosition + Vector3.down * (0.12f * ease);
                transform.localScale = SquashScale(1f + landSquash * 0.6f * ease, 1f - landSquash * ease);
            });

            var flightElapsed = 0f;
            while (flightElapsed < flightDuration)
            {
                flightElapsed += Time.deltaTime;
                var t = Mathf.Clamp01(flightElapsed / flightDuration);
                var height = Mathf.Sin(EaseOutCubic(t) * Mathf.PI) * flipHeight;
                var zDrift = Mathf.Sin(t * Mathf.PI) * 0.18f;
                var angle = totalDegrees * EaseOutCubic(t);

                transform.localPosition = _restPosition + new Vector3(0f, height, zDrift);
                transform.localRotation = Quaternion.Euler(angle, Mathf.Sin(t * Mathf.PI * 2f) * 8f, 0f);
                var stretch = 1f + Mathf.Sin(t * Mathf.PI) * 0.12f;
                transform.localScale = SquashScale(1f / stretch, stretch);
                yield return null;
            }

            CurrentSide = result;
            ApplySideRotation(result);
            Landed?.Invoke();
            _audio?.PlayLand();

            yield return BounceOnce(bounceHeight, bounceDuration * 0.55f, totalDegrees);
            yield return BounceOnce(bounceHeight * 0.35f, bounceDuration * 0.45f, totalDegrees);

            yield return Animate(settleDuration, t =>
            {
                var damp = 1f - EaseOutQuad(t);
                var wobble = Mathf.Sin(t * Mathf.PI * 3f) * 6f * damp;
                ApplySideRotation(result);
                transform.localRotation *= Quaternion.Euler(0f, 0f, wobble);
                transform.localPosition = _restPosition;
                transform.localScale = Vector3.Lerp(SquashScale(1.06f, 0.94f), _restScale, EaseOutQuad(t));
            });

            ApplySideRotation(result);
            transform.localPosition = _restPosition;
            transform.localScale = _restScale;
            _isFlipping = false;
            FlipCompleted?.Invoke(result);
        }

        IEnumerator BounceOnce(float height, float duration, float finalAngle)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var h = Mathf.Sin(t * Mathf.PI) * height;
                var contact = 1f - Mathf.Sin(t * Mathf.PI);
                transform.localPosition = _restPosition + Vector3.up * h;
                transform.localRotation = Quaternion.Euler(finalAngle, 0f, 0f);
                transform.localScale = SquashScale(1f + landSquash * contact, 1f - landSquash * contact);
                yield return null;
            }
        }

        static IEnumerator Animate(float duration, Action<float> onStep)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                onStep(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            onStep(1f);
        }

        Vector3 SquashScale(float xz, float y) =>
            new Vector3(_restScale.x * xz, _restScale.y * y, _restScale.z * xz);

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
            transform.localScale = _restScale;
            ApplySideRotation(CoinSide.Heads);
        }

        static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        static float EaseOutCubic(float t)
        {
            var u = 1f - t;
            return 1f - u * u * u;
        }
    }
}
