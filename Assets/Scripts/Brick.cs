using UnityEngine;
using System.Collections;


/// <summary>
/// A class to control each brick
/// </summary>
public class Brick : MonoBehaviour {

	//----------------------------
	//  EDITOR PROPERTIES
	//----------------------------
	
	/// Points to be awarded when the brick is destroyed
	public int Points = 1;
	
	//TODO: Add a subtle particle to play whenever a brick is weakened
	/// A ParticleSystem to be instantiated when the brick is weakened
//	public GameObject WeakenEffect = null;
	
	/// A ParticleSystem to be instantiated when the brick is destroyed
	public GameObject DestroyEffect = null;
	
	/// An array of Materials to change this brick to, each time it's hit.
	public Material[] weakenedStates;
	
	//----------------------------
	//  FIELDS
	//----------------------------
	
	/// The number of times the brick must be hit before it can be destroyed,
	/// determined by how many weakened states have been specified
	int strength;
	
	/// True if this brick can be destroyed
	bool destroyable = true;
	
	
	//----------------------------------------------------
	
	/// <summary>
	/// Initialize this instance
	/// </summary>
	void Awake()
	{
		// The brick must be hit once for every weakened state, 
		// before hitting it again will destroy it.
		strength = weakenedStates.Length + 1;
	}
	
	//---------------------------------

	/// <summary>
	/// The brick has been hit!
	/// </summary>
	/// <param name="other">Other.</param>
	void OnCollisionEnter (Collision other)
	{
		if (!destroyable) return;
		
		// Brick has been hit by a ball
		if (--strength > 0) {
		
			// Weaken the brick!
			GetComponent<Renderer>().material = weakenedStates[strength - 1];
//			if (WeakenEffect != null) {
//				Instantiate(WeakenEffect, transform.position, Quaternion.identity);
//			}

		} else {
			
			// Destroy this brick!
			if (DestroyEffect != null) {
				Instantiate(DestroyEffect, transform.position, Quaternion.identity);
			}
			var row = GetComponentInParent<BrickRow>();
			row.BrickDestroyed();
			Game.s_Inst.AddToScore(Points);
			Destroy(gameObject);
		}
	}
	
	//---------------------------------
	
	/// <summary>
	/// Effect fired by Game.GameOver()
	/// </summary>
	public void Drop()
	{
		destroyable = false;
		var body = gameObject.AddComponent<Rigidbody>();
		body.angularVelocity = new Vector3(Random.Range(-20f,20f), Random.Range(-100f,100f), Random.Range(-50f,50f));
	}
	
	/// <summary>
	/// The brick has fallen to the bottom wall, as a result of the Drop() effect
	/// </summary>
	void OnTriggerEnter (Collider col)
	{
		// Destroy this brick, and subtract it from the row.
		var row = GetComponentInParent<BrickRow>();
		row.BrickDestroyed();
		Destroy(gameObject);
	}
	
}
