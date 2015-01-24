//#define HIDE_SUB_ASSETS
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Galaxia
{
    public sealed class Galaxy : MonoBehaviour
    {
        #region Public
        #endregion
        #region Private
        [SerializeField]
        [HideInInspector]
        private List<Particles> particles;
        [SerializeField]
        private GenerationTime m_generationType;
        [SerializeField]
        private float m_animationSpeed = 0.001f;
        [SerializeField]
        private bool m_animate = false;
        [SerializeField]
        private bool m_directx11 = true;
        [SerializeField]
        private GalaxyPrefab m_galaxy;
        #endregion
        #region Constructors
        void OnEnable()
        {
            if (GalaxyPrefab != null)
            {
                if (GenerationType == GenerationTime.Runtime)
                    GenerateParticles();
                else
                    UpdateParticles();

                foreach(Particles p in particles)
                {
                    p.Build(GalaxyPrefab,m_directx11);
                }
            }
        }
        #endregion
        #region Methods
        #region Particle Generation and Update
        /// <summary>
        /// Generate The Particles Component to all the ParticlesPrefabs in the galaxy
        /// </summary>
        /// <param name="galaxy">The galaxy prefab to use for the ParticlesPrefabs</param>
        public void GenerateParticles()
        {
            DestroyParticles();

            if (GalaxyPrefab != null)
            {
                foreach (ParticlesPrefab prefab in GalaxyPrefab)
                {
                    if (prefab != null)
                    {
                        GameObject obj = new GameObject(prefab.name,typeof(Particles));
                        #if HIDE_SUB_ASSETS
                        obj.hideFlags = HideFlags.HideInHierarchy;
                        #endif
                        obj.hideFlags |= HideFlags.NotEditable;
                        obj.transform.parent = transform;
                        Particles p = obj.GetComponent<Particles>();
                        p.Generate(prefab, GalaxyPrefab);
                        particles.Add(p);
                    }
                }

                //if(Application.isPlaying)
                //{
                //    foreach(Particles p in particles)
                //    {
                //        p.Build(m_galaxy, directx11);
                //    }
                //}
            }
        }
        /// <summary>
        /// Destroy all the Particles components in the galaxy
        /// </summary>
        public void DestroyParticles()
        {
            Debug.Log("Particles Cleared");

            if (particles != null)
            {
                foreach (Particles particle in particles)
                {
                    if (particle != null)
                    {
                        particle.Destroy();
                    }
                        
                }

                particles.Clear();
            }else
            {
                particles = new List<Particles>();
            }
        }
        /// <summary>
        /// Update all the particles in the galaxy
        /// </summary>
        public void UpdateParticles()
        {
            Debug.Log("Updating particles");

            if(GalaxyPrefab != null)
            {
                GalaxyPrefab.Distributor.RecreateCurves();
            }

            foreach(Particles particle in particles)
            {
                if(GalaxyPrefab != null)
                {
                    particle.UpdateParticles(m_galaxy);

                    //if(Application.isPlaying)
                    //{
                    //    particle.Build(m_galaxy);
                    //}
                } 
            }
        }
        /// <summary>
        /// Update all the particles in the galaxy Thread Safe
        /// </summary>
        public void UpdateParticles_MT()
        {
            Debug.Log("Updating particles");

            foreach (Particles particle in particles)
            {
                if (GalaxyPrefab != null && particle.Prefab != null)
                {
                    ThreadPool.QueueUserWorkItem(particle.UpdateParticles_MT, GalaxyPrefab);
                }
            }
        }
        /// <summary>
        /// Recreate the particles component witch was made from a given ParticlesPrefab
        /// </summary>
        /// <param name="prefab">The ParticlesPrefab to search for</param>
        public void UpdateParticles(ParticlesPrefab prefab)
        {
            foreach (Particles particle in particles)
            {
                if (m_galaxy != null && particle.Prefab == prefab)
                {
                    particle.UpdateParticles(GalaxyPrefab);
                    //if(Application.isPlaying)
                    //{
                    //    particle.Build(m_galaxy);
                    //}
                }
            }
        }
        #endregion
        void Update()
        {
            if (GalaxyPrefab != null)
            {
                foreach (Particles particle in particles)
                {
                    if (particle != null)
                    {
                        particle.Prefab.UpdateMaterialAnimation(GalaxyPrefab, AnimationSpeed, Animate);
                        particle.Prefab.UpdateMaterial(GalaxyPrefab);
                        GalaxyPrefab.Distributor.UpdateMaterial(particle.Prefab.Material);
                    }
                    else
                    {
                        Debug.LogWarning("Particle Component was destroyed");
                    }
                    
                }
            }
        }

        public void DrawNow()
        {
            if (GalaxyPrefab != null)
            {
                foreach (Particles particle in particles)
                {
                    if(particle != null)
                    {
                        particle.Prefab.UpdateMaterialAnimation(GalaxyPrefab, AnimationSpeed, Animate);

                        if (particle != null && particle.Prefab != null)
                            particle.DrawNow(GalaxyPrefab);
                    }
                }
            }
        }
        public void Draw()
        {
            if (GalaxyPrefab != null)
            {
                foreach (Particles particle in particles)
                {
                    particle.Prefab.UpdateMaterialAnimation(GalaxyPrefab, AnimationSpeed, Animate);
                    if (particle != null && particle.Prefab != null)
                        particle.Draw();
                }
            }
        }

        

        #endregion
        #region Getters And Setters
        public Vector3 Position { get { return transform.position; } set { transform.position = value; } }
        public List<Particles> Particles { get { return particles; } }
        public GenerationTime GenerationType { get { return m_generationType; } set { m_generationType = value; } }
        public float AnimationSpeed { get { return m_animationSpeed; } set { m_animationSpeed = value; } }
        public bool Animate { get { return m_animate; } set { m_animate = value; } }
        public GalaxyPrefab GalaxyPrefab { get { return m_galaxy; } set { m_galaxy = value; } }
        public bool DirectX11 { get { return m_directx11; } set { m_directx11 = value; } }
        #endregion
        #region enums
        public enum GenerationTime
        {
            Runtime,
            Editor
        }
        #endregion

    }
}
