// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using UnityEngine;

namespace Galaxia
{
	/// <summary>
	/// The Gaussian Distribution algorith.
	/// </summary>
	/// <remarks>
	/// This distributor resembles a star cluster.
	/// </remarks>
	public class GaussianDistributor : ParticleDistributor
    {
        #region Private
        [SerializeField]
        private double m_variation = 1;
		#endregion

		/// <summary>
		/// Used by the Particle Generator to modify/distribute the particles to a desired shape.
		/// </summary>
		/// <remarks>
		/// This is where particles are processed one by one.
		/// <see cref="Galaxia.ParticleDistributor.ProcessContext"/>
		/// </remarks>
		/// <param name="context">The context holds information on the current particle and Galaxy Object.</param>
		public override void Process(ProcessContext context)
        {
            Vector3 _pos = new Vector3((float)Random.NextGaussianDouble(m_variation), (float)Random.NextGaussianDouble(m_variation), (float)Random.NextGaussianDouble(m_variation)) * context.galaxy.Size;
            ProcessProperties(context, _pos, 0);
            context.particle.position = _pos;
        }

		/// <summary>
		/// Updates all uniform variables in the given material
		/// </summary>
		/// <param name="material">the given material</param>
		public override void UpdateMaterial(Material material)
        {
            
        }
    }
}
