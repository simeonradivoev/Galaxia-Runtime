#define HIDE_SUB_ASSETS
#define EDIT_RESOURCES
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Galaxia
{
    /// <summary>
    /// The Component that holds and manages visualisation of GalaxyPrefab
    /// </summary>
    public sealed class Galaxy : MonoBehaviour
    {
        #region Public
        #endregion
        #region Private
        [SerializeField]
        [HideInInspector]
        private List<Particles> particles;
        [SerializeField]
        private GalaxyGenerationType m_generationType;
        [SerializeField]
        private bool m_gpu = true;
        [SerializeField]
        private GalaxyPrefab m_galaxy;
        #endregion
        #region Constructors
        void OnEnable()
        {
            if (GalaxyPrefab != null)
            {
                GenerateParticles();
            }
        }
        #endregion
        #region Methods
        #region Particle Generation and Update
        /// <summary>
        /// Generates <see name="Particles" cref="T:Galaxia.Particles"/> to all the <see name="Particle Prefabs" cref="T:Galaxia.ParticlesPrefab"/> in the galaxy
        /// This function only generates the particles, not the Meshes
        /// </summary>
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
                        #if EDIT_RESOURCES
                        obj.hideFlags |= HideFlags.NotEditable;
                        #endif
                        obj.transform.parent = transform;
                        Particles p = obj.GetComponent<Particles>();
                        p.Generate(prefab, GalaxyPrefab, GPU);
                        particles.Add(p);
                        prefab.CreateMaterial(GalaxyPrefab,GPU);
                        prefab.UpdateMaterial(GalaxyPrefab);
                    }
                }
            }
        }

        /// <summary>
        /// Same as the <see name="GenerateParticles()" cref="M:Galaxia.Galaxy.GenerateParticles"/> but for a specific <see name="Particle Prefab" cref="T:Galaxia.ParticlesPrefab"/>
        /// </summary>
        /// <param name="prefab" type="T:Galaxia.ParticlesPrefab">The Particles Prefab to use for the generation</param>
        public void GenerateParticles(ParticlesPrefab prefab)
        {
            if (GalaxyPrefab != null)
            {
                if (prefab != null)
                {
                    GameObject obj = new GameObject(prefab.name, typeof(Particles));
                    #if HIDE_SUB_ASSETS
                    obj.hideFlags = HideFlags.HideInHierarchy;
                    #endif
                    #if EDIT_RESOURCES
                    obj.hideFlags |= HideFlags.NotEditable;
                    #endif
                    obj.transform.parent = transform;
                    Particles p = obj.GetComponent<Particles>();
                    p.Generate(prefab, GalaxyPrefab, GPU);
                    particles.Add(p);
                    prefab.CreateMaterial(GalaxyPrefab, GPU);
                    prefab.UpdateMaterial(GalaxyPrefab);
                }
                else
                {
                    Debug.LogError("Particle Prefab was destoryed or missing");
                }
            }else
            {
                Debug.LogWarning("No Prefab assigned");
            }
        }
        /// <summary>
        /// Destroys all the Particles components in the Galaxy.
        /// </summary>
        public void DestroyParticles()
        {
            //Debug.Log("Particles Cleared");

            if (particles != null)
            {
                for (int i = 0; i < particles.Count; i++)
                {
                    particles[i].Destroy();
                }

                particles.Clear();

                foreach(Transform t in transform)
                {
                    DestroyImmediate(t.gameObject);
                }
            }
            else
            {
                particles = new List<Particles>();
            }
        }
        /// <summary>
        /// Destroys all <see name="Particles" cref="T:Galaxia.Particles"/> with a given prefab.
        /// </summary>
        /// <param name="prefab" type="T:Galaxia.ParticlesPrefab">particle prefab to search for</param>
        public void DestroyParticles(ParticlesPrefab prefab)
        {
            if (particles != null)
            {
                for (int i = 0; i < particles.Count;i++ )
                {
                    if (particles[i] != null && particles[i].Prefab == prefab)
                    {
                        particles[i].Destroy();
                        particles.RemoveAt(i);
                    }
                }
            }
            else
            {
                particles = new List<Particles>();
            }
        }

        /// <summary>
        /// Marks all the <see name="Particles" cref="T:Galaxia.Particles"/> for Update, next frame.
        /// </summary>
        public void UpdateParticles()
        {
            //Debug.Log("Updating particles");

            if(GalaxyPrefab != null)
            {
                GalaxyPrefab.Distributor.RecreateCurves();
            }

            foreach(Particles particle in particles)
            {
                if(GalaxyPrefab != null)
                {
                    particle.NeedsUpdate = true;
                } 
            }
        }
        void ForceUpdateParticles()
        {
            //Debug.Log("Updating particles");

            if (GalaxyPrefab != null)
            {
                GalaxyPrefab.Distributor.RecreateCurves();
            }

            foreach (Particles particle in particles)
            {
                if (GalaxyPrefab != null)
                {
                    //particle.Prefab.CreateMaterial(GalaxyPrefab,DirectX11);
                    particle.UpdateParticles();
                }
            }
        }
        /// <summary>
        /// Marks a <see name="Particles Component" cref="T:Galaxia.Particles"/> with a given <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/> for Update, next frame.
        /// </summary>
        /// <param name="prefab">The ParticlesPrefab to search for</param>
        public void UpdateParticles(ParticlesPrefab prefab)
        {
            foreach (Particles particle in particles)
            {
                if (m_galaxy != null && particle.Prefab == prefab)
                {
                    particle.NeedsUpdate = true;
                }
            }
        }
        void ForceUpdateParticles(ParticlesPrefab prefab)
        {
            foreach (Particles particle in particles)
            {
                if (m_galaxy != null && particle.Prefab == prefab)
                {
                    particle.UpdateParticles();
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
                        particle.Prefab.UpdateMaterialAnimation(GalaxyPrefab, 0, false);
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
        /// <summary>
        /// Draws the Galaxy and all the <see name="Particles" cref="T:Galaxia.Particles"/> in the Galaxy Now
        /// </summary>
        public void DrawNow()
        {
            if (GalaxyPrefab != null && GPU)
            {
                foreach (Particles particle in particles)
                {
                    if(particle != null)
                    {
                        particle.Prefab.UpdateMaterialAnimation(GalaxyPrefab, 0, false);

                        if (particle != null && particle.Prefab != null)
                            particle.DrawNow();
                    }
                }
            }
        }
        /// <summary>
        /// Sends the Galaxy and all the <see name="Particles" cref="T:Galaxia.Particles"/> for Rendering
        /// </summary>
        public void Draw()
        {
            if (GalaxyPrefab != null && GPU)
            {
                foreach (Particles particle in particles)
                {
                    particle.Prefab.UpdateMaterialAnimation(GalaxyPrefab, 0, false);
                    if (particle != null && particle.Prefab != null)
                        particle.Draw();
                }
            }
        }

        

        #endregion
        #region Getters And Setters
        public Vector3 Position { get { return transform.position; } set { transform.position = value; } }
        /// <summary>
        /// List of <see name="Particles" cref="T:Galaxia.Particles"/> that holds the Components responsible for the individual visualisation of the <see name="Particle Prefabs" cref="T:Galaxia.ParticlesPrefabs"/>.
        /// </summary>
        public List<Particles> Particles { get { return particles; } }
        /// <summary>
        /// This dictates if the <see name="Galaxy" cref="T:Galaxia.Galaxy"/> updates automaticly when a Proprty is changed.
        /// If it is set to manual, <see name="GenerateParticles()" cref="M:Galaxia.Galaxy.GenerateParticles"/> or <see name="UpdateParticles()" cref="M:Galaxia.Galaxy.UpdateParticles"/> must be called every time the galaxy is changed.
        /// </summary>
        public GalaxyGenerationType GenerationType { get { return m_generationType; } set { m_generationType = value; } }
        /// <summary>
        /// The meat of the galaxy. This is a holder for galaxy specific particle properties.
        /// It holds a list of <see name="Particle Prefabs" cref="T:Galaxia.ParticlesPrefab"/> as well as the <see name="Distributor" cref="T:Galaxia.ParticleDistributor"/>.
        /// </summary>
        public GalaxyPrefab GalaxyPrefab
        {
            get { return m_galaxy; }
            set
            {
                if(m_galaxy != value && m_generationType == GalaxyGenerationType.Automatic)
                {
                    m_galaxy = value;
                    GenerateParticles();
                }
                else
                {
                    m_galaxy = value;
                }
            }
        }
        /// <summary>
        /// Returns if DirectX 11 is supported on the software.
        /// If it is avalible you can disable it from here.
        /// </summary>
        [Obsolete("New Function is GPU")]
        public bool DirectX11
        {
            get { return GPU; }
            set
            {
                GPU = value;
            }
        }

        /// <summary>
        /// Will use Custom particles, rendered on the GPU
        /// if not, it will use the Unity Particle System
        /// </summary>
        public bool GPU
        {
            get { return m_gpu; }
            set
            {
                if (m_gpu != value)
                {
                    m_gpu = value;
                    if (m_generationType == GalaxyGenerationType.Automatic)
                        GenerateParticles();
                }
            }
        }

        public static bool OpenGL
        {
            get { return SystemInfo.graphicsDeviceVersion.Contains("OpenGL"); }
        }

        public static bool SupportsDirectX11
        {
            get { return !OpenGL && SystemInfo.graphicsShaderLevel >= 40; }
        }

        #endregion
        #region enums
        public enum GalaxyGenerationType
        {
            Automatic,
            Manual
        }
        #endregion

    }
}
