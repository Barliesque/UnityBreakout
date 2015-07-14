using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// The Game class is the main class of the game, BREAKOUT!
/// </summary>
public class Game : MonoBehaviour {

	//----------------------------
	//  EDITOR PROPERTIES
	//----------------------------
	
	public GameObject[] rowPrefab;
	public GameObject Fireworks;
	
	//----------------------------
	//  FIELDS
	//----------------------------
	
	/// Global access to singleton instance
	public static Game inst = null;
	
	/// The player's paddle
	Paddle paddle;
	
	/// The number of rows of bricks still active in the game
	int rowCount;
	
	/// The number of balls the player has, including the one in play
	int ballsLeft;
	
	/// Label displaying how many balls the player still has
	UILabel uiBallsLeft;

	/// How many balls does the player start with?
	const int BALLS_PER_GAME = 3;
	
	/// The player's current score
	int score;
	
	/// Label displaying the player's score
	UILabel uiScore;
	
	/// The current level
	int level;
	
	/// Label displaying the current level
	UILabel uiLevel;
	
	/// The current hiscore, which is saved to PlayerPrefs with the key: "hiscore"
	int hiscore;
	
	/// Label displaying the current hiscore
	UILabel uiHiScore;
	
	/// The initial "PLAY" button shown when the application starts
	GameObject goPlayButton;
	
	/// The "PLAY AGAIN" button shown with the Game Over message
	GameObject goPlayAgainButton;
	
	/// A welcome message: "BREAKOUT!" and the "PLAY" button
	GameObject goBegin;
	
	/// GameObject containing the "GAME OVER" message and the "PLAY AGAIN" button
	GameObject goGameOver;
	
	/// Text to explain what keys to use to play the game
	GameObject goInstructions;
	
	/// Text at center of screen to announce the next level
	GameObject goLevelAnnounce;
	
	/// True, until the game is over
	bool isInPlay = false;
	
	
	
	//----------------------------
	//  CONSTRUCTOR
	//----------------------------
	public Game() : base() {
		
	}
	
	
	//----------------------------------------------------
	
	/// <summary>
	/// Initialize the application
	/// </summary>
	void Awake () {
		// Enforce singleton...
		if (inst != null) {
			Destroy(inst.gameObject);
			Debug.Log ("Duplicate Game instance destroyed!");
		}
		inst = this;
		
		// Make an interpolator value, based on the aspect ratio of the game area...
		// 0.0 = Aspect 3:2 (or narrower)
		// 1.0 = Aspect 16:9 (or wider)
		var camera = GameObject.Find("Game Camera").GetComponent<Camera>();
		var aspect = (camera.aspect - 1.5f) / 0.2777778f;
		
		// Use the aspect ratio to position the score panels
		var go = GameObject.Find("UpperLeft");
		var pos = go.transform.localPosition;
		go.transform.localPosition = new Vector3(Mathf.Lerp(0f,-100f,aspect), pos.y, pos.z);
		go = GameObject.Find("UpperRight");
		pos = go.transform.localPosition;
		go.transform.localPosition = new Vector3(Mathf.Lerp(374f,460f,aspect), pos.y, pos.z);
		
		// Find the player's paddle, which will never be destroyed
		paddle = MonoBehaviour.FindObjectOfType<Paddle>();
		
		// Find the various score keeping labels
		uiBallsLeft = GetLabel("BallsLeft");
		uiScore = GetLabel("Score");
		uiHiScore = GetLabel("HiScore");
		uiLevel = GetLabel("Level");
		
		// New Level Announcement - Hide it for now
		goLevelAnnounce = GameObject.Find("LevelAnnounce");
		goLevelAnnounce.SetActive(false);
		
		// BREAKOUT! and PLAY button
		goBegin = GameObject.Find("GameBegin");
		goPlayButton = GameObject.Find("PlayButton");
		UIEventListener.Get (goPlayButton).onClick += StartGame;
		
		// The PLAY AGAIN button
		goPlayAgainButton = GameObject.Find("PlayAgainButton");
		UIEventListener.Get (goPlayAgainButton).onClick += PlayAgain;
		
		// GAME OVER message - Hide for now
		goGameOver = GameObject.Find("GameOver");
		goGameOver.SetActive(false);
		
		// Instruction text - Hide for now
		goInstructions = GameObject.Find("Instructions");
		goInstructions.SetActive(false);
		
		// Check for a saved hiscore
		if (PlayerPrefs.HasKey("hiscore")) {
			hiscore = PlayerPrefs.GetInt("hiscore");
			UpdateHiScore();
		}
		
		// Build some bricks
		BuildLevel(1);
	}
	
