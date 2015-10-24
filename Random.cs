using SharpNeatLib.Maths;
using System;

namespace Galaxia
{
    public static class Random
    {
        const double TWO_PI = 6.2831853071795864769252866;
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

        public static double NextGaussianDouble(double variance)
        {
	        double rand1, rand2;

	        rand1 = Generator.NextDouble();
	        if(rand1 < 1e-100) rand1 = 1e-100;
	        rand1 = -2 * Math.Log(rand1);
            rand2 = Generator.NextDouble() * TWO_PI;
 
	        return Math.Sqrt(variance * rand1) * Math.Cos(rand2);
        }
    }
}
