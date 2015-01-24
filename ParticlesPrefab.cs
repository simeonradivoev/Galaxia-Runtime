using System.Collections.Generic;
using UnityEngine;

namespace Galaxia
{
    public class ParticlesPrefab : ScriptableObject
    {
        #region Public
        #endregion
        #region Private
        [SerializeField]
        [HideInInspector]
        private bool m_active = true;
        [SerializeField]
        private int m_count = 100000;
        [SerializeField]
        private int m_seed = 0;
        [SerializeField]
        private float m_size = 0.1f;
        [SerializeField]
        private float m_maxScreenSize = 0.1f;
        [Header("Distribution")]
        [CurveRange(0, 0, 1, 1)]
        [SerializeField]
        private AnimationCurve m_positionDistribution = AnimationCurve.Linear(0,0,1,1);
        [Header("Size")]
        [SerializeField]
        private Distribution m_sizeDistributionType;
        [CurveRange(0, 0, 1, 1)]
        [SerializeField]
        private AnimationCurve m_sizeDistribution = AnimationCurve.Linear(0, 1, 1, 1);
        [Range(0, 1)]
        [SerializeField]
        private float m_sizeVariation = 0;
        [Header("Alpha")]
        [SerializeField]
        private Distribution m_alphaDistributionType;
        [CurveRange(0, 0, 1, 1)]
        [SerializeField]
        private AnimationCurve m_alpha = DefaultResources.AlphaCurve;
        [Range(0, 1)]
        [SerializeField]
        private float m_alphaVariation = 0;
        [Range(0, 1)]
        [SerializeField]
        private float m_alphaMultiplayer = 1;
        [Header("Color")]
        [SerializeField]
        private Distribution m_colorDistributionType;
        [SerializeField]
        private Gradient m_color = DefaultResources.StarColorGradient;
        [Range(0, 1)]
        [SerializeField]
        private float m_colorVariation = 0;
        [SerializeField]
        private Texture2D m_texture;

        #region hidden
        [SerializeField]
        [HideInInspector]
        private Preset m_originalPreset;
        [SerializeField]
        [HideInInspector]
        private Material m_material;
        [SerializeField]
        [HideInInspector]
        private GalaxyPrefab m_galaxyPrefab;
        #endregion
        #endregion

        public void SetUp(GalaxyPrefab prefab)
        {
            m_galaxyPrefab = prefab;
        }

        public Color GetColor(float distance, float GalaxySize, float angle,float index)
        {
            float colorPos = 0;
            float alphaPos = 0;
            float rand = Random.Next();
            switch (ColorDistributionType)
            {
                case ParticlesPrefab.Distribution.Angle:
                    colorPos = Mathf.Lerp((Mathf.Cos(angle) + 1) / 2f, rand, ColorVariation);
                    break;
                case ParticlesPrefab.Distribution.Distance:
                    colorPos = Mathf.Lerp(colorPos, rand, ColorVariation);
                    break;
                case ParticlesPrefab.Distribution.Linear:
                    colorPos = Mathf.Lerp((float)index / (float)Count, rand, ColorVariation);
                    break;
                case Distribution.Random:
                    colorPos = rand;
                    break;
            }

            rand = Random.Next();
            switch (AlphaDistributionType)
            {
                case ParticlesPrefab.Distribution.Angle:
                    alphaPos = Mathf.Lerp((Mathf.Cos(angle) + 1) / 2f, rand, AlphaVariation);
                    break;
                case ParticlesPrefab.Distribution.Distance:
                    alphaPos = Mathf.Lerp(colorPos, rand, AlphaVariation);
                    break;
                case ParticlesPrefab.Distribution.Linear:
                    alphaPos = Mathf.Lerp((float)index / (float)Count, rand, AlphaVariation);
                    break;
                case Distribution.Random:
                    alphaPos = rand;
                    break;
            }

            Color c = Color.Evaluate(colorPos);
            c.a *= Alpha.Evaluate(alphaPos) * AlphaMultiplayer;
            return c;
        }

