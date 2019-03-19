using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Galaxia;
using UnityEngine.UI;
using System.Linq;

public class StarSystemManager : MonoBehaviour {

    public Galaxy galaxy;
    public StarFinder finder;
    public Sprite SystemTexture;
    public float SystemGUISize = 64;
    [SerializeField]
    [HideInInspector]
    public List<GameObject> Systems;

    void Start()
    {
        Galaxia.Random.seed = galaxy.Particles[0].Prefab.Seed;
        Systems = new List<GameObject>();
        List<Galaxia.Particle> Stars = finder.Find(galaxy.Particles[0], Names.Length);
        List<string> names = Names.OrderBy(n => Galaxia.Random.Next()).ToList();
        
        for (int i = 0; i < Stars.Count;i++ )
        {
            int planets = Galaxia.Random.Next(0, 5);
            GameObject system = new GameObject("System", typeof(RectTransform), typeof(Image));
            system.GetComponent<RectTransform>().sizeDelta = Vector3.one * SystemGUISize;
            system.transform.position = Stars[i].position;
            system.transform.rotation = Quaternion.AngleAxis(90, new Vector3(1, 0, 0));
            Image Image = system.GetComponent<Image>();
            Image.sprite = SystemTexture;
            //Image.material = mat;
            //Image.color = new Color(Stars[i].color.r, Stars[i].color.g, Stars[i].color.b, 1);
            SystemGUICircle gui = system.AddComponent<SystemGUICircle>();
            gui.Size = SystemGUISize;
            system.transform.SetParent(transform, true);
            gui.SetPlanets(planets);
            StarSystem starSystem = system.AddComponent<StarSystem>();
            starSystem.Name = i < names.Count ? names[i] : "Unknown";
            starSystem.Planets = planets;
            Systems.Add(system);
        }
    }

    string[] Names = new string[]
    {
        "Albireo","Cygnus X-1","16 Cygni","Kelu-1","Pismis 24-1","Castor","40 Eridani","Polaris","Solaris","TV Crateris","Alpha Centauri","Anus","Avalon","Darwin IV","Far Away","Gaia","Gor","Hydros",
        "K-PAX","Kerbin","Krull","LittleBigPlanet","LV-1201","LV-426","New Terra","Omicron Persei 8","Pandora","Hoth","Planet X","Ragnarok","Rosetta","Terminus","Titan","Zeist","Mustafar"
    };
}
