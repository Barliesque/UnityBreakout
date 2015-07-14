using UnityEngine;
using System.Collections;


/// <summary>
/// Auto destroy ParticleSystem when the effect has completed.
/// </summary>
public class ParticlesAutoDestroy : MonoBehaviour
{
	ParticleSystem ps;

	void Awake()
	{
		ps = GetComponent<ParticleSystem>();
	}

	void LateUpdate()
	{
		if (ps !=null) {
			if (!ps.IsAlive()) {
				Destroy(gameObject);
			}
		}
	}
}
