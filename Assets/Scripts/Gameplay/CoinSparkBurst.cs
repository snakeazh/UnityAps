using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Lightweight gold spark burst played when the coin lands (暴富日记 landing pop).
    /// </summary>
    public sealed class CoinSparkBurst : MonoBehaviour
    {
        ParticleSystem _ps;
        Transform _follow;

        public void EnsureBuilt(Transform coin)
        {
            _follow = coin;
            if (_ps != null)
            {
                return;
            }

            var go = new GameObject("LandSparks");
            go.transform.SetParent(transform, false);
            if (coin != null)
            {
                go.transform.position = coin.position;
            }

            _ps = go.AddComponent<ParticleSystem>();
            var main = _ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.35f;
            main.startLifetime = 0.45f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.11f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.92f, 0.45f, 1f),
                new Color(1f, 0.72f, 0.2f, 1f));
            main.gravityModifier = 0.85f;
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = _ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });

            var shape = _ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;

            var colorOverLifetime = _ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                    new GradientColorKey(new Color(1f, 0.6f, 0.15f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            renderer.material = new Material(shader);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Follow(Transform coin)
        {
            _follow = coin;
            if (_ps != null && coin != null)
            {
                _ps.transform.position = coin.position;
            }
        }

        public void Play()
        {
            if (_ps == null)
            {
                return;
            }

            if (_follow != null)
            {
                _ps.transform.position = _follow.position;
            }

            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _ps.Play();
        }
    }
}
