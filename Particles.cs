//#define HIDE_SUB_ASSETS
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace Galaxia
{
    [System.Serializable]
    public sealed class Particles : MonoBehaviour
    {
        #region Constants
        public const int MAX_VERTEX_PER_MESH = 65000;
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
        #endregion

        /// <summary>
        /// Generate the particle data as well as the mesh
        /// </summary>
        /// <param name="Prefab">The Particle prefab to use</param>
        /// <param name="galaxy">The galaxy prefab</param>
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
            if(m_needsRebuild)
            {
                if (m_prefab != null && m_galaxyPrefab != null)
                {
                    Build(m_gpu);
                }
            }

            if(m_needsUpdate)
            {
                UpdateParticles();
            }
        }


        /// <summary>
        /// Rebuilds the renderers with the generated mesh
        /// if the system doesn not support geometry shaders it will build it with unity's particle system
        /// </summary>
        /// <param name="galaxy">The galaxy prefab</param>
        /// <param name="directx11">if you want to render in directx 11 or not</param>
        internal void Build(bool directx11)
        {
            if (m_prefab != null && m_prefab.active)
            {
                //only do geometry shader particles on direct x 10 and above
                if (directx11 && SystemInfo.graphicsShaderLevel >= 40)
                {
                    if (m_meshes != null)
                    {
                        DestroyRenderers();
                        m_renderers = new GameObject[m_meshes.Length];
                        m_prefab.UpdateMaterial(m_galaxyPrefab);

                        for (int i = 0; i < m_meshes.Length; i++)
                        {
                            GameObject g = new GameObject("Renderer", typeof(MeshRenderer), typeof(MeshFilter));
                            #if HIDE_SUB_ASSETS
                            g.hideFlags = HideFlags.HideInHierarchy;
                            #endif
                            //g.hideFlags |= HideFlags.DontSave;
                            g.transform.parent = transform;
                            g.GetComponent<MeshFilter>().sharedMesh = m_meshes[i];
                            g.renderer.sharedMaterial = m_prefab.Material;
                            g.renderer.castShadows = false;
                            g.renderer.receiveShadows = false;

                            m_renderers[i] = g;
                        }

                        
                    }
                }
                else
                {
                    m_renderers = new GameObject[1];
                    GameObject g = new GameObject("Shuriken Renderer",typeof(ParticleSystem));
                    g.transform.parent = transform;
                    ParticleSystem system = g.GetComponent<ParticleSystem>();
                    system.maxParticles = m_prefab.Count;
                    system.playOnAwake = false;
                    system.renderer.material = Resources.Load<Material>("Materials/ParticleSystemParticle");
                    system.renderer.material.mainTexture = m_prefab.Texture;
                    system.renderer.castShadows = false;
                    system.renderer.receiveShadows = false;
                    system.SetParticles(ParticleList.Select(p => (ParticleSystem.Particle)p).ToArray(), m_particleList.Length);
                    system.Stop();
                    m_renderers[0] = g;
                }

                m_needsRebuild = false;
            }
        }

        internal void UpdateParticles()
        {
            if (m_prefab != null)
            {
                if (m_meshes != null && m_meshes.Length == MeshCount(m_prefab.Count))
                {
                    UpdateRenderer();
                    UpdateParticleList();
                    if (Galaxy.DirectX11)
                    {
                        UpdateMeshes();
                    }
                    else
                    {
                        UpdateShuriken();
                    }
                    
                }
                else
                {
                    Generate(m_prefab, m_galaxyPrefab,m_gpu);
                }

                m_needsUpdate = false;
            }
            else
            {
                Debug.LogWarning("Prefab was deleted");
            }
        }

        internal void UpdateParticles_MT(object galaxyObj)
        {
            GalaxyPrefab galaxy = galaxyObj as GalaxyPrefab;

            if (m_meshes != null && m_meshes.Length == MeshCount(m_prefab.Count))
            {
                UpdateParticleList();
                UpdateMeshes();
            }
            else
            {
                //Generate(Prefab, galaxy);
            }
        }

        public void DrawNow()
        {
            foreach(Mesh m in m_meshes)
            {
                if (m_prefab.active && m_prefab.Material != null)
                {
                    if (m_prefab.Material.SetPass(0))
                    {
                        m_prefab.UpdateMaterial(m_galaxyPrefab);
                        Graphics.DrawMeshNow(m, transform.parent.localToWorldMatrix);
                    }
                }
            }
        }

        public void Draw()
        {
            foreach (Mesh m in m_meshes)
            {
                if (m_prefab.active)
                {
                    Graphics.DrawMesh(m, transform.localToWorldMatrix, m_prefab.Material, 0);
                }
            }
        }

        int MeshCount(int Count)
        {
            return Mathf.FloorToInt((float)Count / (float)MAX_VERTEX_PER_MESH) + 1;
        }

        /// <summary>
        /// Updates the generated meshes with the information from the particle data list
        /// </summary>
        /// <param name="galaxy">The galaxy prefab</param>
        void UpdateMeshes()
        {
            if (m_meshes == null)
            {
                m_meshes = new Mesh[0];
            }

            if (m_meshes.Length != MeshCount(m_prefab.Count))
            {
                System.Array.Resize<Mesh>(ref m_meshes, MeshCount(m_prefab.Count));
            }

            Random.seed = m_prefab.Seed;

            for (int i = 0; i < m_meshes.Length; i++)
            {
                int size = MAX_VERTEX_PER_MESH;
                if (i == m_meshes.Length - 1)
                    size = m_prefab.Count - MAX_VERTEX_PER_MESH * i;

                if (m_meshes[i] == null)
                {
                    m_meshes[i] = new Mesh();
                    m_meshes[i].hideFlags = HideFlags.HideAndDontSave;
                }
                else if (m_meshes[i].vertexCount > size)
                {
                    m_meshes[i].Clear(true);
                }

                Vector3[] vertex = new Vector3[size];
                Color[] color = new Color[size];
                Vector2[] info = new Vector2[size];
                Vector2[] sheetPos = new Vector2[size];
                int[] indexes = new int[size];

                for (int e = 0; e < size; e++)
                {
                    vertex[e] = m_particleList[i * MAX_VERTEX_PER_MESH + e].position;
                    color[e] = m_particleList[i * MAX_VERTEX_PER_MESH + e].color;
                    info[e].x = m_particleList[i * MAX_VERTEX_PER_MESH + e].size;
                    info[e].y = m_particleList[i * MAX_VERTEX_PER_MESH + e].rotation;
                    sheetPos[e].x = Random.Next(0, (int)Mathf.Pow(Prefab.TextureSheetPow, 2));
                    indexes[e] = e;
                }

                m_meshes[i].vertices = vertex;
                m_meshes[i].colors = color;
                m_meshes[i].uv = info;
                m_meshes[i].uv1 = sheetPos;
                m_meshes[i].SetIndices(indexes, MeshTopology.Points, 0);
                m_meshes[i].RecalculateBounds();


            }
        }

        void UpdateShuriken()
        {
            foreach(GameObject g in Renderers)
            {
                if(g.particleSystem != null)
                {
                    g.particleSystem.SetParticles(ParticleList.Select(p => (ParticleSystem.Particle)p).ToArray(), m_particleList.Length);
                }
            }
        }
        /// <summary>
        /// Update all parameters of the renderer
        /// </summary>
        void UpdateRenderer()
        {
            if (Renderers != null && Prefab != null)
            {
                foreach (GameObject g in Renderers)
                {
                    g.renderer.enabled = Prefab.active;
                }
            }
        }

        /// <summary>
        /// Update the particle data list without destroying it
        /// Resizes the array as needed
        /// </summary>
        /// <param name="galaxy">The galaxy prefab</param>
        void UpdateParticleList()
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
                    m_galaxyPrefab.Distributor.Process(new ParticleDistributor.ProcessContext(m_particleList[i], m_galaxyPrefab, m_prefab, 0, i));
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
            if (m_renderers != null)
            {
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
        public ParticlesPrefab Prefab { get { return m_prefab; } }
        public Particle[] ParticleList { get { return m_particleList; } }
        public GameObject[] Renderers { get { return m_renderers; } }
        public Mesh[] Meshes { get { return m_meshes; } }
        public bool NeedsRebuild { get { return m_needsRebuild; } set { m_needsRebuild = value; } }
        public bool NeedsUpdate { get { return m_needsUpdate; } set { m_needsUpdate = value; } }
        public GalaxyPrefab GalaxyPrefab { get { return m_galaxyPrefab; } set { m_galaxyPrefab = value; } }
        public Galaxy Galaxy { get { return transform.parent.GetComponent<Galaxy>(); } }
        #endregion
    }
}