	//----------------------------------------------------
	
	/// <summary>
	/// Start a new game.
	/// </summary>
	/// <param name="go">GameObject passed by NGUI.UIEventListener</param>
	void StartGame(GameObject go = null)
	{
		// Hide welcome message, and show instructions (which will be hidden by ClearMessages if this is not the first game)
		goBegin.SetActive(false);
		goInstructions.SetActive(true);
		
		// Set up the game
		SetBallsLeft(BALLS_PER_GAME);
		score = 0;
		UpdateScore();
		paddle.Size = Paddle.DEFAULT_SIZE;
		paddle.NewBall();
		Ball.velocity = Ball.INIT_VELOCITY;
		BuildLevel(1);
	}
	
	/// <summary>
	/// Hide messages, the game has begun!
	/// </summary>
	public void ClearMessages()
	{
		goGameOver.SetActive(false);
		goInstructions.SetActive(false);
		isInPlay = true;
	}
	
	/// <summary>
	/// The "PLAY AGAIN" button has been clicked.  Start a new game.
	/// </summary>
	/// <param name="go">GameObject passed by NGUI.UIEventListener</param>
	void PlayAgain (GameObject go)
	{
		StartGame();
		
		// Don't show instructions after the first game
		ClearMessages();
	}
	
	/// <summary>
	/// Set the level and build a new wall of bricks to destroy.
	/// </summary>
	/// <param name="level">The current level</param>
	void BuildLevel(int level = 1)
	{
		// Update the level number
		this.level = level;
		uiLevel.text = "<" + level + ">";
		
		if (level == 1) {
			// It's Level 1, so just get on with it
			BuildBricks();
		} else {
			// Show level up animation
			var label = goLevelAnnounce.GetComponent<UILabel>();
			label.text = "< LEVEL " + level + " >";
			var tween = goLevelAnnounce.GetComponent<TweenScale>();
			tween.Reset();
			tween.Play(true);
			goLevelAnnounce.SetActive(true);
			
			// Fireworks!
			Instantiate(Fireworks);
			
			// Wait a moment before building bricks
			Invoke("BuildBricks", 2.5f);
		}
	}
	
	/// <summary>
	/// Builds the bricks.
	/// </summary>
	void BuildBricks()
	{
		// Remove any pre-existing bricks
		var bricks = GameObject.Find("Bricks").transform;
		foreach (var row in bricks.GetComponentsInChildren<BrickRow>()) {
			Destroy(row.gameObject);
		}
		
		// If necessary, re-connect ball to the paddle (so it doesn't get bricks built on top of it!)
		var ball = GameObject.FindObjectOfType<Ball>();
		if (ball != null && ball.isInPlay) {
			ball.isInPlay = false;
			ball.transform.parent = paddle.gameObject.transform;
			ball.transform.localPosition = new Vector3(0f, 1f, 0f);
			ball.body.isKinematic = true;
		}
		
		// Build rows of bricks, as specified in the editor property:  rowPrefab[]
		const float ROW_SCALE = 1.5f;
		rowCount = rowPrefab.Length;
		for (int i = 0; i < rowCount; i++) {
			var row = Instantiate(rowPrefab[i], new Vector3(0f, bricks.position.y - i * ROW_SCALE, 0f), Quaternion.identity) as GameObject;
			row.transform.localScale = new Vector3(1f, ROW_SCALE, 1f);
			row.transform.parent = bricks;
		}
	}

	/// <summary>
	/// A row of bricks has been destroyed
	/// </summary>
	public void RowDestroyed ()
	{
		// Is that *all* the bricks?
		if (--rowCount == 0) {
			// Make sure a game is in progress, ie. this is not a result of the Drop effect when a game is over
			if (isInPlay) {
				// Level Up!
				BuildLevel(++level);
			}
		}
	}
	
