// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using UnityEngine;

namespace Galaxia
{
    /// <summary>
    /// Main data storage class for particles.
    /// </summary>
    [System.Serializable]
    public class Particle
    {
        #region Private
        [SerializeField]
        private Vector3 m_position;
        [SerializeField]
        private Color m_color;
        [SerializeField]
        private float m_size;
        [SerializeField]
        private float m_focalPoint;
        [SerializeField]
        private float m_startingTime;
        [SerializeField]
        private float m_index;
        [SerializeField]
        private float m_rotation;
        [SerializeField]
        private int m_sheetPosition;
        #endregion
        #region Methods
        /// <summary>
        /// Converts to Unity's particle system particle
        /// </summary>
        /// <param name="p"></param>
        /// <param name="sheetSize">The sheet power size.</param>
        /// <returns></returns>
        public static ParticleSystem.Particle ConvertToParticleSystem(Particle p,int sheetSize)
        {
            ParticleSystem.Particle particle = new ParticleSystem.Particle();
            particle.startColor = p.color;
            particle.position = p.position;
			//add 1 to life to make sure particles don't disappear
            particle.remainingLifetime = p.sheetPosition + 1;
            particle.startLifetime = sheetSize* sheetSize + 1;
            particle.startSize = p.size;
            particle.rotation = p.rotation * Mathf.Rad2Deg;
            return particle;
        }

		public static implicit operator Particle(ParticleSystem.Particle p)
        {
            Particle particle = new Particle();
            particle.color = p.startColor;
            particle.position = p.position;
            return particle;
        }
        #endregion
        #region Getters and setters
        /// <summary>
        /// The Position of the particle
        /// </summary>
        public Vector3 position { get { return m_position; } set { m_position = value; } }
        /// <summary>
        /// The Color of the particle
        /// </summary>
        public Color color { get { return m_color; } set { m_color = value; } }
        /// <summary>
        /// The size of the particle
        /// </summary>
        public float size { get { return m_size; } set { m_size = value; } }
        /// <summary>
        /// The rotation of the particle
        /// </summary>
        public float rotation { get { return m_rotation; } set { m_rotation = value; } }
        /// <summary>
        /// The focal point of the particle's elliptical orbit.
        /// </summary>
        public float focalPoint { get { return m_focalPoint; } set { m_focalPoint = value; } }
        /// <summary>
        /// The starting time of the particle. Used by distribution algorithms.
        /// </summary>
        public float startingTime { get { return m_startingTime; } set { m_startingTime = value; } }
        /// <summary>
        /// The global index of the particle.
        /// </summary>
        public float index { get { return m_index; } set { m_index = value; } }
        /// <summary>
        /// The sheet position of the particle. Used by particle rendering for having multiple particle type from one texture sheet.
        /// </summary>
        public int sheetPosition { get { return m_sheetPosition; } set { m_sheetPosition = value; } }
        #endregion
    }
}
