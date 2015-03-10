#define HIDE_SUB_ASSETS
#define CAN_EDIT_RESOURCES
using System.Collections.Generic;
using UnityEngine;

namespace Galaxia
{
    /// <summary>
    /// The holder of all the ParticlesPrefab
    /// </summary>
    public class GalaxyPrefab : ScriptableObject, IEnumerable<ParticlesPrefab>, ICollection<ParticlesPrefab>
    {
        #region Public
        #endregion
        #region Private
        [SerializeField]
        [HideInInspector]
        private List<ParticlesPrefab> m_particlePrefabs;
        [SerializeField]
        [HideInInspector]
        private ParticleDistributor m_distributor;
        [SerializeField]
        private float m_size = 100;
        [SerializeField]
        private float m_heightOffset = 10;
        [SerializeField]
        [HideInInspector]
        [CurveRange(0, 0, 1, 1)]
        private AnimationCurve m_galaxySpeed = DefaultResources.SpeedCurve;
        #endregion
        #region Internal 
        [SerializeField]
        [HideInInspector]
        internal Shader shader;
        [SerializeField]
        [HideInInspector]
        internal Shader shaderBruteForce;
        [SerializeField]
        [HideInInspector]
        internal Shader shaderBruteForceGLSL;
        #endregion

        /// <summary>
        /// Gets a <see name="Particle Prefab" cref="T:Galaxia.ParticlesPrefab"/>
        /// </summary>
        /// <example>
        /// <code>
        /// GalaxyPrefab galaxy;
        /// galaxy[0].Size = 0.2;
        /// </code>
        /// </example>
        /// <param name="index"></param>
        /// <returns>A <see name="Galaxy Prefab" cref="T:Galaxia.ParticlesPrefab"/> at index</returns>
        public ParticlesPrefab this[int index]
        {
            get { return m_particlePrefabs[index]; }
        }

        private void OnEnable()
        {
            if (m_particlePrefabs == null)
                m_particlePrefabs = new List<ParticlesPrefab>();

            if (shader == null)
                shader = Resources.Load<Shader>("Shaders/ParticleBillboard");

            if(shaderBruteForce == null)
                shaderBruteForce = Resources.Load<Shader>("Shaders/ParticleBillboardBruteForce");

            if (shaderBruteForceGLSL == null)
                shaderBruteForceGLSL = Resources.Load<Shader>("Shaders/ParticleBillboardBruteForceGLSL");
        }

        public IEnumerator<ParticlesPrefab> GetEnumerator()
        {
            return m_particlePrefabs.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_particlePrefabs.GetEnumerator();
        }

        /// <summary>
        /// Populates a <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/> with ready set Presets.
        /// </summary>
        /// <param name="prefab" type="T:Galaxia.ParticlesPrefab">The Prefab to populate</param>
        /// <param name="Preset"> The preset to use</param>
        public void PopulatePreset(ParticlesPrefab prefab,ParticlesPrefab.Preset Preset)
        {
            switch(Preset)
            {
                case ParticlesPrefab.Preset.SmallStars:
                    prefab.Size = 0.4f;
                    prefab.MaxScreenSize = 0.04f;
                    prefab.Texture = DefaultResources.StarTexture;
                    prefab.ColorDistributor.Variation = 0.6f;
                    break;
                case ParticlesPrefab.Preset.BigStars:
                    prefab.Size = 1;
                    prefab.MaxScreenSize = 0.1f;
                    prefab.ColorDistributor.Variation = 0.4f;
                    prefab.Texture = DefaultResources.StarTexture;
                    break;
                case ParticlesPrefab.Preset.Dust:
                    prefab.Size = 3;
                    prefab.MaxScreenSize = 0.25f;
                    prefab.AlphaDistributor.Multiplayer = 0.02f;
                    prefab.ColorDistributor.Variation = 0.1f;
                    prefab.Texture = DefaultResources.DustTexture;
                    break;
            }

            prefab.Seed = Random.Next(int.MinValue, int.MaxValue);
            prefab.OriginalPreset = Preset;
        }

        /// <summary>
        /// Adds a <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/> to the Galaxy
        /// </summary>
        /// <param name="item">The particle prefab</param>
        public void Add(ParticlesPrefab item)
        {
            item.SetUp(this);
            #if HIDE_SUB_ASSETS
            item.hideFlags = HideFlags.HideInHierarchy;
            #endif
            item.Material = new Material(shader);
            item.Material.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            m_particlePrefabs.Add(item);
            RecreateAllGalaxies();
        }

        /// <summary>
        /// Creates and adds a <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/> from a given preset
        /// </summary>
        /// <param name="name">Name of the new Particles Prefab</param>
        /// <param name="Preset"></param>
        /// <returns>The created and populated Particles Prefab</returns>
        public ParticlesPrefab Create(string name,ParticlesPrefab.Preset Preset)
        {
            ParticlesPrefab prefab = CreateInstance<ParticlesPrefab>();
            
            prefab.name = name;
            PopulatePreset(prefab, Preset);
            Add(prefab);
            #if HIDE_SUB_ASSETS
            prefab.hideFlags = HideFlags.HideInHierarchy;
            #endif
            return prefab;
        }
        /// <summary>
        /// Insert a <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/> afer an other
        /// </summary>
        /// <param name="after" type="T:Galaxia.ParticlesPrefab">The prefab to insert after</param>
        /// <param name="toInsert" type="T:Galaxia.ParticlesPrefab">The Prefab to Insert</param>
        public void Insert(ParticlesPrefab after,ParticlesPrefab toInsert)
        {
            int index = 0;
            index = m_particlePrefabs.IndexOf(after);
            if(index >= 0)
            {
                m_particlePrefabs.Insert(index + 1, toInsert);
            #if HIDE_SUB_ASSETS
            toInsert.hideFlags = HideFlags.HideInHierarchy;
            #endif
            }
        }
        /// <summary>
        /// Clears all the Particle Prefabs
        /// Note that this does not Destory the Prefabs
        /// </summary>
        public void Clear()
        {
            m_particlePrefabs.Clear();
        }

