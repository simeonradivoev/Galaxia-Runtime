using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galaxia
{
    public class ImageDistributor : ParticleDistributor
    {
        public Texture2D DistributionMap;
        public Texture2D ColorMap;
        public Texture2D HeightMap;

        public AnimationCurve heightDistribution;
        public float maxHeight = 10;
        [SerializeField]
        [HideInInspector]
        private AnimationCurve[] cy;
        [SerializeField]
        [HideInInspector]
        private AnimationCurve cx;
        [Range(0,1)]
        public float ColorContribution = 1;

        public override void Process(ProcessContext context)
        {
            int x = Mathf.Clamp(Mathf.FloorToInt(cx.Evaluate(Random.Next())), 0, (cy.Length-1));
            int y = Mathf.FloorToInt(cy[x].Evaluate(Random.Next()));

            float xStepSize = 1f / (float)DistributionMap.width;
            float yStepSize = 1f / (float)DistributionMap.height;
            float xPos = (float)x / (float)DistributionMap.width;
            float yPos = (float)y / (float)DistributionMap.height;

            Vector3 _pos = (new Vector3(-0.5f, 0, -0.5f) + new Vector3(xPos + Random.Next() * xStepSize, 0, yPos + Random.Next() * yStepSize));
            float distance = Vector3.Distance(Vector3.zero,_pos);
            _pos *= context.galaxy.Size * 2f;
            _pos.y = (float)Random.NextGaussianDouble(1) * maxHeight;
            _pos.y *= HeightMap.GetPixel(x,y).grayscale;

            ProcessProperties(context, _pos, 0);

            if (ColorMap != null)
                context.particle.color *= Color.Lerp(Color.white, ColorMap.GetPixelBilinear(xPos, yPos), ColorContribution);

            context.particle.position = _pos;
        }

        private void AnalizeImage()
        {
            float[,] samples = new float[DistributionMap.width, DistributionMap.height];
            cy = new AnimationCurve[DistributionMap.width];
            cx = new AnimationCurve();

            for(int x = 0;x < DistributionMap.width;x++)
            {
                for (int y = 0; y < DistributionMap.height-1; y++)
                {
                    samples[x, y] = DistributionMap.GetPixel(x, y).grayscale;
                }
            }

            for (int x = 0; x < DistributionMap.width; x++)
            {
                for (int y = 1; y < DistributionMap.height; y++)
                {
                    samples[x, y] += samples[x, y - 1];
                }
            }

            float max = float.Epsilon;
            for (int x = 1; x < DistributionMap.width; x++)
            {
                samples[x, DistributionMap.height - 1] += samples[x - 1, DistributionMap.height - 1];
                max = Mathf.Max(max, samples[x, DistributionMap.height - 1]);
            }

            for (int x = 1; x < DistributionMap.width; x++)
            {
                cx.AddKey(new Keyframe(samples[x, DistributionMap.height - 1] / max, x, 0, 0));
            }

            for (int x = 0; x < DistributionMap.width; x++)
            {
                List<Keyframe> keys = new List<Keyframe>();
                max = float.Epsilon;
                for (int y = 0; y < DistributionMap.height; y++)
                {
                    max = Mathf.Max(max,samples[x, y]);
                }

                for (int y = 0; y < DistributionMap.height-1; y++)
                {
                    keys.Add(new Keyframe(samples[x, y] / max, y, 0, 0));
                }

                cy[x] = new AnimationCurve(keys.ToArray());
                cy[x].postWrapMode = WrapMode.Loop;
            }
        }

        public override void RecreateCurves()
        {
            base.RecreateCurves();
            if (DistributionMap != null)
            {
                try
                {
                    AnalizeImage();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public override void UpdateMaterial(Material material)
        {
            
        }
    }
}
