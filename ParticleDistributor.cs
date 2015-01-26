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
        #endregion
        #region Abstract methods
        public abstract void Process(ProcessContext context);
        public virtual void UpdateMaterial(Material material)
        {

        }
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

        public struct ProcessContext
        {
            public Particle particle;
            public GalaxyPrefab galaxy;
            public ParticlesPrefab particles;
            public float time;
            public float index;

            public ProcessContext(Particle particle, GalaxyPrefab galaxy, ParticlesPrefab particles, float time, float index)
            {
                this.particle = particle;
                this.galaxy = galaxy;
                this.particles = particles;
                this.time = time;
                this.index = index;
            }
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
    }
}
