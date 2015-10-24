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
            ProcessProperties(context, _pos, 0);
            context.particle.position = _pos;
        }

        public override void UpdateMaterial(Material material)
        {
            
        }
    }
}