        public float GetSize(float distance, float GalaxySize, float angle, float index)
        {
            float sizePos = 0;
            float rand = Random.Next();
            switch (SizeDistributionType)
            {
                case ParticlesPrefab.Distribution.Angle:
                    sizePos = Mathf.Lerp((Mathf.Cos(angle) + 1) / 2f, rand, SizeVariation);
                    break;
                case ParticlesPrefab.Distribution.Distance:
                    sizePos = Mathf.Lerp(sizePos, rand, SizeVariation);
                    break;
                case ParticlesPrefab.Distribution.Linear:
                    sizePos = Mathf.Lerp((float)index / (float)Count, rand, SizeVariation);
                    break;
                case Distribution.Random:
                    sizePos = rand;
                    break;
            }

            float size = SizeDistribution.Evaluate(sizePos) * Size;
            return size;
        }

        public void RecreateMaterial(GalaxyPrefab prefab)
        {
            if (m_material == null)
                m_material = new Material(prefab.shader);

            UpdateMaterial(prefab);
        }

        public void UpdateMaterial(GalaxyPrefab prefab)
        {
            if (prefab != null && m_material != null)
            {
                m_material.mainTexture = Texture;
                m_material.SetFloat("GalaxySize", prefab.Size);
                m_material.SetFloat("MaxScreenSize", MaxScreenSize);
                m_material.SetFloat("Count", Count);
            }
        }

        public void UpdateMaterialAnimation(GalaxyPrefab prefab,float speed,bool Animate)
        {
            if (prefab != null && m_material != null)
            {
                m_material.SetInt("Animate", Animate ? 1 : 0);
                m_material.SetFloat("AnimationSpeed", speed);
            }
        }

        public void DestoryPrefab()
        {
            GameObject.DestroyImmediate(m_material,true);
            GameObject.DestroyImmediate(this, true);
        }

        #region Getters and Setters
        public bool active { get { return m_active; } set { m_active = value; } }
        public int Count { get { return m_count; } set { m_count = value; } }
        public int Seed { get { return m_seed; } set { m_seed = value; } }
        public float Size { get { return m_size; } set { m_size = value; } }
        public float MaxScreenSize { get { return m_maxScreenSize; } set { m_maxScreenSize = value; } }
        public AnimationCurve PositionDistribution { get { return m_positionDistribution; } set { m_positionDistribution = value; } }
        public Distribution SizeDistributionType { get { return m_sizeDistributionType; } set { m_sizeDistributionType = value; } }
        public AnimationCurve SizeDistribution { get { return m_sizeDistribution; } set { m_sizeDistribution = value; } }
        public float SizeVariation { get { return m_sizeVariation; } set { m_sizeVariation = value; } }
        public Distribution AlphaDistributionType { get { return m_alphaDistributionType; } set { m_alphaDistributionType = value; } }
        public AnimationCurve Alpha { get { return m_alpha; } set { m_alpha = value; } }
        public float AlphaVariation { get { return m_alphaVariation; } set { m_alphaVariation = value; } }
        public float AlphaMultiplayer { get { return m_alphaMultiplayer; } set { m_alphaMultiplayer = value; } }
        public Distribution ColorDistributionType { get { return m_colorDistributionType; } set { m_colorDistributionType = value; } }
        public Gradient Color { get { return m_color; } set { m_color = value; } }
        public float ColorVariation { get { return m_colorVariation; } set { m_colorVariation = value; } }
        public Texture2D Texture { get { return m_texture; } set { m_texture = value; } }
        public Preset OriginalPreset { get { return m_originalPreset; } set { m_originalPreset = value; } }
        public Material Material { get { return m_material; } set { m_material = value; } }
        public GalaxyPrefab GalaxyPrefab { get { return m_galaxyPrefab; } }
        #endregion

        #region enums
        public enum Preset
        {
            None,
            SmallStars,
            BigStars,
            Dust
        }

        public enum Distribution
        {
            Linear,
            Distance,
            Angle,
            Random,
        }
        #endregion
    }
}
