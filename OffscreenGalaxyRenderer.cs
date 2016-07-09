// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using UnityEngine;
using UnityEngine.Rendering;

namespace Galaxia
{
	/// <summary>
	/// Renders the Galaxy and all it's particles as an Image Effect. Allowing downsampling of particles for overdrawing and performance.
	/// </summary>
	/// <remarks>
	/// This Component must be attached to a Camera. It works much the same as an Image Effect.
	/// Galaxy Rendering will be canceled on the Galaxy Component, no manual disabling of rendering is required.
	/// Depth not yet implemented!
	/// </remarks>
	[RequireComponent(typeof(Camera))]
	public sealed class OffscreenGalaxyRenderer : GalaxyRenderer
	{
		[SerializeField] private int globalDownsample = 1;
		[SerializeField] private bool hdr = true;
		private Camera outputCamera;
		private Camera hiddenCamera;
		private Shader compositeShader;
		private Material compositeMaterial;
		private Particles currentRenderingParticles;

		/// <summary>
		/// MonoBehaviour call
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			Camera.onPostRender += OnPostRenderCustom;
		}

		/// <summary>
		/// MonoBehaviour call
		/// </summary>
		private void Start()
		{
			outputCamera = GetComponent<Camera>();
			compositeShader = Shader.Find("Hidden/Off-Screen Particles Composite");
			if (compositeShader == null)
			{
				Debug.LogError("Could not find Off-Screen Particles Composite Shader");
				return;
			}
			compositeMaterial = new Material(compositeShader) { hideFlags = HideFlags.HideAndDontSave };
			InitHiddenCamera();
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			if (compositeMaterial != null && Galaxy != null && outputCamera != null && this.outputCamera != null && hiddenCamera != null)
			{
				CopyCamera(outputCamera, hiddenCamera);
                hiddenCamera.hdr = hdr;

				RenderTexture particlesRenderTexture = RenderTexture.GetTemporary(outputCamera.pixelWidth / globalDownsample, outputCamera.pixelHeight / globalDownsample, 8);
				ClearRenderTexture(particlesRenderTexture, Color.black);
				Vector2 texelOffset = Vector2.Scale(src.GetTexelOffset(), new Vector2(src.width, src.height));
				foreach (var particle in Galaxy.Particles)
				{
					RenderTexture temp = RenderTexture.GetTemporary(particlesRenderTexture.width / particle.Prefab.Downsample, particlesRenderTexture.height / particle.Prefab.Downsample, 8);
					ClearRenderTexture(temp, Color.black);
                    temp.DiscardContents(true, true);
					hiddenCamera.targetTexture = temp;
					currentRenderingParticles = particle;
					hiddenCamera.Render();
					Graphics.BlitMultiTap(temp, particlesRenderTexture, compositeMaterial, texelOffset);
					RenderTexture.ReleaseTemporary(temp);
				}

				hiddenCamera.targetTexture = null;
				Graphics.Blit(particlesRenderTexture, src, compositeMaterial);
				RenderTexture.ReleaseTemporary(particlesRenderTexture);
			}

			Graphics.Blit(src, dest);
        }

		private void InitHiddenCamera()
		{
			GameObject hiddenCameraObj = new GameObject("Hidden Galaxy Camera", typeof(Camera));
			hiddenCamera = hiddenCameraObj.GetComponent<Camera>();
			hiddenCamera.enabled = false;
			hiddenCamera.cullingMask = 0;
			hiddenCamera.clearFlags = CameraClearFlags.SolidColor;
			hiddenCamera.backgroundColor = Color.black;
			hiddenCamera.depthTextureMode = DepthTextureMode.None;
			hiddenCamera.hdr = hdr;
			hiddenCameraObj.hideFlags = HideFlags.HideAndDontSave;
		}

		private static void CopyCamera(Camera from, Camera to)
		{
			to.nearClipPlane = from.nearClipPlane;
			to.farClipPlane = from.farClipPlane;
			to.fieldOfView = from.fieldOfView;
			to.transform.position = from.transform.position;
			to.transform.rotation = from.transform.rotation;
		}

		private void ClearRenderTexture(RenderTexture renderTexture,Color color)
		{
			RenderTexture lastRenderTex = RenderTexture.active;
			RenderTexture.active = renderTexture;
			GL.Clear(true,true, color);
			RenderTexture.active = lastRenderTex;
		}

		private void OnPostRenderCustom(Camera camera)
		{
			if (compositeMaterial == null || hiddenCamera != camera) return;
			currentRenderingParticles?.DrawNow();
		}

		/// <summary>
		/// MonoBehaviour call
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			if(Camera.onPostRender != null) Camera.onPostRender -= OnPostRenderCustom;
		}

		private void OnDestory()
		{
			if(hiddenCamera) Destroy(hiddenCamera.gameObject);
			if(compositeMaterial) Destroy(compositeMaterial);
		}

		/// <summary>
		/// Should the galaxy be rendered in HDR
		/// </summary>
		public bool Hdr
		{
			get { return hdr; }
			set { hdr = value; }
		}

		/// <summary>
		/// How much downsample should be applied globally to all particles.
		/// </summary>
		public int GlobalDownsample
		{
			get { return globalDownsample; }
			set { globalDownsample = value; }
		}

		/// <summary>
		/// The Output Camera
		/// </summary>
		public Camera OutputCamera
		{
			get { return outputCamera; }
		}
	}
}