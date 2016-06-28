//#define HIDE_SUB_ASSETS
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

namespace Galaxia
{
    /// <summary>
    /// This is the component class that holds the galaxy meshes and handles the visualization of the particles.
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
        #endregion

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

        void LateUpdate()
        {
            if(m_needsUpdate || m_needsRebuild)
            {
                if(m_needsRebuild)
                {
                    Prefab.UpdateMaterial();
                    Build();
                    m_needsRebuild = false;
                }
                else if(m_needsUpdate)
                {
                    m_needsUpdate = false;
                }

                UpdateParticles();
            }
        }

        void OnRenderObject()
        {
            if (GalaxyPrefab != null && Galaxy != null && Camera.current && Camera.current.cullingMask == (Camera.current.cullingMask | (1 << gameObject.layer)))
            {
                DrawNow();
            }
        }


        /// <summary>
        /// Rebuilds the renderers with the generated mesh
        /// if the system doesn't not support geometry shades it will build it with unity's particle system
        /// </summary>
        internal void Build()
        {
            if (m_prefab != null && m_prefab.active && Galaxy != null)
            {
                if (!Galaxy.GPU)
                {
                    m_renderers = new GameObject[1];
                    GameObject g = new GameObject("Shuriken Renderer", typeof(ParticleSystem));
                    g.transform.parent = transform;
                    ParticleSystem system = g.GetComponent<ParticleSystem>();
                    system.maxParticles = m_prefab.Count;
                    system.playOnAwake = false;
                    Renderer renderer = system.GetComponent<Renderer>();
                    renderer.material = Resources.Load<Material>("Materials/ParticleSystemParticle");
                    renderer.material.mainTexture = m_prefab.Texture;
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = false;
                    system.SetParticles(ParticleList.Select(p => (ParticleSystem.Particle)p).ToArray(), m_particleList.Length);
                    system.Stop();
                    m_renderers[0] = g;
                }
            }
        }

		/// <summary>
		/// Used to forcefully update the particles
		/// </summary>
	    public void ForceUpdateParticles()
	    {
		    UpdateParticles();
	    }

        internal void UpdateParticles()
        {
            if (m_prefab != null)
            {
                UpdateRenderer();
                UpdateParticleList();

                if(m_gpu)
                    UpdateMeshes();
                else
                    UpdateShuriken();

                m_needsUpdate = false;
            }
            else
            {
                Debug.LogWarning("Prefab was deleted");
            }
        }

        /// <summary>
        /// Draws the generated particles meshes now.
        /// This is function <see cref="Graphics.DrawMeshNow"/>
        /// </summary>
        public void DrawNow()
        {
            if (m_gpu)
            {
                foreach (Mesh m in m_meshes)
                {
                    if (m == null || m_meshes == null)
                    {
                        UpdateMeshes();
                    }

                    if (m != null && m_prefab.active && m_prefab.Material != null)
                    {
                        if (m_prefab.Material.SetPass(0))
                        {
                            m_prefab.UpdateMaterial(m_galaxyPrefab);
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

                        Graphics.DrawMesh(m, transform.localToWorldMatrix, m_prefab.Material, 8);
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
            if (Galaxy.SupportsDirectX11)
                UpdateMeshesNormal();
            else
                UpdateMeshesBruteForce();
        }

        void UpdateMeshesNormal()
        {
            int meshCount = MeshCount(m_prefab.Count);
            UpdateMeshesBase(meshCount);

            Random.seed = m_prefab.Seed;

            for (int i = 0; i < m_meshes.Length; i++)
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
                    sheetPos[e].x = Random.Next(0, (int)Mathf.Pow(Prefab.TextureSheetPow, 2));
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
                    sheetPos[e].x = Random.Next(0, (int)Mathf.Pow(Prefab.TextureSheetPow, 2));
                    indexes[e] = e;
                }

                for (int e = 0; e < size; e+=4)
                {
                    int particleIndex = Mathf.FloorToInt((i * MAX_VERTEX_PER_MESH + e) / 4f);
                    normals[e] = new Vector3(-1,-1);
                    normals[e+1] = new Vector3(-1, 1);
                    normals[e+2] = new Vector3(1, 1);
                    normals[e+3] = new Vector3(1, -1);
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

        int UpdateMeshBase(int MeshIndex,int MeshCount,int ParicleCount)
        {
            int size = MAX_VERTEX_PER_MESH;
            if (MeshIndex == MeshCount - 1)
                size = ParicleCount - MAX_VERTEX_PER_MESH * MeshIndex;

            if (m_meshes[MeshIndex] == null)
            {
                m_meshes[MeshIndex] = new Mesh();
                m_meshes[MeshIndex].hideFlags = HideFlags.HideAndDontSave;
            }
            else if (m_meshes[MeshIndex].vertexCount > size)
            {
                m_meshes[MeshIndex].Clear(true);
            }

            return size;
        }

        void UpdateShuriken()
        {
	        if (Renderers == null) return;
            foreach(GameObject g in Renderers)
            {
                ParticleSystem system = g.GetComponent<ParticleSystem>();
                if (system != null)
                {
                    system.SetParticles(ParticleList.Select(p => (ParticleSystem.Particle)p).ToArray(), m_particleList.Length);
                }
            }
        }
        /// <summary>
        /// Update all parameters of the renderer
        /// </summary>
        void UpdateRenderer()
        {
	        if (Renderers == null || Prefab == null) return;
	        foreach (GameObject g in Renderers)
	        {
		        Renderer renderer = g.GetComponent<Renderer>();
		        if(renderer != null)
		        {
			        renderer.enabled = Prefab.active;
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
				if (m_particleList.Length != m_prefab.Count)
				{
					System.Array.Resize<Particle>(ref m_particleList, m_prefab.Count);
				}

				Random.seed = (int)(m_prefab.Seed);
				for (int i = 0; i < m_prefab.Count; i++)
				{
					m_particleList[i] = new Particle();
					m_particleList[i].index = i;
					m_galaxyPrefab.Distributor.Process(new ParticleDistributor.ProcessContext(m_particleList[i], m_galaxyPrefab, m_prefab, time, i));
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
            DestoryMeshes();
            DestroyRenderers();
            m_particleList = null;
            m_prefab = null;
            GameObject.DestroyImmediate(gameObject);
        }

        void DestroyRenderers()
        {
	        if (m_renderers == null) return;
	        foreach (GameObject renderer in m_renderers)
	        {
		        if (renderer != null)
		        {
			        //GameObject.DestroyImmediate(renderer.GetComponent<MeshFilter>().sharedMesh, true);
			        GameObject.DestroyImmediate(renderer);
		        }
	        }

	        m_renderers = null;
        }

        void DestoryMeshes()
        {
            if (m_meshes != null)
            {
                foreach (Mesh m in m_meshes)
                {
                    DestroyImmediate(m, true);
                }

                m_meshes = null;
            }
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
        public Galaxy Galaxy {
            get {
                if(transform.parent != null)
                {
                    return transform.parent.GetComponent<Galaxy>();
                }
                return null;
            }
        }
		/// <summary>
		/// The Time of the particles.
		/// Used manly for animations.
		/// </summary>
	    public float Time
	    {
		    get { return m_time; }
			set { m_time = value; }
	    }

	    #endregion
    }
}
