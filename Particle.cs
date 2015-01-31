using UnityEngine;
using System.Collections.Generic;

namespace Galaxia
{
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
        public static implicit operator ParticleSystem.Particle(Particle p)
        {
            ParticleSystem.Particle particle = new ParticleSystem.Particle();
            particle.color = p.color;
            particle.position = p.position;
            particle.lifetime = Mathf.Infinity;
            particle.startLifetime = Mathf.Infinity;
            particle.size = p.size;
            particle.rotation = p.rotation * Mathf.Rad2Deg;
            return particle;
        }

        public static implicit operator Particle(ParticleSystem.Particle p)
        {
            Particle particle = new Particle();
            particle.color = p.color;
            particle.position = p.position;
            return particle;
        }
        #endregion
        #region Getters and setters
        public Vector3 position { get { return m_position; } set { m_position = value; } }
        public Color color { get { return m_color; } set { m_color = value; } }
        public float size { get { return m_size; } set { m_size = value; } }
        public float rotation { get { return m_rotation; } set { m_rotation = value; } }
        public float focalPoint { get { return m_focalPoint; } set { m_focalPoint = value; } }
        public float startingTime { get { return m_startingTime; } set { m_startingTime = value; } }
        public float index { get { return m_index; } set { m_index = value; } }
        public int sheetPosition { get { return m_sheetPosition; } set { m_sheetPosition = value; } }
        #endregion
    }
}
