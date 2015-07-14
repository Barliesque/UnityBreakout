using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A class to manage a row of Bricks.
/// </summary>
public class BrickRow : MonoBehaviour {

	//----------------------------
	//  EDITOR PROPERTIES
	//----------------------------
	
	// Ball speed increase, when this row is completed
	public float ballSpeedIncrease = 4.0f;
	
	//----------------------------
	//  FIELDS
	//----------------------------
	
	/// The number of bricks currently remaining in this row
	int brickCount;
	
	
	//----------------------------------------------------
	
	/// <summary>
	/// Initialize this instance
	/// </summary>
	void Awake () {
		// Count bricks in this row
		brickCount = gameObject.GetComponentsInChildren<Brick>().Length;
	}
	
	/// <summary>
	/// A brick in this row has been destroyed.
	/// </summary>
	public void BrickDestroyed()
	{
		// Has the entire row been destroyed?
		if (--brickCount == 0) {
		
			// Increase the ball speed
			Ball.velocity += ballSpeedIncrease;
			
			// Remove this row from the hierarchy
			Destroy(gameObject);
			
			// Inform the Game that this row has been destroyed
			Game.inst.RowDestroyed();
		}
	}
	
}
