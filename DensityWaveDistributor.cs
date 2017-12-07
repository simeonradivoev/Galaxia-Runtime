// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using UnityEngine;

namespace Galaxia
{
	/// <summary>
	/// The Density Wave Distribution Algorithm
	/// <see href="http://simeon.co.vu/Documentation/Galaxia/custom_distributors.html"/>
	/// </summary>
	public sealed class DensityWaveDistributor : ParticleDistributor
    {
        #region Private
        [SerializeField, Delayed]
        private float periapsisDistance = 0.08f;
        [SerializeField, Delayed]
        private float apsisDistance = 0.01f;
        [SerializeField, Delayed]
        private float CenterMass = 830000;
        [SerializeField, Delayed]
        private float StarMass = 80;
        [SerializeField, Delayed]
        private float FocalPoint = -1;
        [SerializeField, Delayed]
        private float angleOffset = 8;
        [SerializeField, Delayed]
        private float m_heightVariance = 1;
        [SerializeField]
        [CurveRange(0, 0, 1, 1)]
        private AnimationCurve m_galaxyHeightMultiply = DefaultResources.HeightCurve;
		#endregion

		/// <summary>
		/// <see cref="Galaxia.ParticleDistributor.CanProcess"/>
		/// </summary>
		/// <param name="particles"></param>
		/// <returns></returns>
		public override bool CanProcess(ParticlesPrefab particles)
		{
			return true;
		}

		/// <summary>
		/// The main function for star orbits in the galaxy.
		/// It also calculates the color of each particle
		/// <see href="http://simeon.co.vu/Documentation/Galaxia/custom_distributors.html"/>
		/// </summary>
		/// <param name="context">The particle context containing all particle data</param>
		public override void Process(ProcessContext context)
        {
            float dis = context.particles.PositionDistribution.Evaluate(context.index / (float)context.particles.Count);
            context.particle.index = (float)context.particles.Count * dis;
            float starIndexMultiplyer = (1f / (float)context.particles.Count) * context.particle.index;

            float a = periapsisDistance + starIndexMultiplyer;			//Apsis
            float a2 = Mathf.Pow(a, 2);							//the Apsis * Apsis
            float b = apsisDistance + starIndexMultiplyer;			//Periphrasis
            float b2 = Mathf.Pow(b, 2);							//the Periphrasis * Periphrasis
            float e = Mathf.Sqrt(1 - Mathf.Min(b2, a2) / Mathf.Max(b2, a2));							//eccentricity of the ellipse
            float GP = G * (CenterMass + StarMass);

            float T = (Mathf.PI * 2) * Mathf.Sqrt(Mathf.Pow(a, 3) / GP);	//orbital period

            context.particle.focalPoint = Random.Next(FocalPoint, -FocalPoint);
            float _centerX = (a * e) * context.particle.focalPoint;							//set the center to be the foci's point;

            float angle = (angleOffset / (float)context.particles.Count) * context.particle.index;

            context.particle.startingTime = Random.Next(0f, (2f * Mathf.PI));
            float _angleRotation = context.time + context.particle.startingTime;
            float ellipseRotation = _angleRotation / T;

            float CosB = Mathf.Cos(angle);								//the Cos of the rotation angle of the ellipse
            float SinB = Mathf.Sin(angle);								//the Sin of the rotation angle of the ellipse

            float x = (a * Mathf.Cos(ellipseRotation)) + _centerX;
            float z = (b * Mathf.Sin(ellipseRotation));

            Vector3 _pos = new Vector3(((x * CosB) + (z * SinB)) * context.galaxy.Size, 0, ((x * SinB) - (z * CosB)) * context.galaxy.Size);


            float height = (float)Random.NextGaussianDouble(m_heightVariance) * context.galaxy.HeightOffset;
            height *= GalaxyHeightMultiply.Evaluate(_pos.magnitude / context.galaxy.Size);
            _pos.y = height;

            ProcessProperties(context,_pos,ellipseRotation);

            context.particle.position = _pos;
        }

        public override void UpdateMaterial(Material material)
        {
            material.SetFloat("CenterMass", CenterMass);
            material.SetFloat("StarMass", StarMass);
            material.SetFloat("FocalPoint", FocalPoint);
            material.SetFloat("angleOffset", angleOffset);
            material.SetFloat("apsisDistance", apsisDistance);
            material.SetFloat("periapsisDistance", periapsisDistance);
        }

        #region Getters and setters
        //public AnimationCurve GalaxyHeightDistribution { get { return m_galaxyHeightDistribution; } set { m_galaxyHeightDistribution = value; RecreateCurves(); } }
        public AnimationCurve GalaxyHeightMultiply { get { return m_galaxyHeightMultiply; } set { m_galaxyHeightMultiply = value; RecreateCurves(); } }
        #endregion
    }
}
