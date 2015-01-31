using UnityEngine;
using System.Collections.Generic;

namespace Galaxia
{
    public sealed class DensityWaveDistributor : ParticleDistributor
    {
        #region Private
        [SerializeField]
        private float periapsisDistance = 0.08f;
        [SerializeField]
        private float apsisDistance = 0.01f;
        [SerializeField]
        private float CenterMass = 830000;
        [SerializeField]
        private float StarMass = 80;
        [SerializeField]
        private float FocalPoint = -1;
        [SerializeField]
        private float angleOffset = 8;
        [SerializeField]
        [CurveRange(0, 0, 1, 1)]
        private AnimationCurve m_galaxyHeightDistribution = DefaultResources.HeightCurve;
        [SerializeField]
        [CurveRange(0, 0, 1, 1)]
        private AnimationCurve m_galaxyHeightMultiply = DefaultResources.HeightCurve;
        [SerializeField]
        [HideInInspector]
        private AnimationCurve GalaxyHeightIntegral = Integral(DefaultResources.HeightCurve, 32);
        [SerializeField]
        [HideInInspector]
        private AnimationCurve GalaxyHeightIntegralInverse = Inverse(Integral(DefaultResources.HeightCurve, 32));
        #endregion
        

        /// <summary>
        /// The main function for star orbits in the galaxy.
        /// It also calculates the color of each particle
        /// </summary>
        /// <param name="_particle">The particle to use, to calucate it's position</param>
        /// <param name="galaxy">The GalaxyPrefab that holds all the information on the galaxy generation</param>
        /// <param name="particles">The ParticlePrefab that holds the information on the particle itself</param>
        /// <param name="center">The local center of the galaxy. It is adviced to use the transform of the galaxy to move it</param>
        /// <param name="angleRotation">the global rotation for all the particles a.k.a. the time</param>
        /// <param name="index">the index of the particle</param>
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

            float CosB = Mathf.Cos(angle);								//the Cos of the rotation angle of the ellipse
            float SinB = Mathf.Sin(angle);								//the Sin of the rotation angle of the ellipse

            float x = (a * Mathf.Cos(_angleRotation / T)) + _centerX;
            float z = (b * Mathf.Sin(_angleRotation / T));

            Vector3 _pos = new Vector3(((x * CosB) + (z * SinB)) * context.galaxy.Size, 0, ((x * SinB) - (z * CosB)) * context.galaxy.Size);


            float height = (Mathf.Clamp01(GalaxyHeightIntegralInverse.Evaluate(Random.Next())) * (context.galaxy.HeightOffset * 2)) - context.galaxy.HeightOffset;
            height *= GalaxyHeightMultiply.Evaluate(context.index / (float)context.particles.Count);
            _pos.y = height;

            context.particle.color = context.particles.GetColor(_pos,_pos.magnitude, context.galaxy.Size, _angleRotation / T, context.particle.index);
            context.particle.size = context.particles.GetSize(_pos,_pos.magnitude, context.galaxy.Size, _angleRotation / T, context.particle.index);
            context.particle.rotation = context.particles.GetRotation();

            context.particle.position = _pos;
        }

        public override void RecreateCurves()
        {
            base.RecreateCurves();
            GalaxyHeightIntegral = ParticleDistributor.Integral(GalaxyHeightDistribution, 64);
            GalaxyHeightIntegralInverse = Inverse(GalaxyHeightIntegral);
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
        public AnimationCurve GalaxyHeightDistribution { get { return m_galaxyHeightDistribution; } set { m_galaxyHeightDistribution = value; RecreateCurves(); } }
        public AnimationCurve GalaxyHeightMultiply { get { return m_galaxyHeightMultiply; } set { m_galaxyHeightMultiply = value; RecreateCurves(); } }
        #endregion
    }
}
