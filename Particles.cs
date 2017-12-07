// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
#define EDIT_RESOURCES
#define HIDE_SUB_ASSETS

using System;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Rendering;

namespace Galaxia
{
	/// <summary>
	/// This is the component class that holds the generated particles.
	/// It also hold the Unity's particle system object when CPU particles are active.
	/// This is the main connection between Galaxia and Unity.
	/// </summary>
	[System.Serializable]
    public sealed class Particles : MonoBehaviour
    {
        #region Constants
        /// <summary>
        /// The maximum amount of vertex per mesh.
        /// This is used to get around the limitation of Unity's meshes having a max vertex count.
        /// </summary>
        public const int MAX_VERTEX_PER_MESH = 40000;
        #endregion
        #region Private
        [SerializeField]
        [HideInInspector]
        private ParticlesPrefab m_prefab;
        [SerializeField]
        [HideInInspector]
        private Particle[] m_particleList;
        [SerializeField]
        [HideInInspector]
        private GameObject[] m_renderers;
        [SerializeField]
        [HideInInspector]
        private Mesh[] m_meshes;
        [SerializeField]
        [HideInInspector]
        private GalaxyPrefab m_galaxyPrefab;
        private bool m_gpu = true;
        private bool m_needsRebuild = true;
        private bool m_needsUpdate = true;
	    private float m_time = 0;
		[SerializeField, HideInInspector]
	    private Color m_overlayColor = Color.white;
	    private Material m_suruken_material;
	    private Bounds? m_renderBounds;
        #endregion

		internal void Init()
		{
			UpdateHideFlags();
		}

        /// <summary>
        /// Generate the particle data as well as the mesh
        /// </summary>
        /// <param name="Prefab">The Particle prefab to use</param>
        /// <param name="galaxy">The galaxy prefab</param>
        /// <param name="gpu">Should the generation be using the GPU acceleration</param>
        public void Generate(ParticlesPrefab Prefab, GalaxyPrefab galaxy,bool gpu)
        {
            this.m_prefab = Prefab;
            this.m_galaxyPrefab = galaxy;
            this.m_gpu = gpu;
            UpdateParticleList();
            UpdateMeshes();
        }

		[UsedImplicitly]
		private void OnEnable()
		{
			//force shrunken update
			if (!m_gpu)
				UpdateShuriken();
		}

        void LateUpdate()
        {
            if(m_needsUpdate || m_needsRebuild)
            {
                if(m_needsRebuild)
                {
                    Prefab.UpdateMaterial(m_overlayColor);
                    Build();
                    m_needsRebuild = false;
                }

                if(m_needsUpdate)
                {
                    m_needsUpdate = false;
                }

                UpdateParticles();
            }
        }

		/// <summary>
		/// Used for rendering Internally by the <see cref="Galaxia.Galaxy"/> Component
		/// It Does frustum culling if enabled
		/// </summary>
        internal void Render()
        {
	        if (GalaxyPrefab == null || Galaxy == null) return;
	        if (!Galaxy.GPU) return;;
			//Draw the galaxy and take into account it's gameObject layer
			//This allow for Camera layer culling to work properly
	        if (!Galaxy.RenderGalaxy || !Camera.current || Camera.current.cullingMask != (Camera.current.cullingMask | (1 << gameObject.layer))) return;
	        if (Galaxy.FrustumCulling)
	        {
		        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.current);
		        if (RenderBounds.HasValue)
		        {
			        if (GeometryUtility.TestPlanesAABB(planes, transform.TransformBounds(RenderBounds.Value)))
			        {
				        DrawNow();
			        }
					return;
		        }
	        }

			DrawNow();
		}

		private void DestoryOldRenderers()
		{
			if(m_renderers == null) return;
			foreach (var mRenderer in m_renderers)
			{
				DestroyImmediate(mRenderer);
			}
		}