        /// <summary>
        /// Check if a <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/> exists in the galaxy
        /// </summary>
        /// <param name="item">The Prefab to search for</param>
        /// <returns>Is the Prefab present in the galaxy</returns>
        public bool Contains(ParticlesPrefab item)
        {
            return m_particlePrefabs.Contains(item);
        }

        /// <summary>
        /// Copies all the <see name="Particle Prefabs" cref="T:Galaxia.ParticlesPrefab"/> to another array
        /// </summary>
        /// <param name="array">The array used to copy tp</param>
        /// <param name="arrayIndex">The index to start at</param>
        public void CopyTo(ParticlesPrefab[] array, int arrayIndex)
        {
            m_particlePrefabs.CopyTo(array,arrayIndex);
        }
        /// <summary>
        /// The number of <see name="Particle Prefabs" cref="T:Galaxia.ParticlesPrefab"/> in the Galaxy
        /// </summary>
        public int Count
        {
            get { return m_particlePrefabs.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// Removes a given <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/> from the galaxy.
        /// If a Prefab does not exist in the galaxy it will return false
        /// </summary>
        /// <param name="item">The Prefab to remove</param>
        /// <returns>Is a Prefab successfuly removed</returns>
        public bool Remove(ParticlesPrefab item)
        {
            return m_particlePrefabs.Remove(item);
        }
        /// <summary>
        /// Updates all <see name="Galaxies" cref="T:Galaxia.Galaxy"/> that use the Galaxy Prefab
        /// If a Galaxy is set to a Manual update, it will not be Updated.
        /// This calls the <see name="UpdateParticles()" cref="M:Galaxia.UpdateParticles"/> method
        /// </summary>
        public void UpdateAllGalaxies()
        {
            foreach (Galaxy galaxy in GameObject.FindObjectsOfType<Galaxy>())
            {
                if (galaxy.GalaxyPrefab == this && galaxy.GenerationType == Galaxy.GalaxyGenerationType.Automatic)
                {
                    galaxy.UpdateParticles();
                }
            }
        }
        /// <summary>
        /// Updates all the <see name="Particles" cref="T:Galaxia.Particles"/> with a given <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/>
        /// in all the Galaxies using the Galaxy prefab
        /// Note that this updated the existing particles and does not Destory them
        /// </summary>
        /// <param name="prefab"></param>
        public void UpdateAllGalaxies(ParticlesPrefab prefab)
        {
            foreach (Galaxy galaxy in GameObject.FindObjectsOfType<Galaxy>())
            {
                if (galaxy.GalaxyPrefab == this && galaxy.GenerationType == Galaxy.GalaxyGenerationType.Automatic)
                {
                    galaxy.UpdateParticles(prefab);
                }
            }
        }

        /// <summary>
        /// Recreates the <see name="Particles" cref="T:Galaxia.Particles"/> of all <see name="Galaxies" cref="T:Galaxia.Galaxy"/> with the Galaxy Prefab
        /// Note that this fully Destroys previous <see name="Particles" cref="T:Galaxia.Particles"/> and Creates new ones.
        /// </summary>
        public void RecreateAllGalaxies()
        {
            foreach (Galaxy galaxy in GameObject.FindObjectsOfType<Galaxy>())
            {
                if (galaxy.GalaxyPrefab == this && galaxy.GenerationType == Galaxy.GalaxyGenerationType.Automatic)
                {
                    galaxy.GenerateParticles();
                }
            }
        }

        /// <summary>
        /// Recreates the <see name="Particles" cref="T:Galaxia.Particles"/> with a given <see name="Particles Prefab" cref="T:Galaxia.ParticlesPrefab"/> of all <see name="Galaxies" cref="T:Galaxia.Galaxy"/> with the Galaxy Prefab
        /// Note that this fully Destroys previous <see name="Particles" cref="T:Galaxia.Particles"/> and Creates new ones.
        /// </summary>
        public void RecreateAllGalaxies(ParticlesPrefab prefab)
        {
            foreach (Galaxy galaxy in GameObject.FindObjectsOfType<Galaxy>())
            {
                if (galaxy.GalaxyPrefab == this && galaxy.GenerationType == Galaxy.GalaxyGenerationType.Automatic)
                {
                    galaxy.GenerateParticles(prefab);
                }
            }
        }

        #region Setters and Getters
        /// <summary>
        /// Size of the Galaxy
        /// </summary>
        public float Size
        {
            get { return m_size; }
            set
            {
                m_size = value;
                UpdateAllGalaxies();
            }
        }

        /// <summary>
        /// Height Offset of the galaxy particles
        /// This includes the range from -Offset ot +Offset from the position the Galaxy is in
        /// </summary>
        public float HeightOffset
        {
            get { return m_heightOffset; }
            set
            {
                m_heightOffset = value;
                UpdateAllGalaxies();
            }
        }
        /// <summary>
        /// The Active Distributor
        /// Here are the algoriths for Position, color, size and rotation distribution of the particles
        /// </summary>
        public ParticleDistributor Distributor
        {
            get { return m_distributor; }
            set
            {
                m_distributor = value;
                #if HIDE_SUB_ASSETS
                m_distributor.hideFlags = HideFlags.HideInHierarchy;
                #endif
                RecreateAllGalaxies();
            }
        }
        #endregion
    }
}
