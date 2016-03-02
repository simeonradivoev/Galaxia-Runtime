using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Galaxia
{
    /// <summary>
    /// The star Finder helper component.
    /// Used to find particular stars.
    /// </summary>
    public class StarFinder : MonoBehaviour
    {
        #region Private
        [SerializeField]
        private int m_separation = 0;
        [SerializeField]
        private Distribution m_distribution;
        private List<int> randomIndexes;
        #endregion

        public List<Particle> Find(Particles particles, int count)
        {
            return Find(particles, DefaultChooser, m_distribution, count);
        }

        public List<Particle> Find(Particles particles, Distribution distribution, int count)
        {
            return Find(particles, DefaultChooser, distribution, count);
        }

        public List<Particle> Find(Particles particles, Chooser chosser,Distribution distribution, int count)
        {
            if(count <= particles.ParticleList.Length )
            {
                List<Particle> chosenParticles = new List<Particle>();

                SetUpIncrement(distribution, particles, count);

                for (int i = 0; i < particles.ParticleList.Length; i += GetIncrement(distribution, particles, count))
                {
                    int index = GetIndex(distribution,particles,count,i);

                    if (index < particles.ParticleList.Length && chosenParticles.Count < count)
                    {
                        if (chosser(particles.ParticleList[index], index))
                        {
                            chosenParticles.Add(particles.ParticleList[index]);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return chosenParticles;
            }
            else
            {
                Debug.LogError("Not enough stars in galaxy");
                return new List<Particle>();
            }
            
        }

        void SetUpIncrement(Distribution distribution,Particles particles,int count)
        {
            switch (distribution)
            {
                case Distribution.Random:
                    if (particles.Prefab != null)
                        Random.seed = particles.Prefab.Seed;

                    randomIndexes = Enumerable.Range(0, particles.ParticleList.Length).OrderBy(r => Random.Next()).ToList();
                    break;
            }
        }

        int GetIncrement(Distribution distribution,Particles particles,int count)
        {
            switch(distribution)
            {
                case Distribution.Linear:
                    return m_separation;
                case Distribution.Spread:
                    return Mathf.CeilToInt((float)particles.ParticleList.Length / (float)count);
                default:
                    return 1;
            }
        }

        int GetIndex(Distribution distribution, Particles particles, int count,int index)
        {
            switch (distribution)
            {
                case Distribution.Random:
                    return randomIndexes[index];
                default:
                    return index;
            }
        }

        bool DefaultChooser(Particle particle, int index)
        {

            return true;
        }

        public delegate bool Chooser(Particle particle,int index);
        public enum Distribution
        {
            Spread,
            Linear,
            Random
        }
    }
}
