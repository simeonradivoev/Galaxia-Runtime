// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Galaxia
{
    /// <summary>
    /// The Component that holds and manages visualization of <see cref="Galaxia.GalaxyPrefab"/>
    /// </summary>
    public sealed class Galaxy : MonoBehaviour
    {
        #region Public
        #endregion
        #region Private
        [SerializeField]
        [HideInInspector]
        private List<Particles> particles;
        [SerializeField,HideInInspector]
        private GalaxyGenerationType m_generationType;
        [SerializeField,HideInInspector]
        private bool m_gpu = true;
	    [SerializeField,HideInInspector] private bool m_render_galaxy = true;
	    [SerializeField, HideInInspector] private bool m_frustumCulling = true;
        [SerializeField]
        private GalaxyPrefab m_galaxy;
		[SerializeField,HideInInspector]
		private bool m_saveMeshes;
		[SerializeField, HideInInspector]
		private bool m_saveParticles;
		#endregion
		#region Events
		private RenderEventHandler m_preRenderHandler;
		private RenderEventHandler m_postRenderHandler;
	    private RenderEvent lastPreRenderEvent = new RenderEvent(false); 
		#endregion
		#region Constructors
		void OnEnable()
        {
			SmartParticleInitialization();

			if (m_preRenderHandler == null)
				m_preRenderHandler = new RenderEventHandler();
			if (m_postRenderHandler == null)
				m_postRenderHandler = new RenderEventHandler();
		}
        #endregion
        #region Methods
        #region Particle Generation and Update

		/// <summary>
		/// Initializes or updates the galaxy and each of it's particles only when necessary.
		/// </summary>
	    public void SmartParticleInitialization()
	    {
		    if (particles == null)
		    {
			    GenerateParticles();
				return;
		    }

			//if the particles components were destroyed without us knowing
		    if (particles.Any(t => !t))
		    {
			    GenerateParticles();
			    return;
		    }

		    foreach (var particle in particles)
		    {
			    particle.NeedsRebuild = !m_gpu;
			    particle.NeedsUpdate = particle.CalculateNeedsUpdate();
		    }
	    }

		/// <summary>
		/// Generates <see cref="Galaxia.Particles"/> to all the <see cref="Galaxia.ParticlesPrefab"/> in the galaxy
		/// </summary>
		/// <remarks>
		/// This function only generates the particles, not the Meshes
		/// </remarks>
		public void GenerateParticles()
        {
            DestroyParticles();

	        if (!GalaxyPrefab) return;
	        foreach (ParticlesPrefab prefab in GalaxyPrefab.Where(prefab => prefab != null))
	        {
		        GenerateParticles(prefab);
	        }
        }

        /// <summary>
        /// Same as the <see cref="Galaxia.Galaxy.GenerateParticles()"/> but for a specific <see cref="Galaxia.ParticlesPrefab"/>
        /// </summary>
        /// <param name="prefab">The Particles Prefab to use for the generation</param>
        public void GenerateParticles(ParticlesPrefab prefab)
        {
            if (GalaxyPrefab != null)
            {
                if (prefab != null)
                {
                    GameObject obj = new GameObject(prefab.name, typeof(Particles));
					obj.transform.SetParent(transform);
                    Particles p = obj.GetComponent<Particles>();
					p.Init();
                    p.Generate(prefab, GalaxyPrefab, GPU);
                    particles.Add(p);
                    prefab.CreateMaterial(GalaxyPrefab, GPU);
                    prefab.UpdateMaterial(GalaxyPrefab);
                }
                else
                {
                    Debug.LogError("Particle Prefab was destroyed or missing");
                }
            }else
            {
                Debug.LogWarning("No Prefab assigned");
            }
        }
        /// <summary>
        /// Destroys all the <see cref="Galaxia.Particles"/> components in the Galaxy.
        /// </summary>
        public void DestroyParticles()
        {
            //Debug.Log("Particles Cleared");

            if (particles != null)
            {
				//the particles remove themselves from the galaxy
	            for (int i = particles.Count - 1; i >= 0; i--)
	            {
		            particles[i]?.Destroy();
				}

	            particles.Clear();

                foreach(Transform t in transform)
                {
                    DestroyImmediate(t.gameObject);
                }
            }
            else
            {
                particles = new List<Particles>();
            }
        }
        /// <summary>
        /// Destroys all <see cref="Galaxia.Particles"/> with a given prefab.
        /// </summary>
        /// <param name="prefab">particle prefab to search for</param>
        public void DestroyParticles(ParticlesPrefab prefab)
        {
            if (particles != null)
            {
	            //the particles remove themselves from the galaxy
				for (int i = particles.Count-1; i >= 0; i--)
	            {
					if (!particles[i] || particles[i].Prefab != prefab) continue;
		            particles[i].Destroy();
				}
            }
            else
            {
                particles = new List<Particles>();
            }
        }

	    internal void RemoveParticles(Particles item)
	    {
		    particles?.Remove(item);
	    }

		/// <summary>
		/// Marks all the <see cref="Galaxia.Particles"/> for Update, next frame.
		/// </summary>
		public void UpdateParticles()
        {
			GalaxyPrefab?.Distributor.RecreateCurves();

			foreach (Particles particle in particles)
            {
                if(particle && GalaxyPrefab != null)
                {
                    particle.NeedsUpdate = true;
                } 
            }
        }

		/// <summary>
		/// Forces all <see cref="Galaxia.Particles"/> to update Immediately
		/// </summary>
		public void UpdateParticlesImmediately()
        {
			GalaxyPrefab?.Distributor.RecreateCurves();
			if(GalaxyPrefab == null) return;
	        ForceUpdateParticleComponents();
        }

	    private void ForceUpdateParticleComponents()
	    {
			//particles components may remove themselves if invalid, that's why we need backwards iteration 
		    for (int i = particles.Count - 1; i >= 0; i--)
		    {
			    particles[i]?.UpdateParticles();
		    }
		}

		/// <summary>
		/// Marks a <see cref="Galaxia.Particles"/> with a given <see cref="Galaxia.ParticlesPrefab"/> for Update, next frame.
		/// </summary>
		/// <param name="prefab">The <see cref="Galaxia.ParticlesPrefab"/> to search for</param>
		public void UpdateParticles(ParticlesPrefab prefab)
        {
            foreach (Particles particle in particles)
            {
                if (particle && m_galaxy != null && particle.Prefab == prefab)
                {
                    particle.NeedsUpdate = true;
                }
            }
        }

		/// <summary>
		/// Forces all <see cref="Galaxia.Particles"/> with a given <see cref="Galaxia.ParticlesPrefab"/> to update Immediately.
		/// </summary>
		/// <param name="prefab">The <see cref="Galaxia.ParticlesPrefab"/> to search for</param>
		public void UpdateParticlesImmediately(ParticlesPrefab prefab)
        {
	        //particles components may remove themselves if invalid, that's why we need backwards iteration 
			for (int i = particles.Count - 1; i >= 0; i--)
	        {
		        if (particles[i] && m_galaxy != null && particles[i].Prefab == prefab)
		        {
			        particles[i].UpdateParticles();
		        }
	        }
        }

        #endregion
        void Update()
        {
            if (GalaxyPrefab != null)
            {
                foreach (Particles particle in particles)
                {
                    if (particle)
                    {
                        particle.UpdateMaterials();
                        GalaxyPrefab.Distributor.UpdateMaterial(particle.Prefab.Material);
                    }
                    else
                    {
                        Debug.LogWarning("Particle Component was destroyed");
						GenerateParticles();
						return;
                    }
                    
                }
            }
        }

		/// <summary>
		/// Called by unity when the object is drawn by any camera.
		/// </summary>
		/// <remarks>
		/// This method is the main rendering method for the galaxy.
		/// It will not be called if the component is disabled or the Pre render event is used.
		/// </remarks>
		private void OnRenderObject()
	    {
			if (!isActiveAndEnabled) return;
		    foreach (var particle in particles)
		    {
				if(!particle) continue;
				lastPreRenderEvent.Used = false;
                m_preRenderHandler.Invoke(particle, lastPreRenderEvent);
			    if (!lastPreRenderEvent.Used)
			    {
					particle.Render();
				}
		    }
		}

		/// <summary>
		/// Draws the Galaxy and all the <see cref="Galaxia.Particles"/> in the Galaxy Now
		/// </summary>
		/// <remarks>
		/// <example>
		/// <code>
		/// private void OnRenderObject()
		/// {
		///    foreach (var particle in particles)
		///    {
		///		particle.Render();
		///    }
		///	}
		///}
		/// </code>
		/// </example>
		/// </remarks>
		/// 
		public void DrawNow()
        {
            if (GalaxyPrefab != null && GPU)
            {
                foreach (Particles particle in particles)
                {
                    if(particle)
                    {
						particle.UpdateMaterials();

                        if (particle.Prefab != null)
                            particle.DrawNow();
                    }
                }
            }
        }
        /// <summary>
        /// Sends the Galaxy and all the <see cref="Galaxia.Particles"/> for Rendering
        /// </summary>
        public void Draw()
        {
            if (GalaxyPrefab != null && GPU)
            {
                foreach (Particles particle in particles)
                {
                    if (particle && particle.Prefab != null)
                        particle.Draw();
                }
            }
        }

	    /// <summary>
	    /// </summary>
	    private void OnDrawGizmosSelected()
	    {
			Bounds? bounds = null;
			foreach (var particle in particles)
			{
				if(!particle) continue;
				Bounds? particleBounds = particle.RenderBounds;
				if (!particleBounds.HasValue) continue;
				if (!bounds.HasValue)
				{
					bounds = particleBounds;
				}
				else
				{
					bounds.Value.Encapsulate(particleBounds.Value);
				}
			}
			if (bounds.HasValue)
			{
				bounds = transform.TransformBounds(bounds.Value);
				Gizmos.color = new Color(0.7f,0.8f,1,0.5f);
                Gizmos.DrawWireCube(bounds.Value.center, bounds.Value.size);
			}
		}

	    /// <summary>
	    /// </summary>
	    private void OnDrawGizmos()
	    {
		    if (!Application.isPlaying && m_render_galaxy)
		    {
				DrawNow();
			}
	    }

		#endregion
		#region Getters And Setters
		/// <summary>
		/// A Utility function for getting the position of the <see cref="Galaxia.Galaxy"/>.
		/// </summary>
		public Vector3 Position { get { return transform.position; } set { transform.position = value; } }
        /// <summary>
        /// List of <see cref="Galaxia.Particles"/> that holds the Components responsible for the individual visualization of the <see cref="Galaxia.ParticlesPrefab"/>.
        /// </summary>
        public List<Particles> Particles { get { return particles; } }
		/// <summary>
		/// This dictates if the <see cref="Galaxia.Galaxy"/> updates automatically when a Property is changed.
		/// If it is set to manual, <see cref="Galaxia.Galaxy.GenerateParticles()"/> or <see cref="Galaxia.Galaxy.UpdateParticles(ParticlesPrefab)"/> must be called every time the galaxy is changed.
		/// </summary>
		public GalaxyGenerationType GenerationType { get { return m_generationType; } set { m_generationType = value; } }
        /// <summary>
        /// The meat of the galaxy. This is a holder for galaxy specific particle properties.
        /// It holds a list of <see cref="Galaxia.ParticlesPrefab"/> as well as the <see cref="Galaxia.ParticleDistributor"/>.
        /// </summary>
        public GalaxyPrefab GalaxyPrefab
        {
            get { return m_galaxy; }
            set
            {
                if(m_galaxy != value && m_generationType == GalaxyGenerationType.Automatic)
                {
                    m_galaxy = value;
                    GenerateParticles();
                }
                else
                {
                    m_galaxy = value;
                }
            }
        }
        /// <summary>
        /// Returns if DirectX 11 is supported on the software.
        /// If it is available you can disable it from here.
        /// </summary>
        [Obsolete("New Function is GPU")]
        public bool DirectX11
        {
            get { return GPU; }
            set
            {
                GPU = value;
            }
        }

        /// <summary>
        /// Will use Custom particles, rendered on the GPU
        /// if not, it will use the Unity Particle System
        /// </summary>
        public bool GPU
        {
            get { return m_gpu; }
            set
            {
                if (m_gpu != value)
                {
                    m_gpu = value;
	                if (!m_gpu)
	                {
		                foreach (var particle in particles)
		                {
			                particle?.CleanMeshes();
		                }
	                }
                    if (m_generationType == GalaxyGenerationType.Automatic)
                        GenerateParticles();
                }
            }
        }

		/// <summary>
		/// Should the Generated meshes be saved in the scene or prefab.
		/// This only works with GPU particles. As they generated meshes.
		/// </summary>
	    public bool SaveMeshes
	    {
		    get { return m_saveMeshes; }
		    set
		    {
			    if (m_saveMeshes == value) return;
			    m_saveMeshes = value;
			    ForceUpdateParticleComponents();
		    }
	    }

		/// <summary>
		/// Should the Generated Particles be saved in the scene or prefab.
		/// This works with all particles, as it saves the data.
		/// </summary>
		public bool SaveParticles
		{
			get { return m_saveParticles; }
			set
			{
				if (m_saveParticles == value) return;
				m_saveParticles = value;
				ForceUpdateParticleComponents();
			}
		}

		/// <summary>
		/// Should the galaxy be rendered. This disables the automatic rendering of the galaxy and does not disable the <see cref="Galaxia.Galaxy.Draw()"/> and <see cref="Galaxia.Galaxy.DrawNow()"/> methods.
		/// </summary>
		public bool RenderGalaxy
	    {
		    get { return m_render_galaxy; }
		    set
		    {
			    if (m_render_galaxy == value) return;
			    m_render_galaxy = value;
				ForceUpdateParticleComponents();
			}
	    }

		/// <summary>
		/// Enable FrustumCulling.
		/// This determines if the galaxy is rendered when outside the camera's frustum. This only applies to the GPU particles and not the Unity ones.
		/// </summary>
	    public bool FrustumCulling
	    {
		    get { return m_frustumCulling; }
			set { m_frustumCulling = value; }
	    }

		/// <summary>
		/// Returns the last pre-render event
		/// </summary>
	    public RenderEvent LastPreRenderEvent => lastPreRenderEvent;

	    /// <summary>
        /// Checks if the currently utilized Graphics API is OpenGL instead of DirectX
        /// </summary>
        public static bool OpenGL => SystemInfo.graphicsDeviceVersion.Contains("OpenGL");

	    /// <summary>
        /// Checks if the DirectX is the current Graphics API and if so then is it >= than DX11
        /// </summary>
        public static bool SupportsDirectX11 => !OpenGL && SystemInfo.graphicsShaderLevel >= 40;

		/// <summary>
		/// A utility method for setting the time of all <see cref="Galaxia.Particles"/> on the <see cref="Galaxia.Galaxy"/>.
		/// Used mainly for animations.
		/// </summary>
		/// <param name="time">The time of the particles.</param>
		public void SetParticlesTime(float time)
		{
			foreach (var particle in particles)
			{
				if(particle)
					particle.Time = time;
			}
		}

		/// <summary>
		/// On Pre Render Event Handler. This is called before each <see cref="Galaxia.Particles"/> is rendered.
		/// If this event is used then the <see cref="Galaxia.Particles"/> won't be rendered.
		/// </summary>
		public RenderEventHandler OnPreRender => m_preRenderHandler ?? (m_preRenderHandler = new RenderEventHandler());

		/// <summary>
		/// On Post Render Event Handler. This is called after each <see cref="Galaxia.Particles"/> is rendered.
		/// </summary>
		public RenderEventHandler OnPostRender => m_postRenderHandler ?? (m_postRenderHandler = new RenderEventHandler());

		#endregion
		#region enums
		/// <summary>
		/// Used for specifying the Galaxy Particle Generation Type.
		/// </summary>
		public enum GalaxyGenerationType
        {
            /// <summary>
            /// generates particles when the Galaxy Prefab properties change.
            /// </summary>
            Automatic,
            /// <summary>
            /// used to manually generate particles.
            /// </summary>
            Manual
        }
        #endregion

		/// <summary>
		/// Galaxy Render Event
		/// </summary>
		public class RenderEvent
		{
			/// <summary>
			/// Mark the render event as Used. This will make the galaxy skip it's own rendering.
			/// Good for handling galaxy rendering elsewhere
			/// </summary>
			public bool Used;

			public RenderEvent(bool used)
			{
				Used = used;
			}
		}

		/// <summary>
		/// Render Event Implementation
		/// </summary>
		public class RenderEventHandler : UnityEvent<Particles, RenderEvent>
		{
			 
		}
    }
}
