using SharpNeatLib.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Galaxia
{
    public static class Random
    {
        static FastRandom m_generator;
        static FastRandom Generator { get { if (m_generator == null) m_generator = new FastRandom(); return m_generator; } }

        public static int seed { get { return Generator.Seed; } set { Generator.Reinitialise(value); } }

        public static float Next()
        {
            return (float)Generator.NextDouble();
        }

        public static float Next(float min,float max)
        {
            return (Next() * (max - min)) + min;
        }

        public static int Next(int Max)
        {
            return Generator.Next(Max);
        }

        public static int Next(int min,int max)
        {
            return Generator.Next(min, max);
        }
    }
}