        /// <summary>
        /// Rebuilds the renderers.
        /// if the system doesn't not support geometry shades it will build it with unity's particle system
        /// </summary>
        internal void Build()
        {
            if (m_prefab != null && m_prefab.active && Galaxy != null)
            {
                if (!Galaxy.GPU)
                {
					DestoryOldRenderers();
					m_renderers = new GameObject[1];
                    GameObject g = new GameObject("Shuriken Renderer", typeof(ParticleSystem));
                    g.transform.SetParent(transform);
                    ParticleSystem system = g.GetComponent<ParticleSystem>();
	                var main = system.main;
					main.maxParticles = m_prefab.Count;
					main.playOnAwake = false;
					main.startSpeed = 0;
	                main.prewarm = false;
	                main.loop = false;
					//emission Module
					ParticleSystem.EmissionModule emissionModule = system.emission;
	                emissionModule.enabled = false;
					//rendrer
					ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
                    renderer.material = Resources.Load<Material>("Materials/ParticleSystemParticle");
                    renderer.material.mainTexture = m_prefab.Texture;
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = false;
	                m_suruken_material = renderer.material;
	                var surukenParticles = ParticleList.Select(p => Particle.ConvertToParticleSystem(p, m_prefab.TextureSheetPow)).ToArray();
					system.SetParticles(surukenParticles, surukenParticles.Length);
					//multiplying it by 10 fixed the size differences between Unity's particle System and the Custom Mesh Particles
					renderer.maxParticleSize = m_prefab.MaxScreenSize * 10;
					//animation module
					ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = system.textureSheetAnimation;
					textureSheetAnimation.enabled = true;
                    textureSheetAnimation.numTilesX = m_prefab.TextureSheetPow;
					textureSheetAnimation.numTilesY = m_prefab.TextureSheetPow;
					system.Pause();
					m_renderers[0] = g;
                }
				//Generation of Mesh Renderer to use meshes on, makes the galaxy always visible behind scene objects
				//using Draw Now solves that Issue
				/*else
                {
					
					
	                m_renderers = new GameObject[m_meshes.Length];
	                for (int i = 0; i < m_meshes.Length; i++)
	                {
		                GameObject g = new GameObject("Mesh Renderer",new Type[] {typeof(MeshRenderer),typeof(MeshFilter)});
#if HIDE_SUB_ASSETS
						g.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
#endif
						Renderer renderer = g.GetComponent<Renderer>();
		                renderer.sharedMaterial = Prefab.Material;
		                MeshFilter meshFilter = g.GetComponent<MeshFilter>();
		                meshFilter.sharedMesh = m_meshes[i];
						g.transform.SetParent(transform,false);
		                m_renderers[i] = g;
	                }
			}*/
			}
		}

		private void UpdateHideFlags()
		{
			gameObject.hideFlags = Galaxy.SaveParticles ? HideFlags.None : HideFlags.DontSave;
#if HIDE_SUB_ASSETS
			gameObject.hideFlags |= HideFlags.HideInHierarchy;
#endif
#if EDIT_RESOURCES
			gameObject.hideFlags |= HideFlags.NotEditable;
#endif
		}

		/// <summary>
		/// Used to forcefully update the particles
		/// </summary>
		public void ForceUpdateParticles()
	    {
		    UpdateParticles();
	    }

		/// <summary>
		/// Used to update the prefab material used by the GPU particles.
		/// It also updates the material for the Unity particle system if CPU particles are enabled.
		/// </summary>
	    internal void UpdateMaterials()
		{
		    if (Prefab == null) return;
		    Prefab.UpdateMaterialAnimation(GalaxyPrefab, 0, false);
			Prefab.UpdateMaterial(GalaxyPrefab,m_overlayColor);
		    if (m_suruken_material)
		    {
			    m_suruken_material.color = OverlayColor * Prefab.ColorOverlay;
		    }
		}

		/// <summary>
		/// Updates all the particles by redistributing them.
		/// It also assign the particle data to the meshes or the Unity's particle system.
		/// </summary>
        internal void UpdateParticles()
        {
            if (m_prefab != null)
            {
                UpdateRenderer();
                UpdateParticleList();
				UpdateHideFlags();

				if (m_gpu)
                    UpdateMeshes();
                else
                    UpdateShuriken();

                m_needsUpdate = false;
            }
            else
            {
                Debug.LogWarning("Prefab was deleted");
	            if (Galaxy != null)
	            {
		            Destroy();
	            }
            }
        }

