using UnityEngine;
using System.Collections;

/// <summary>
/// A class to control a ball currently in play
/// </summary>
public class Ball : MonoBehaviour {


	//----------------------------
	//  FIELDS
	//----------------------------
	
	/// The velocity of balls when the game starts
	public const float INIT_VELOCITY = 16f;
	
	/// The current velocity of balls
	static public float velocity = INIT_VELOCITY;
	
	/// A light with a halo effect around the ball
	Light halo;
	
	/// An interpolator to make the halo pulse
	float haloCycle;
	
	/// The ball's RigidBody
	private Rigidbody _body;
	public Rigidbody body {
		get { return _body; }
	}
	
	/// True after the ball has been launched
	public bool isInPlay;
	
	//----------------------------------------------------
	
	/// <summary>
	/// Initialize the ball
	/// </summary>
	void Awake()
	{
		// Find the ball's RigidBody
		_body = GetComponent<Rigidbody>();
		
		// Find the light with the halo
		var lights = GetComponentsInChildren<Light>();
		foreach (var light in lights) {
			if (light.gameObject.name == "Ball Halo") {
				halo = light;
				break;
			}
		}
	}

	/// <summary>
	/// Update the ball
	/// </summary>
	void Update ()
	{
		// Press FIRE (Space Key) to Launch!
		if (Input.GetButtonDown("Fire1")) {
			LaunchBall();
		}
		
		// Make the halo pulse
		haloCycle += 0.1f;
		halo.range = Mathf.Abs(Mathf.Sin(haloCycle) * 0.2f) + 1.5f;
	}
	
	/// <summary>
	/// Launch the ball!
	/// </summary>
	void LaunchBall()
	{
		if (!isInPlay) {
			// Detach from the paddle...
			transform.parent = GameObject.Find("Game").transform;
			
			// Fire ball in a random direction:  straight up from the paddle, plus or minus 45 degrees
			_body.isKinematic = false;
			var angle = Random.Range(Mathf.PI * 0.25f, Mathf.PI * 0.75f);
			var force = new Vector3(velocity * Mathf.Cos(angle) * 50f, velocity * Mathf.Sin(angle) * 50f, 0f);
			_body.AddForce(force);
			
			isInPlay = true;
			Game.s_Inst.ClearMessages();
		}
	}
	
	/// <summary>
	/// The ball has collided with something solid...
	/// </summary>
	/// <param name="col">Information about the collision</param>
	void OnCollisionEnter(Collision col)
	{
		// Did we hit the paddle?
		if (col.collider.name == "Paddle") {
			// Check that the paddle is actually BELOW the ball...  if not, skip this!
			if (col.gameObject.transform.position.y > transform.position.y) return;
			
			// Tweak its direction and velocity, based on where it hit the paddle.
			var capsule = col.collider as CapsuleCollider;
			var rel = (col.gameObject.transform.position.x - transform.position.x) / (capsule.height * 0.5f);
			var angle = rel * 60f * Mathf.Deg2Rad + Mathf.PI * 0.5f;
			_body.velocity = new Vector3( velocity * Mathf.Cos(angle), velocity * Mathf.Sin(angle), 0f);
		}
		
		// Did we hit the Top Wall?
		if (col.collider.name == "WallTop") {
			Game.s_Inst.ShrinkPaddle();
		}
	}
	
	/// <summary>
	/// The ball has entered a trigger area...
	/// </summary>
	/// <param name="col">The trigger Collider object</param>
	void OnTriggerEnter(Collider col)
	{
		// The only trigger in use is the bottom wall.
		// So it is safe to assume the ball is lost.
		if (isInPlay) {
			isInPlay = false;
			Game.s_Inst.LoseBall();
			Destroy(gameObject);
		}
	}
	
	
}
