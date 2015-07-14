using UnityEngine;
using System.Collections;

/// <summary>
/// A class to control the player's paddle, which is 
/// never destroyed for the life of the application.
/// </summary>
public class Paddle : MonoBehaviour {

	
	//----------------------------
	//  EDITOR PROPERTIES
	//----------------------------
	
	/// A particle effect played when the paddle shrinks
	public GameObject ShrinkEffect;
	
	/// The Ball Prefab
	public GameObject BallPrefab;
	
	/// A speed limit for the movement of the paddle
	public float maxSpeed = 0.75f;
	
	//----------------------------
	//  FIELDS
	//----------------------------
	
	/// Cached position of the paddle
	Vector3 pos;
	
	/// Maximum distance from the center the paddle may move
	float maxPos = 13f;
	
	/// The paddle's shape in the physics simulation
	CapsuleCollider capsule;
	
	/// Center mesh of the paddle capsule shape
	GameObject cylinder;
	
	/// Left end mesh of the paddle capsule shape
	GameObject capLeft;
	
	/// Right end mesh of the paddle capsule shape
	GameObject capRight;
	
	/// Paddle's size at the start of a game
	public const float DEFAULT_SIZE = 6f;
	
	/// Paddle's size when it shrinks
	public const float SMALL_SIZE = 4f;
	
	/// The paddle's current size
	float _size;
	public float Size {
		get { return _size; }
		set { setSize(value); }
	}
	
	//----------------------------------------------------
	
	/// <summary>
	/// Initialize the paddle
	/// </summary>
	void Start ()
	{
		// Copy initial paddle position, set in the editor
		pos = transform.position;
		
		// Find paddle components
		capsule = GetComponent<CapsuleCollider>();
		cylinder = GameObject.Find("Cylinder");
		capLeft = GameObject.Find("CapLeft");
		capRight = GameObject.Find("CapRight");
		
		// Set the size of the paddle
		setSize(DEFAULT_SIZE);
	}
	
	/// <summary>
	/// Set the size of the paddle.
	/// </summary>
	/// <param name="value">Value.</param>
	void setSize(float value)
	{
		_size = value;
		if (capsule != null) {
			capsule.height = _size + 1f;
			var half = _size * 0.5f;
			cylinder.transform.localScale = new Vector3(1f, half, 1f);
			capLeft.transform.localPosition = new Vector3(-half, 0f, 0f);
			capRight.transform.localPosition = new Vector3(half, 0f, 0f);
			maxPos = (33f - (_size + 1f)) * 0.5f;
		}
	}
	
	/// <summary>
	/// Create a new ball, locked to the paddle.
	/// The ball will reparent itself when launched.
	/// </summary>
	public void NewBall()
	{
		var ball = Instantiate(BallPrefab) as GameObject;
		ball.transform.parent = transform;
		ball.transform.localPosition = new Vector3(0f,1f,0f);
	}
	
	/// <summary>
	/// Update the paddle's position
	/// </summary>
	void FixedUpdate ()
	{
		// Use the standard horizontal input to control the paddle
		var delta = Mathf.Clamp(Input.GetAxis("Horizontal"), -maxSpeed, maxSpeed);
		var newX = pos.x + delta;
		newX = Mathf.Clamp(newX, -maxPos, maxPos);
		transform.position = pos = new Vector3(newX, pos.y, pos.z);
	}
	
	
	/// <summary>
	/// Shrink the paddle and add a particle effect to hilight the change
	/// </summary>
	public void Shrink()
	{
		// ...only allow this to happen once!
		if (_size < Paddle.DEFAULT_SIZE) return;
		Instantiate(ShrinkEffect, new Vector3(pos.x - _size * 0.5f, pos.y, pos.z), Quaternion.identity);
		Instantiate(ShrinkEffect, new Vector3(pos.x + _size * 0.5f, pos.y, pos.z), Quaternion.identity);
		setSize(Paddle.SMALL_SIZE);
	}
	
}