		/// <summary>
		/// Draws the generated particles meshes now.
		/// </summary>
		public void DrawNow()
        {
            if (m_gpu)
            {
	            if (m_meshes == null) return;
                foreach (Mesh m in m_meshes)
                {
                    if (m == null)
                    {
                        UpdateMeshes();
                    }

                    if (m != null && m_prefab.active && m_prefab.Material != null)
                    {
                        if (m_prefab.Material.SetPass(0))
                        {
                            Graphics.DrawMeshNow(m, transform.parent.localToWorldMatrix);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Queues the particle meshes for drawing next frame.
        /// This can be used anywhere.
        /// </summary>
        public void Draw()
        {
            if (m_gpu)
            {
	            if (m_meshes == null) return;
	            foreach (Mesh m in m_meshes)
	            {
		            if (m == null)
		            {
			            UpdateMeshes();
		            }

		            if (m != null && m_prefab.active)
		            {
			            //if(Galaxy.DirectX11)
			            //    Prefab.Material.shader = GalaxyPrefab.shader;
			            //else
			            //    Prefab.Material.shader = GalaxyPrefab.shaderCPU;

			            Graphics.DrawMesh(m, transform.localToWorldMatrix, m_prefab.Material, Galaxy.gameObject.layer);
		            }
	            }
            }
        }

		/// <summary>
		/// Draws the particle meshes to a given Command buffer.
		/// </summary>
		/// <param name="buffer">The Command buffer to draw into.</param>
		public void Draw(CommandBuffer buffer)
	    {
			if (m_gpu)
			{
				if (m_meshes == null)
					UpdateMeshes();

				foreach (Mesh m in m_meshes)
				{
					if (m == null)
					{
						UpdateMeshes();
					}

					if (m != null && m_prefab.active)
					{
						//if(Galaxy.DirectX11)
						//    Prefab.Material.shader = GalaxyPrefab.shader;
						//else
						//    Prefab.Material.shader = GalaxyPrefab.shaderCPU;

						buffer.DrawMesh(m,transform.localToWorldMatrix,m_prefab.Material,Galaxy.gameObject.layer);
					}
				}
			}
		}

        int MeshCount(int Count)
        {
            return Mathf.FloorToInt((float)Count / (float)MAX_VERTEX_PER_MESH) + 1;
        }

        int MeshCountCPU(int Count)
        {
            return Mathf.FloorToInt((float)(Count * 4) / (float)MAX_VERTEX_PER_MESH) + 1;
        }

        /// <summary>
        /// Updates the generated meshes with the information from the particle data list
        /// </summary>
        void UpdateMeshes()
        {
			//Destroys meshes if they are incompatible with Rendering API
			if(CalculateNeedsMeshRebuild())
				DestoryMeshes();

            if (Galaxy.SupportsDirectX11)
                UpdateMeshesNormal();
            else
                UpdateMeshesBruteForce();

	        CalculateBounds();
        }

		public bool CalculateNeedsUpdate()
		{
			if (CalculateNeedsMeshRebuild()) return true;
			if (m_meshes == null) return true;
			return m_meshes.Any(t => t == null);
		}

		/// <summary>
		/// Check if the generated meshes do not match the current rendering API
		/// Open GL and Non-DirectX11 meshes use the brute force and have a quad topology
		/// DirectX11 meshes use the normal (Geometry Shader) way and have a point topology
		/// </summary>
		/// <returns></returns>
		internal bool CalculateNeedsMeshRebuild()
		{
			if(!Galaxy.GPU) return false;
			if (m_meshes == null || CheckMeshesNulls() || m_meshes.Any(m => m.hideFlags != (Galaxy.SaveMeshes ? HideFlags.HideInInspector : HideFlags.HideAndDontSave))) return true;
			if (!Galaxy.SupportsDirectX11)
			{
				return m_meshes.Any(m => m.GetTopology(0) != MeshTopology.Quads);
			}

			return m_meshes.Any(m => m.GetTopology(0) != MeshTopology.Points);
		}

		private bool CheckMeshesNulls()
		{
			return m_meshes.Any(t => t == null);
		}

		/// <summary>
		/// Calculates the mesh or the Unity particle System bounds and stores them in a cache.
		/// It can be accessed by <see cref="Particles.RenderBounds"/>.
		/// </summary>
	    private void CalculateBounds()
	    {
		    m_renderBounds = null;
		    if (!Galaxy.GPU)
		    {
			    if (Renderers == null) return;
			    foreach (var r in Renderers)
			    {
					if (m_renderBounds == null)
					{
						m_renderBounds = r.GetComponent<Renderer>().bounds;
					}
					else
					{
						m_renderBounds.Value.Encapsulate(r.GetComponent<Renderer>().bounds);
					}
			    }
		    }
		    else
		    {
			    if (m_meshes == null) return;
				foreach (var mesh in m_meshes)
				{
					if (m_renderBounds == null)
					{
						m_renderBounds = mesh.bounds;
					}
					else
					{
						m_renderBounds.Value.Encapsulate(mesh.bounds);
					}
				}
			}
	    }

		/// <summary>
		/// This method updates the meshes by assigning points to the mesh.
		/// This is used when Geometry Shaders are enabled. The Geometry shader generates the camera facing quads.
		/// </summary>
        void UpdateMeshesNormal()
        {
            int meshCount = MeshCount(m_prefab.Count);
            UpdateMeshesBase(meshCount);

            Random.seed = m_prefab.Seed;

            for (int i = 0; i < meshCount; i++)
            {
                int size = UpdateMeshBase(i, meshCount, m_prefab.Count);

                Vector3[] vertex = new Vector3[size];
                Color[] color = new Color[size];
                Vector2[] info = new Vector2[size];
                Vector2[] sheetPos = new Vector2[size];
                int[] indexes = new int[size];

                for (int e = 0; e < size; e++)
                {
                    int particleIndex = i * MAX_VERTEX_PER_MESH + e;
                    vertex[e] = m_particleList[particleIndex].position;
                    color[e] = m_particleList[particleIndex].color;
                    info[e].x = m_particleList[particleIndex].size;
                    info[e].y = m_particleList[particleIndex].rotation;
                    sheetPos[e].x = Random.Next(0, Prefab.TextureSheetPow* Prefab.TextureSheetPow);
                    indexes[e] = e;
                }

                m_meshes[i].vertices = vertex;
                m_meshes[i].colors = color;
                m_meshes[i].uv = info;
                m_meshes[i].uv2 = sheetPos;
                m_meshes[i].SetIndices(indexes, MeshTopology.Points, 0);
                m_meshes[i].RecalculateBounds();
            }
        }

		/// <summary>
		/// This method generates quads and populates the mesh.
		/// It is used when Geometry shaders are not supported and quads cannot be generated by the GPU.
		/// </summary>
		void UpdateMeshesBruteForce()
        {
            int meshCount = MeshCountCPU(m_prefab.Count);
            UpdateMeshesBase(meshCount);

            Random.seed = m_prefab.Seed;

            for (int i = 0; i < meshCount; i++)
            {
                int size = UpdateMeshBase(i,meshCount,m_prefab.Count * 4);
                Vector3[] vertex = new Vector3[size];
                Color[] color = new Color[size];
                Vector2[] info = new Vector2[size];
                Vector2[] sheetPos = new Vector2[size];
                Vector3[] normals = new Vector3[size];
                int[] indexes = new int[size];

                for (int e = 0; e < size; e++)
                {
                    int particleIndex = Mathf.FloorToInt((i * MAX_VERTEX_PER_MESH + e) / 4f);
                    vertex[e] = m_particleList[particleIndex].position;
                    color[e] = m_particleList[particleIndex].color;
                    info[e].x = m_particleList[particleIndex].size;
                    info[e].y = m_particleList[particleIndex].rotation;
                    indexes[e] = e;
                }

                for (int e = 0; e < size; e+=4)
                {
	                int sheetIndex = Random.Next(0, Prefab.TextureSheetPow * Prefab.TextureSheetPow);
                    normals[e] = new Vector3(-1,-1);
                    normals[e+1] = new Vector3(-1, 1);
                    normals[e+2] = new Vector3(1, 1);
                    normals[e+3] = new Vector3(1, -1);
					sheetPos[e].x = sheetIndex;
					sheetPos[e + 1].x = sheetIndex;
					sheetPos[e + 2].x = sheetIndex;
					sheetPos[e + 3].x = sheetIndex;
				}

                m_meshes[i].vertices = vertex;
                m_meshes[i].colors = color;
                m_meshes[i].uv = info;
                m_meshes[i].uv2 = sheetPos;
                m_meshes[i].normals = normals;
                m_meshes[i].SetIndices(indexes, MeshTopology.Quads, 0);
                m_meshes[i].RecalculateBounds();
            }
        }

		/// <summary>
		/// This is a utility method that makes sure the meshes array is not empty and has enough space.
		/// </summary>
		/// <param name="MeshCount"></param>
        void UpdateMeshesBase(int MeshCount)
        {
            if (m_meshes == null)
            {
                m_meshes = new Mesh[0];
            }

            if(MeshCount != m_meshes.Length)
            {
                System.Array.Resize<Mesh>(ref m_meshes,MeshCount);
            }
        }


		/// <summary>
		/// This is a utility method that initializes meshes if they are <c>null</c>.
		/// </summary>
		/// <param name="meshIndex">The index of the mesh.</param>
		/// <param name="meshCount">The total mesh count.</param>
		/// <param name="paricleCount">The total particle count.</param>
		/// <returns></returns>
        int UpdateMeshBase(int meshIndex,int meshCount,int paricleCount)
        {
            int size = MAX_VERTEX_PER_MESH;
            if (meshIndex == meshCount - 1)
                size = paricleCount - MAX_VERTEX_PER_MESH * meshIndex;

            if (m_meshes[meshIndex] == null)
            {
                m_meshes[meshIndex] = new Mesh();
			}
            else if (m_meshes[meshIndex].vertexCount > size)
            {
                m_meshes[meshIndex].Clear(true);
            }

			m_meshes[meshIndex].hideFlags = Galaxy.SaveMeshes ? HideFlags.HideInInspector : HideFlags.HideAndDontSave;

			return size;
        }

		/// <summary>
		/// Cleans (Destroys) old meshes.
		/// </summary>
	    internal void CleanMeshes()
	    {
		    m_renderBounds = null;
			//clear old meshes that are no longer needed when using the Unity Particle System
			if (m_meshes != null && (m_meshes != null || m_meshes.Length > 0))
			{
				DestoryMeshes();
			}
		}

		/// <summary>
		/// Updates the Unity Particle System with the generated particle data.
		/// </summary>
        void UpdateShuriken()
        {
	        if (Renderers == null) return;
            foreach(GameObject g in Renderers)
            {
                ParticleSystem system = g.GetComponent<ParticleSystem>();
                system?.SetParticles(ParticleList.Select(p => Particle.ConvertToParticleSystem(p,m_prefab.TextureSheetPow)).ToArray(), m_particleList.Length);
            }
			CalculateBounds();
        }

        /// <summary>
        /// Update all parameters of the renderer
        /// </summary>
        internal void UpdateRenderer()
        {
	        if (Renderers == null || Prefab == null) return;
	        foreach (GameObject g in Renderers)
	        {
		        Renderer renderer = g.GetComponent<Renderer>();
		        if(renderer != null)
		        {
			        renderer.enabled = Prefab.active && Galaxy.RenderGalaxy;
		        }
	        }
        }

	    /// <summary>
		/// Update the particle data list without destroying it
		/// Resizes the array as needed
		/// </summary>
		/// <param name="time">The global time of the particles. This is mainly used for animations.</param>
		void UpdateParticleList(float time)
	    {
			if (m_particleList == null)
			{
				m_particleList = new Particle[m_prefab.Count];
			}

			if (m_galaxyPrefab.Distributor != null && m_prefab != null)
			{
				bool processFlag = m_galaxyPrefab.Distributor.CanProcess(m_prefab);

				if (m_particleList.Length != m_prefab.Count)
				{
					System.Array.Resize<Particle>(ref m_particleList, m_prefab.Count);
				}

				Random.seed = (int)(m_prefab.Seed);
				for (int i = 0; i < m_prefab.Count; i++)
				{
					m_particleList[i] = new Particle
					{
						index = i,
						sheetPosition = Random.Next(0, m_prefab.TextureSheetPow * m_prefab.TextureSheetPow)
					};
					if(processFlag) m_galaxyPrefab.Distributor.Process(new ParticleDistributor.ProcessContext(m_particleList[i], m_galaxyPrefab, m_prefab, time, i));
				}
			}
			else
			{
				if (m_galaxyPrefab.Distributor == null)
					Debug.LogWarning("No Particle Distributor");
				if (m_prefab == null)
					Debug.LogWarning("No Particle Distributor");
			}
		}

        /// <summary>
        /// Update the particle data list without destroying it
        /// Resizes the array as needed
        /// </summary>
        void UpdateParticleList()
        {
            UpdateParticleList(m_time);
        }

        /// <summary>
        /// Destroys all particle meshes and all renderers as well as the Game Object the component is attached to.
        /// </summary>
        public void Destroy()
        {
	        var galaxy = Galaxy;
			if(galaxy != null) galaxy.RemoveParticles(this);
			DestoryMeshes();
            DestroyRenderers();
            m_particleList = null;
            m_prefab = null;
            GameObject.DestroyImmediate(gameObject);
        }

		/// <summary>
		/// Destroys all renderers
		/// </summary>
        void DestroyRenderers()
        {
	        if (m_renderers == null) return;
	        foreach (GameObject r in m_renderers.Where(r => r != null))
	        {
		        GameObject.DestroyImmediate(r);
	        }

	        m_renderers = null;
        }

		/// <summary>
		/// Destroys all mesh data.
		/// </summary>
        void DestoryMeshes()
        {
			if (m_meshes == null) return;
			foreach (Mesh m in m_meshes)
			{
				DestroyImmediate(m, true);
			}

			m_meshes = null;
        }

#region Getters and setters
        /// <summary>
        /// The Particles Prefab
        /// </summary>
        public ParticlesPrefab Prefab { get { return m_prefab; } }
        /// <summary>
        /// The list of Particles
        /// </summary>
        public Particle[] ParticleList { get { return m_particleList; } }
        /// <summary>
        /// The list of Game object renderers.
        /// These are the Game objects that have mesh renderers attached to them.
        /// They are used when in-game to render particle meshes more efficiently.
        /// </summary>
        public GameObject[] Renderers { get { return m_renderers; } }
        /// <summary>
        /// All the Particle meshes. Multiple meshes can be generated because of Unity's mesh vertex cap.
        /// </summary>
        public Mesh[] Meshes { get { return m_meshes; } }
        /// <summary>
        /// Shows if the Particles Prefab has changed. And the particles need to be rebuilt.
        /// </summary>
        public bool NeedsRebuild { get { return m_needsRebuild; } set { m_needsRebuild = value; } }
        /// <summary>
        /// Shows if the articles Prefab has changed. And the particles need to be updated.
        /// </summary>
        public bool NeedsUpdate { get { return m_needsUpdate; } set { m_needsUpdate = value; } }
        /// <summary>
        /// The Galaxy Prefab
        /// </summary>
        public GalaxyPrefab GalaxyPrefab { get { return m_galaxyPrefab; } set { m_galaxyPrefab = value; } }
        /// <summary>
        /// The Galaxy Component of the parent.
        /// The Particles component is attached to children objects added to the Main Game Object with the Galaxy component.
        /// The Children objects are hidden from the Unity's inspector.
        /// </summary>
        public Galaxy Galaxy => transform.parent?.GetComponent<Galaxy>();

		/// <summary>
		/// The Time of the particles.
		/// Used manly for animations.
		/// </summary>
	    public float Time
	    {
		    get { return m_time; }
			set { m_time = value; }
	    }

		/// <summary>
		/// The Overlay Color of the Particles. This color will be multiplied with the overlay color of the Particle Prefab
		/// </summary>
	    public Color OverlayColor
	    {
		    get { return m_overlayColor; }
			set { m_overlayColor = value; }
	    }

		/// <summary>
		/// The render bounds of the galaxy. This could be generated by the meshes of GPU particles or Unity's Particle system if CPU particles are implemented.
		/// </summary>
	    public Bounds? RenderBounds
	    {
		    get
		    {
			    if (m_renderBounds == null)
			    {
				    CalculateBounds();
			    }
			    return m_renderBounds; 
			    
		    }
	    }

#endregion
    }
}
