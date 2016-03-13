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

	// Height of the bricks in this row.  Used by Game.cs to assemble wall of bricks.  Use a value larger than the height of the bricks to create a gap below this row.
	public float RowHeight = 1.5f;

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
			Game.s_Inst.RowDestroyed();
		}
	}
	
}
