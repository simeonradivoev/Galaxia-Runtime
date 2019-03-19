using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SystemGUICircle : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler {

    public float Size;
    public bool Over;
    bool visible;
    GameObject[] planets;

	public void SetPlanets (int num) 
    {
        planets = new GameObject[num];

        for (int i = 0; i < num; i++)
        {
            GameObject g = new GameObject("Planet", typeof(Image));
            Image Image = g.GetComponent<Image>();
            Image.transform.transform.SetParent(transform, false);
            Image.rectTransform.sizeDelta = Vector3.one * Size * 0.3f;
            float angle = ((Mathf.PI * 2) / (float)num) * (float)i;
            Image.rectTransform.localPosition = new Vector3(Mathf.Sin(angle) * (Size / 1.7f), Mathf.Cos(angle) * (Size / 1.7f), 0);
            Image.sprite = GetComponent<Image>().sprite;
            Image.color = GetComponent<Image>().color;
            planets[i] = g;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(Over)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 3 * Size,0.1f);
            for (int i = 0; i < planets.Length;i++ )
            {
                float angle = ((Mathf.PI * 2) / (float)planets.Length) * (float)i + Time.time;
                planets[i].transform.localPosition = new Vector3(Mathf.Sin(angle) * (Size / 1.7f), Mathf.Cos(angle) * (Size / 1.7f), 0);
            }
        }else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * Size, 0.1f);
        }
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        Over = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Over = false;
    }
}
