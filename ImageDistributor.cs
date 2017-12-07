// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galaxia
{
	/// <summary>
	/// This is the Image Distribution Algorithm.
	/// It uses Images to distribute stars based on it's grayscale values.
	/// It Generates Inverse Integrals for probability distributon.
	/// </summary>
    public class ImageDistributor : ParticleDistributor
    {
		/// <summary>
		/// The distribution Map's grayscale values are used as probabilities for distribution.
		/// </summary>
        [SerializeField,HideInInspector] private Texture2D DistributionMap;
		/// <summary>
		/// The Color Map is used to determine the particle's color.
		/// </summary>
        [SerializeField, HideInInspector] private Texture2D ColorMap;
		/// <summary>
		/// The Heightmap is used to control the height destribution of the particles.
		/// </summary>
        [SerializeField, HideInInspector] private Texture2D HeightMap;

        public AnimationCurve heightDistribution;
        [Delayed] public float maxHeight = 10;
        [SerializeField]
        [HideInInspector]
        private AnimationCurve[] cy;
        [SerializeField]
        [HideInInspector]
        private AnimationCurve cx;
        [Delayed,Range(0,1)]
        public float ColorContribution = 1;
		[SerializeField,HideInInspector]
		private int m_distributonDownsample = 1;

	    /// <summary>
	    /// <see cref="Galaxia.ParticleDistributor.CanProcess"/>
	    /// </summary>
	    /// <param name="particles"></param>
	    /// <returns></returns>
		public override bool CanProcess(ParticlesPrefab particles)
	    {
		    if (DistributionMap == null)
		    {
			    Debug.LogWarning("No Distribution Map", this);
				return false;
		    }
		    return true;
	    }

	    /// <summary>
		/// Used by the Particle Generator to modify/distribute the particles to a desired shape.
		/// This is where particles are processed one by one.
		/// <see cref="Galaxia.ParticleDistributor.ProcessContext"/>
		/// </summary>
		/// <param name="context">The context holds information on the current particle and Galaxy Object.</param>
		public override void Process(ProcessContext context)
        {
	        if (DistributionMap == null) return;
	        if (cy == null || cx == null) return;

            int x = Mathf.Clamp(Mathf.FloorToInt(cx.Evaluate(Random.Next())), 0, (cy.Length-1));
            int y = Mathf.FloorToInt(cy[Mathf.Clamp(x,0,cy.Length-1)].Evaluate(Random.Next()));

	        int width = DistributionMap.width / m_distributonDownsample;
	        int height = DistributionMap.height / m_distributonDownsample;

			float xStepSize = 1f / width;
            float yStepSize = 1f / height;
            float xPos = (float)x / width;
            float yPos = (float)y / height;

            Vector3 _pos = (new Vector3(-0.5f, 0, -0.5f) + new Vector3(xPos + Random.Next() * xStepSize, 0, yPos + Random.Next() * yStepSize));
            float distance = Vector3.Distance(Vector3.zero,_pos);
            _pos *= context.galaxy.Size * 2f;
            _pos.y = (float)Random.NextGaussianDouble(1) * maxHeight;
	        if (HeightMap != null)
				_pos.y *= HeightMap.GetPixelBilinear(x, y).grayscale;

			ProcessProperties(context, _pos, 0);

            if (ColorMap != null)
                context.particle.color *= Color.Lerp(Color.white, ColorMap.GetPixelBilinear(xPos, yPos), ColorContribution);

            context.particle.position = _pos;
        }

        private void AnalizeImage()
        {
	        if (DistributionMap == null || !CanReadFromTexture(DistributionMap))
	        {
		        cy = null;
		        cx = null;
				return;
	        }

	        int width = DistributionMap.width / Mathf.Max(m_distributonDownsample,1);
	        int height = DistributionMap.height / Mathf.Max(m_distributonDownsample, 1);

			float[,] samples = new float[width, height];
            cy = new AnimationCurve[width];
            cx = new AnimationCurve();

            for(int x = 0;x < width; x++)
            {
                for (int y = 0; y < height-1; y++)
                {
                    samples[x, y] = DistributionMap.GetPixelBilinear((float)x / width, (float)y / width).grayscale;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    samples[x, y] += samples[x, y - 1];
                }
            }

            float max = float.Epsilon;
            for (int x = 1; x < width; x++)
            {
                samples[x, height - 1] += samples[x - 1, height - 1];
                max = Mathf.Max(max, samples[x, height - 1]);
            }

            for (int x = 1; x < width; x++)
            {
                cx.AddKey(new Keyframe(samples[x, height - 1] / max, x, 0, 0));
            }

            for (int x = 0; x < width; x++)
            {
                List<Keyframe> keys = new List<Keyframe>();
                max = float.Epsilon;
                for (int y = 0; y < height; y++)
                {
                    max = Mathf.Max(max,samples[x, y]);
                }

                for (int y = 0; y < height-1; y++)
                {
                    keys.Add(new Keyframe(samples[x, y] / max, y, 0, 0));
                }

	            cy[x] = new AnimationCurve(keys.ToArray()) {postWrapMode = WrapMode.Loop};
            }
        }

		/// <summary>
		/// Used to bake the integral curves used for image distribution.
		/// </summary>
        public override void RecreateCurves()
        {
            base.RecreateCurves();
			try
			{
				AnalizeImage();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

        public override void UpdateMaterial(Material material)
        {
            
        }

		private bool CanReadFromTexture(Texture2D texture)
		{
			if (!texture) return false;
			try
			{
				texture.GetPixelBilinear(0,0);
			}
			catch (UnityException e)
			{
				if (e.Message.StartsWith("Texture '" + texture.name + "' is not readable"))
				{
					Debug.LogError("Read/write on texture [" + texture.name + "] must be enabled!");
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Sets the Distribution map. This map controls the distribution of the particles.
		/// </summary>
		/// <param name="texture"></param>
		public void SetDistributionMap(Texture2D texture)
		{
			if (DistributionMap != texture)
			{
				DistributionMap = texture;
				RecreateCurves();
                GalaxyPrefab.RecreateAllGalaxies();
			}
		}

		/// <summary>
		/// Sets the Color Map. This map controls the color of the particles.
		/// </summary>
		/// <param name="texture"></param>
		public void SetColorMap(Texture2D texture)
		{
			if (ColorMap != texture)
			{
				ColorMap = texture;
				GalaxyPrefab.RecreateAllGalaxies();
			}
		}

		/// <summary>
		/// Sets the Height Map. This map controls the height of the particles.
		/// </summary>
		/// <param name="texture"></param>
		public void SetHeightMap(Texture2D texture)
		{
			if (HeightMap != texture)
			{
				HeightMap = texture;
				GalaxyPrefab.RecreateAllGalaxies();
			}
		}

		/// <summary>
		/// The Distribution Downsample. This controls at what resolution is the Distribution Map sampled.
		/// </summary>
		/// <remarks>
		/// Lower values mean higher resolution, but lower performance.
		/// Higher values mean lower resolution, but higher performance.
		/// </remarks>
		public int DistributonDownsample
		{
			get { return m_distributonDownsample; }
			set
			{
				if (m_distributonDownsample == value) return;
				m_distributonDownsample = value;
				RecreateCurves();
				GalaxyPrefab.RecreateAllGalaxies();
			}
		}

		/// <summary>
		/// Get The currently used distribution map
		/// </summary>
		/// <returns></returns>
	    public Texture2D GetDistributionMap()
	    {
		    return DistributionMap;
	    }

		/// <summary>
		/// Get the currently used color map
		/// </summary>
		/// <returns></returns>
	    public Texture2D GetColorMap()
	    {
		    return ColorMap;
	    }

		/// <summary>
		/// Get the currently used height map
		/// </summary>
		/// <returns></returns>
	    public Texture2D GetHeightMap()
	    {
		    return HeightMap;
	    }
    }
}
