// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using UnityEngine;

namespace Galaxia
{
    /// <summary>
    /// Stores default values for Curves used in the Galaxy Prefab
    /// </summary>
    public static class DefaultResources
    {
        public static Gradient StarColorGradient
        {
            get
            {
	            Gradient g = new Gradient
	            {
		            colorKeys = new GradientColorKey[]
		            {
			            new GradientColorKey(new Color32(245, 237, 236, 255), 0),
			            new GradientColorKey(new Color32(237, 170, 91, 255), 0.198f),
			            new GradientColorKey(new Color32(148, 175, 236, 255), 0.607f),
			            new GradientColorKey(new Color32(136, 176, 254, 255), 1)
		            }
	            };
	            return g;
            }
        }

        public static Gradient AngleColorGradient
        {
            get
            {
	            Gradient g = new Gradient
	            {
		            colorKeys = new GradientColorKey[]
		            {
			            new GradientColorKey(new Color32(0, 0, 0, 255), 0),
			            new GradientColorKey(new Color32(0, 0, 0, 255), 0.355f),
			            new GradientColorKey(new Color32(255, 0, 0, 255), 0.5f),
			            new GradientColorKey(new Color32(0, 0, 0, 255), 0.658f),
			            new GradientColorKey(new Color32(0, 0, 0, 255), 1)
		            }
	            };
	            return g;
            }
        }

        public static AnimationCurve AlphaCurve
        {
            get
            {
	            AnimationCurve g = new AnimationCurve
	            {
		            keys = new Keyframe[]
		            {
			            new Keyframe(0.0008992806f, 1.000408f, -5.090356f, -5.090356f),
			            new Keyframe(0.01340023f, 0.9367739f, -2.968448f, -2.968448f),
			            new Keyframe(0.02689196f, 0.9076897f, -0.4536995f, -0.4536995f),
			            new Keyframe(0.1230904f, 0.851472f, -0.2048324f, -0.2048324f),
			            new Keyframe(0.1477445f, 0.8311051f, -2.438177f, -2.438177f),
			            new Keyframe(0.9980599f, 0.2003719f, -0.01887445f, -0.03320185f)
		            }
	            };
	            return g;
            }
        }

        public static AnimationCurve SpeedCurve
        {
            get
            {
	            AnimationCurve g = new AnimationCurve
	            {
		            keys = new Keyframe[]
		            {
			            new Keyframe(0.004496403f, 0.1028679f, 2f, 2f),
			            new Keyframe(0.03126431f, 0.886272f, 1.851171f, 1.851171f),
			            new Keyframe(0.2146933f, 0.9149699f, -0.1125879f, -0.1125879f),
			            new Keyframe(0.5727876f, 0.9286684f, -0.1552161f, -0.1552161f),
			            new Keyframe(1.002698f, 0.9114413f, 0f, 0f)
		            }
	            };
	            return g;
            }
        }

        public static AnimationCurve HeightCurve
        {
            get
            {
	            AnimationCurve g = new AnimationCurve
	            {
		            keys = new Keyframe[]
		            {
			            new Keyframe(-8.086581E-05f, 0.9473922f, -0.9130224f, -0.9130224f),
			            new Keyframe(0.03777196f, 0.7948538f, -6.380379f, -6.380379f),
			            new Keyframe(0.1750727f, 0.446302f, -0.5989536f, -0.5989536f),
			            new Keyframe(0.9987727f, 0.04905841f, -0.3058203f, -0.3058203f)
		            }
	            };
	            return g;
            }
        }

        public static Texture2D StarTexture
        {
            get
            {
                return Resources.Load<Texture2D>("Textures/Particles/StarParticle");
            }
        }

        public static Texture2D DustTexture
        {
            get
            {
                return Resources.Load<Texture2D>("Textures/Particles/DustParticle");
            }
        }
    }
}
