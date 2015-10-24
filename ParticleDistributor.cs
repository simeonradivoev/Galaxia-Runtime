using UnityEngine;

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
        /// <summary>
        /// Used by the Particle Generator to modify/distribute the particles to a desired shape.
        /// This is where particles are procceded one by one.
        /// <see cref="Galaxia.ParticleDistributor.ProcessContext"/>
        /// </summary>
        /// <param name="context">The context holds information on the current particle and Galaxy Object.</param>
        public abstract void Process(ProcessContext context);
        protected virtual void ProcessProperties(ProcessContext context,Vector3 pos,float angle)
        {
            context.particle.color = context.particles.GetColor(pos, pos.magnitude, context.galaxy.Size, angle, context.particle.index);
            context.particle.size = context.particles.GetSize(pos, pos.magnitude, context.galaxy.Size, angle, context.particle.index);
            context.particle.rotation = context.particles.GetRotation(pos, pos.magnitude, context.galaxy.Size, angle, context.particle.index);
        }
        public virtual void UpdateMaterial(Material material)
        {

        }
        #endregion
        #region Virtual Methods
        public virtual void RecreateCurves() { }
        #endregion
        #region static methods
        /// <summary>
        /// Used to calculated the Integral Curve of the given Animation curve.
        /// Used by the distribution function for <see cref="Galaxia.Image"/>
        /// </summary>
        /// <param name="curve">The animation curve.</param>
        /// <param name="steps">The resolution/quality of the integral sampling. A grater value means more detail but slower speed.</param>
        /// <returns>The integrated Curve.</returns>
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

            //Debug.Log(normalizer);
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

        /// <summary>
        /// Used to inverse an Animation curve.
        /// This swaps the time and value of all the keys in the animation curve.
        /// </summary>
        /// <param name="curve">The animation curve to invert.</param>
        /// <returns>The inverted version of the given animation curve.</returns>
        public static AnimationCurve Inverse(AnimationCurve curve)
        {
            AnimationCurve inverse = new AnimationCurve();
            //Debug.Log(curve.keys.Length);
            for (int i = 0; i < curve.keys.Length; i++)
            {
                inverse.AddKey(curve.keys[i].value, curve.keys[i].time);
            }
            return inverse;
        }
        #endregion
    }
}