	//----------------------------------------------------
	
	/// <summary>
	/// The current ball has been lost
	/// </summary>
	public void LoseBall()
	{
		// Update the counter
		SetBallsLeft(--ballsLeft);
		
		if (ballsLeft == 0) {
			GameOver();
		} else {
			paddle.NewBall();
		}
	}
	
	/// <summary>
	/// Set the number of balls
	/// </summary>
	/// <param name="balls">Balls.</param>
	void SetBallsLeft(int balls)
	{
		ballsLeft = balls;
		// Update the display counter
		uiBallsLeft.text = repeat("*", balls);
	}
	
	/// <summary>
	/// The game is over.
	/// </summary>
	void GameOver ()
	{
		// Show GAME OVER message
		goGameOver.SetActive(true);
		goPlayAgainButton.gameObject.SetActive(false);
		isInPlay = false;
		
		// Drop all the bricks off the screen!
		Physics.gravity = new Vector3(0f, -10f, 0f);
		foreach (var brick in GameObject.FindObjectsOfType<Brick>()) {
			brick.Drop();
		}
		
		// Wait a moment before showing PLAY AGAIN button
		Invoke("ShowPlayAgain", 3f);
	}
	
	/// <summary>
	/// Show "PLAY AGAIN" button
	/// </summary>
	void ShowPlayAgain()
	{
		goPlayAgainButton.SetActive(true);
	}
	
	//----------------------------------------------------

	/// <summary>
	/// Shrink the paddle.
	/// Called by the current Ball, when it hits the top wall
	/// </summary>
	public void ShrinkPaddle()
	{
		paddle.Shrink();
	}	
	
	//----------------------------------------------------
	
	/// <summary>
	/// Add points to the player's current score.
	/// </summary>
	/// <param name="points">Points.</param>
	public void AddToScore(int points)
	{
		//TODO: Tween the score up!
		score += points;
		UpdateScore();
	}
	
	/// <summary>
	/// Update the score label
	/// </summary>
	void UpdateScore()
	{
		var leading = 6 - score.ToString().Length;
		uiScore.text = repeat("0", leading) + score;
		
		//TODO: Make it more eventful when beating the hiscore.  Consider waiting until the game is over to announce.
		UpdateHiScore();
	}
	
	/// <summary>
	/// Update the hiscore if it's been beaten
	/// </summary>
	void UpdateHiScore()
	{
		if (score > hiscore) hiscore = score;
		var leading = 6 - hiscore.ToString().Length;
		uiHiScore.text = repeat("0", leading) + hiscore;
	}
	
	//----------------------------------------------------
	
	/// <summary>
	/// Find an NGUI Label, by the name of its GameObject
	/// </summary>
	/// <returns>The UILabel.</returns>
	/// <param name="name">Name of the GameObject containing the UILabel</param>
	UILabel GetLabel(string name)
	{
		return GameObject.Find(name).GetComponent<UILabel>();
	}
	
	/// <summary>
	/// Returns a string that repeats a given string by a specified count
	/// </summary>
	/// <param name="s">The string to repeat</param>
	/// <param name="count">How many times to repeat</param>
	string repeat(string s, int count)
	{
		return string.Join ("", Enumerable.Repeat(s, count).ToArray());
	}
	
	//----------------------------------------------------
	
	
	void Update()
	{
		// Allow ESC to close the application
		if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
		
		/*// Test leveling up
		if (Input.GetKeyDown(KeyCode.L)) {
			// Remove all bricks
			var bricks = GameObject.Find("Bricks").transform;
			foreach (var row in bricks.GetComponentsInChildren<BrickRow>()) {
				Destroy(row.gameObject);
			}
			// Level up!
			BuildLevel(++level);
		}
		//*/
	}
	
	/// <summary>
	/// When exiting the application, save the hiscore
	/// </summary>
	void OnApplicationQuit()
	{
		PlayerPrefs.SetInt("hiscore", hiscore);
		PlayerPrefs.Save();
	}
	
}
