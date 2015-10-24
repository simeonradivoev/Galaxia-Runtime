using UnityEngine;

namespace Galaxia
{
    [System.Serializable]
    public sealed class DistributionProperty
    {
        [SerializeField]
        private DistrbuitionType m_type;
        [SerializeField]
        [CurveRange(0,0,1,1)]
        private AnimationCurve m_distributionCurve;
        [SerializeField]
        [Range(0, 1)]
        private float m_variation = 0;
        [SerializeField]
        [Range(0, 1)]
        private float m_multiplayer = 1;
        [SerializeField]
        private float m_frequncy = 1;
        [SerializeField]
        private float m_amplitude = 1;

        public DistributionProperty(DistrbuitionType type,AnimationCurve distributionCurve, float variation, float multiplayer)
        {
            m_type = type;
            m_distributionCurve = distributionCurve;
            m_variation = variation;
            m_multiplayer = multiplayer;
        }

        public DistrbuitionType Type { get { return m_type; } set { m_type = value; } }
        public AnimationCurve DistributionCurve { get { return m_distributionCurve; } set { m_distributionCurve = value; } }
        public float Variation { get { return m_variation; } set { m_variation = value; } }
        public float Multiplayer { get { return m_multiplayer; } set { m_multiplayer = value; } }
        public float Frequency { get { return m_frequncy; } set { m_frequncy = value; } }
        public float Amplitude { get { return m_amplitude; } set { m_amplitude = value; } }
    }
}
