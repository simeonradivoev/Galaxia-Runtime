using UnityEngine;
using System.Collections;
using Galaxia;

[RequireComponent(typeof(Galaxy))]
public class GalaxyAnimationController : MonoBehaviour
{
	private Galaxy galaxy;
	[SerializeField] private float speed = 0.1f;

	private void Start()
	{
		galaxy = GetComponent<Galaxy>();
	}

	// Update is called once per frame
	private void Update ()
	{
		galaxy.SetParticlesTime(Time.time * speed);
		galaxy.UpdateParticlesImmediately();
    }
}
