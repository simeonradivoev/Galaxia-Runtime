using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Galaxia
{
    public class GaussianDistributor : ParticleDistributor
    {
        #region Private
        [SerializeField]
        private double m_variation = 1;
        #endregion

        public override void Process(ProcessContext context)
        {
            Vector3 _pos = new Vector3((float)Random.NextGaussianDouble(m_variation), (float)Random.NextGaussianDouble(m_variation), (float)Random.NextGaussianDouble(m_variation)) * context.galaxy.Size;
            context.particle.color = context.particles.GetColor(_pos,_pos.magnitude, context.galaxy.Size, 0, context.particle.index);
            context.particle.size = context.particles.GetSize(_pos,_pos.magnitude, context.galaxy.Size, 0, context.particle.index);
            context.particle.rotation = context.particles.GetRotation();
            context.particle.position = _pos;
        }

        public override void UpdateMaterial(Material material)
        {
            
        }
    }
}
