//#define HIDE_SUB_ASSETS
//#define CAN_EDIT_RESOURCES
using System.Collections.Generic;
using UnityEngine;

namespace Galaxia
{
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
        [CurveRange(0, 0, 1, 1)]
        private AnimationCurve m_galaxySpeed = DefaultResources.SpeedCurve;
        #endregion
        #region Internal 
        [SerializeField]
        [HideInInspector]
        internal Shader shader;
        #endregion

        public ParticlesPrefab this[int index]
        {
            get { return m_particlePrefabs[index]; }
        }

        public void OnEnable()
        {
            if (m_particlePrefabs == null)
                m_particlePrefabs = new List<ParticlesPrefab>();

            if (shader == null)
                shader = Resources.Load<Shader>("Shaders/ParticleBillboard");
        }
        public IEnumerator<ParticlesPrefab> GetEnumerator()
        {
            return m_particlePrefabs.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_particlePrefabs.GetEnumerator();
        }

        public void PopulatePreset(ParticlesPrefab prefab,ParticlesPrefab.Preset Preset)
        {
            switch(Preset)
            {
                case ParticlesPrefab.Preset.SmallStars:
                    prefab.Size = 0.4f;
                    prefab.MaxScreenSize = 0.04f;
                    prefab.Texture = DefaultResources.StarTexture;
                    prefab.ColorVariation = 0.6f;
                    break;
                case ParticlesPrefab.Preset.BigStars:
                    prefab.Size = 1;
                    prefab.MaxScreenSize = 0.1f;
                    prefab.ColorVariation = 0.4f;
                    prefab.Texture = DefaultResources.StarTexture;
                    break;
                case ParticlesPrefab.Preset.Dust:
                    prefab.Size = 3;
                    prefab.MaxScreenSize = 0.25f;
                    prefab.AlphaMultiplayer = 0.02f;
                    prefab.ColorVariation = 0.1f;
                    prefab.Texture = DefaultResources.DustTexture;
                    break;
            }

            prefab.Seed = Random.Next(int.MinValue, int.MaxValue);
            prefab.OriginalPreset = Preset;
        }

        public void Add(ParticlesPrefab item)
        {
            item.SetUp(this);
            #if HIDE_SUB_ASSETS
            item.hideFlags = HideFlags.HideInHierarchy;
            #endif
            item.Material = new Material(shader);
            item.Material.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            m_particlePrefabs.Add(item);
        }

        public ParticlesPrefab Create(string name,ParticlesPrefab.Preset Preset)
        {
            ParticlesPrefab prefab = CreateInstance<ParticlesPrefab>();
            
            prefab.name = name;
            //prefab.Material = new Material(shader);
            //prefab.Material.name = name + "_material";
            //prefab.Material.hideFlags = HideFlags.None;
            PopulatePreset(prefab, Preset);
            Add(prefab);
            #if HIDE_SUB_ASSETS
            prefab.hideFlags = HideFlags.HideInHierarchy;
            #endif
            return prefab;
        }

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

        public void Clear()
        {
            m_particlePrefabs.Clear();
        }

        public bool Contains(ParticlesPrefab item)
        {
            return m_particlePrefabs.Contains(item);
        }

        public void CopyTo(ParticlesPrefab[] array, int arrayIndex)
        {
            m_particlePrefabs.CopyTo(array,arrayIndex);
        }

        public int Count
        {
            get { return m_particlePrefabs.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ParticlesPrefab item)
        {
            return m_particlePrefabs.Remove(item);
        }

        #region Setters and Getters
        public float Size { get { return m_size; } set { m_size = value; } }
        public float HeightOffset { get { return m_heightOffset; } set { m_heightOffset = value; } }
        public AnimationCurve GalaxySpeed { get { return m_galaxySpeed; } set { m_galaxySpeed = value; } }
        public ParticleDistributor Distributor
        {
            get { return m_distributor; }
            set
            {
                m_distributor = value;
                #if HIDE_SUB_ASSETS
                m_distributor.hideFlags = HideFlags.HideInHierarchy;
                #endif
            }
        }
        #endregion
    }
}
