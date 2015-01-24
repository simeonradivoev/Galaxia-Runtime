using UnityEngine;
using System.Collections.Generic;

namespace Galaxia
{
    public abstract class ParticleDistributor : ScriptableObject
    {
        #region constants
        public const float G = 6.67384f;
        #endregion
        #region Private
        [SerializeField]
        [CurveRange(0, 0, 1, 1)]
        private AnimationCurve m_galaxyHeightDistribution = DefaultResources.HeightCurve;
        [SerializeField]
        [CurveRange(0, 0, 1, 1)]
        private AnimationCurve m_galaxyHeightMultiply = DefaultResources.HeightCurve;
        #endregion
        #region Abstract methods
        public abstract void Process(Particle particle, GalaxyPrefab galaxy, ParticlesPrefab particles, float time, float index);
        public abstract void UpdateMaterial(Material material);
        #endregion
        #region Virtual Methods
        public virtual void RecreateCurves() { }
        #endregion
        #region static methods
        public static AnimationCurve Integral(AnimationCurve curve, int steps)
        {
            Keyframe[] frames = new Keyframe[steps];

            AnimationCurve integral = new AnimationCurve();
            float stepSize = 1.0f / (float)steps;
            float area = 0;
            float maxArea = 0;

            for (int i = 1; i < steps; i++)
            {
                float pos = stepSize * i;
                area += curve.Evaluate(pos) * stepSize;

                if (maxArea < area)
                    maxArea = area;

                frames[i] = new Keyframe(pos, area);
            }

            float normalizer = 1;

            if (maxArea > 0)
                normalizer = 1.0f / maxArea;


            for (int i = 0; i < steps; i++)
            {
                integral.AddKey(frames[i].time, frames[i].value * normalizer);
            }

            Debug.Log(normalizer);
            return integral;
        }

        public static AnimationCurve Inverse(AnimationCurve curve)
        {
            AnimationCurve inverse = new AnimationCurve();
            Debug.Log(curve.keys.Length);
            for (int i = 0; i < curve.keys.Length; i++)
            {
                inverse.AddKey(curve.keys[i].value, curve.keys[i].time);
            }
            return inverse;
        }
        #endregion
        #region Getters and setters
        public AnimationCurve GalaxyHeightDistribution { get { return m_galaxyHeightDistribution; } set { m_galaxyHeightDistribution = value; RecreateCurves(); } }
        public AnimationCurve GalaxyHeightMultiply { get { return m_galaxyHeightMultiply; } set { m_galaxyHeightMultiply = value; RecreateCurves(); } }
        #endregion
    }
}
