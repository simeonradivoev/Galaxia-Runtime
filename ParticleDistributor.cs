// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using UnityEngine;

namespace Galaxia
{
    /// <summary>
    /// The Particle Distributor is the base class for all Particle Distributors.
    /// As it's name suggests, it is used to control the distribution of generated particles.
    /// </summary>
    public abstract class ParticleDistributor : ScriptableObject
    {
        #region constants
        /// <summary>
        /// The Gravitational constant
        /// </summary>
        public const float G = 6.67384f;
        #endregion
        #region Private
		private GalaxyPrefab m_galaxyPrefab;
        #endregion
        #region Abstract methods
        /// <summary>
        /// Used by the Particle Generator to modify/distribute the particles to a desired shape.
        /// This is where particles are processed one by one.
        /// <see cref="Galaxia.ParticleDistributor.ProcessContext"/>
        /// </summary>
        /// <param name="context">The context holds information on the current particle and Galaxy Object.</param>
        public abstract void Process(ProcessContext context);
        /// <summary>
        /// Used to process any additional properties dependent on the position and angle of a particle
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pos"></param>
        /// <param name="angle"></param>
        protected virtual void ProcessProperties(ProcessContext context,Vector3 pos,float angle)
        {
            context.particle.color = context.particles.GetColor(pos, pos.magnitude, context.galaxy.Size, angle, context.particle.index);
            context.particle.size = context.particles.GetSize(pos, pos.magnitude, context.galaxy.Size, angle, context.particle.index);
            context.particle.rotation = context.particles.GetRotation(pos, pos.magnitude, context.galaxy.Size, angle, context.particle.index);
        }

	    /// <summary>
	    /// Updates all uniform variables in the given material
	    /// </summary>
	    /// <param name="material">the given material</param>
	    public virtual void UpdateMaterial(Material material) { }
        #endregion
        #region Virtual Methods
        /// <summary>
        /// Used for recreating any predefined curves.
        /// </summary>
        public virtual void RecreateCurves() { }
        #endregion
        #region static methods
        /// <summary>
        /// Used to calculated the Integral Curve of the given Animation curve.
        /// Used by the distribution function for <see cref="Galaxia.ImageDistributor"/>
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

            return integral;
        }
        /// <summary>
        /// The context that holds all the data needed for particle distribution by any Particle Distributor.
        /// This is used for customizing and pre processing particle properties for custom distributions.
        /// Used in <see cref="ParticleDistributor.Process(ProcessContext)"/> and <see cref="ParticleDistributor.ProcessProperties(ProcessContext, Vector3, float)"/>
        /// </summary>
        public struct ProcessContext
        {
            /// <summary>
            /// The Context's particle.
            /// </summary>
            public Particle particle;
            /// <summary>
            /// The Galaxy Prefab.
            /// </summary>
            public GalaxyPrefab galaxy;
            /// <summary>
            /// The Particles Prefab.
            /// </summary>
            public ParticlesPrefab particles;
            /// <summary>
            /// The time of the distributed particle.
            /// Used by distribution algorithms.
            /// </summary>
            public float time;
            /// <summary>
            /// The index of the given particle.
            /// This is the global index of the particle.
            /// </summary>
            public float index;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="particle"></param>
            /// <param name="galaxy"></param>
            /// <param name="particles"></param>
            /// <param name="time"></param>
            /// <param name="index"></param>
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

		#region Getters and Setters

	    internal void SetGalaxyPrefab(GalaxyPrefab galaxyPrefab)
	    {
		    m_galaxyPrefab = galaxyPrefab;
	    }

	    public GalaxyPrefab GalaxyPrefab
	    {
		    get { return m_galaxyPrefab; }
	    }

	    #endregion
	}
}
