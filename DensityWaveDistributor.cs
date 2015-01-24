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
        public override void Process(Particle particle, GalaxyPrefab galaxy, ParticlesPrefab particles, float time, float index)
        {
            float dis = particles.PositionDistribution.Evaluate(index / (float)particles.Count);
            particle.index = (float)particles.Count * dis;
            float starIndexMultiplyer = 1f / (float)particles.Count;

            float a = periapsisDistance + starIndexMultiplyer * particle.index;			//Apsis
            float a2 = Mathf.Pow(a, 2);							//the Apsis * Apsis
            float b = apsisDistance + starIndexMultiplyer * particle.index;			//Periphrasis
            float b2 = Mathf.Pow(b, 2);							//the Periphrasis * Periphrasis
            float e = Mathf.Sqrt(1 - Mathf.Min(b2, a2) / Mathf.Max(b2, a2));							//eccentricity of the ellipse
            float GP = G * (CenterMass + StarMass);

            float T = (Mathf.PI * 2) * Mathf.Sqrt(Mathf.Pow(a, 3) / GP);	//orbital period

            //_focalPoint = Random.Range(galaxy.FocalPoint, -galaxy.FocalPoint);		//the foci's center of the ellipse
            particle.focalPoint = Random.Next(FocalPoint, -FocalPoint);
            float _centerX = (a * e) * particle.focalPoint;							//set the center to be the foci's point
            float _centerZ = 0;

            float angle = (angleOffset / (float)particles.Count) * particle.index;

            particle.startingTime = Random.Next(0f, (2f * Mathf.PI));
            float _angleRotation = time + particle.startingTime;

            float CosB = Mathf.Cos(angle);								//the Cos of the rotation angle of the ellipse
            float SinB = Mathf.Sin(angle);								//the Sin of the rotation angle of the ellipse

            float x = (a * Mathf.Cos(_angleRotation / T)) + _centerX;
            float z = (b * Mathf.Sin(_angleRotation / T)) + _centerZ;

            Vector3 _pos = new Vector3(((x * CosB) + (z * SinB)) * galaxy.Size, 0, ((x * SinB) - (z * CosB)) * galaxy.Size);


            float height = (Mathf.Clamp01(GalaxyHeightIntegralInverse.Evaluate(Random.Next())) * (galaxy.HeightOffset + galaxy.HeightOffset)) - galaxy.HeightOffset;
            height *= GalaxyHeightMultiply.Evaluate(index / (float)particles.Count);
            _pos.y = height;

            particle.color = particles.GetColor(_pos.magnitude, galaxy.Size, _angleRotation / T, particle.index);
            particle.size = particles.GetSize(_pos.magnitude, galaxy.Size, _angleRotation / T, particle.index);

            particle.position = _pos;
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
    }
}
