using UnityEngine;
using System.Collections;
//using System.Linq;

/// <summary>
/// The Game class is the main class of the game, BREAKOUT!
/// </summary>
public class Game : MonoBehaviour {

	//----------------------------
	//  EDITOR PROPERTIES
	//----------------------------
	
	/// The brick row prefabs, in the order they will be used to build the game board, from top to bottom
	public GameObject[] rowPrefab;

	/// A reference to the fireworks effect in the scene
	public GameObject Fireworks;
	
	/// Global access to singleton instance
	public static Game s_Inst = null;

	/// Rows of bricks will be contained by this transform
	public Transform m_BricksContainer;

	/// How many balls does the player start with?
	public int m_BallsPerGame = 3;

	/// The player's paddle
	public Paddle m_Paddle;

	/// UI Panel in the upper left corner of the screen
	public Transform m_UpperLeftPanel;

	/// UI Panel in the upper right corner of the screen
	public Transform m_UpperRightPanel;

	/// Label displaying how many balls the player still has
	public UILabel m_BallsLeftUI;
	
	/// Label displaying the player's score
	public UILabel m_ScoreUI;

	/// Label displaying the current level
	public UILabel m_LevelUI;

	/// Label displaying the current hiscore
	public UILabel m_HiScoreUI;

	/// The initial "PLAY" button shown when the application starts
	public GameObject m_PlayButton;

	/// The "PLAY AGAIN" button shown with the Game Over message
	public GameObject m_PlayAgainButton;

	/// A welcome message: "BREAKOUT!" and the "PLAY" button
	public GameObject m_BeginMessage;

	/// GameObject containing the "GAME OVER" message and the "PLAY AGAIN" button
	public GameObject m_GameOverMessage;

	/// Text to explain what keys to use to play the game
	public GameObject m_Instructions;

	/// Text at center of screen to announce the next level
	public GameObject m_LevelAnnounce;


	//----------------------------
	//  PRIVATE FIELDS
	//----------------------------

	/// The number of rows of bricks still active in the game
	int _rowCount;

	/// The number of balls the player has, including the one in play
	int _ballsLeft;

	/// The player's current score
	int _score;

	/// The score currently displayed, different only while adding points up to the actual current score
	int _scoreDisplayed;

	/// The current hiscore, which is saved to PlayerPrefs with the key: "hiscore"
	int _hiscore;

	/// The current level
	int _level;
	
	/// True, until the game is over
	bool isInPlay = false;
	
	
	//----------------------------------------------------
	
	/// <summary>
	/// Initialize the application
	/// </summary>
	void Awake () {
		// Enforce singleton...
		if (s_Inst != null) {
			Destroy(s_Inst.gameObject);
			Debug.Log ("Duplicate Game instance destroyed!");
		}
		s_Inst = this;
		
		// Make an interpolator value, based on the aspect ratio of the game area...
		// 0.0 = Aspect 3:2 (or narrower)
		// 1.0 = Aspect 16:9 (or wider)
		var aspect = (Camera.main.aspect - 1.5f) / 0.2777778f;
		
		// Use the aspect ratio to position the score panels
		var pos = m_UpperLeftPanel.localPosition;
		m_UpperLeftPanel.localPosition = new Vector3(Mathf.Lerp(0f, -100f, aspect), pos.y, pos.z);
		pos = m_UpperRightPanel.localPosition;
		m_UpperRightPanel.localPosition = new Vector3(Mathf.Lerp(374f, 460f, aspect), pos.y, pos.z);
		
		// Find the player's paddle, which will never be destroyed
		m_Paddle = MonoBehaviour.FindObjectOfType<Paddle>();

		// Show BREAKOUT! and Play Button, and hide all other messages and buttons
		m_BeginMessage.SetActive(true);
		m_PlayButton.SetActive(true);
		m_LevelAnnounce.SetActive(false);
		m_GameOverMessage.SetActive(false);
		m_PlayAgainButton.SetActive(false);
		m_Instructions.SetActive(false);

		// Init the PLAY buttons
		UIEventListener.Get (m_PlayAgainButton).onClick += PlayAgain;
		UIEventListener.Get(m_PlayButton).onClick += StartGame;

		// Check for a saved hiscore
		if (PlayerPrefs.HasKey("hiscore")) {
			_hiscore = PlayerPrefs.GetInt("hiscore");
			m_HiScoreUI.text = _hiscore.ToString("D6");
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
		m_BeginMessage.SetActive(false);
		m_PlayButton.SetActive(false);
		m_Instructions.SetActive(true);

		// Set up the game
		SetBallsLeft(m_BallsPerGame);
		_score = 0;
		_scoreDisplayed = -1;  // This will trigger the score to be updated on screen in Update()
		m_Paddle.Size = Paddle.DEFAULT_SIZE;
		m_Paddle.NewBall();
		Ball.velocity = Ball.INIT_VELOCITY;
		BuildLevel(1);
	}
	
	/// <summary>
	/// Hide messages, the game has begun!
	/// </summary>
	public void ClearMessages()
	{
		m_GameOverMessage.SetActive(false);
		m_PlayAgainButton.SetActive(false);
		m_Instructions.SetActive(false);
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
		this._level = level;
		m_LevelUI.text = "<" + level + ">";
		
		if (level == 1) {
			// It's Level 1, so just get on with it
			StartCoroutine(BuildBricks());
		} else {
			// Show level up animation
			var label = m_LevelAnnounce.GetComponent<UILabel>();
			label.text = "< LEVEL " + level + " >";
			var tween = m_LevelAnnounce.GetComponent<TweenScale>();
			tween.Reset();
			tween.Play(true);
			m_LevelAnnounce.SetActive(true);
			
			// Fireworks!
			Instantiate(Fireworks);

			// Wait a moment before building bricks
			StartCoroutine(BuildBricks(2.5f));
		}
	}
	
	/// <summary>
	/// Builds the bricks.
	/// </summary>
	IEnumerator BuildBricks(float delay = 0f)
	{
		if (delay > 0f)
			yield return new WaitForSeconds(delay);

		// Remove any pre-existing bricks
		BrickRow[] rows = m_BricksContainer.GetComponentsInChildren<BrickRow>();
        for (int i = 0; i < rows.Length; i++) {
			Destroy(rows[i].gameObject);
		}
		
		// If necessary, re-connect ball to the paddle (so it doesn't get bricks built on top of it!)
		var ball = GameObject.FindObjectOfType<Ball>();
		if (ball != null && ball.isInPlay) {
			ball.isInPlay = false;
			ball.transform.parent = m_Paddle.gameObject.transform;
			ball.transform.localPosition = new Vector3(0f, 1f, 0f);
			ball.body.isKinematic = true;
		}
		
		// Build rows of bricks, as specified in the editor property:  rowPrefab[]
		_rowCount = rowPrefab.Length;
		float wallHeight = 0f;
		for (int i = 0; i < _rowCount; i++) {
			var row = Instantiate(rowPrefab[i], new Vector3(0f, m_BricksContainer.position.y - wallHeight, 0f), Quaternion.identity) as GameObject;
			row.transform.parent = m_BricksContainer;
			wallHeight += row.GetComponent<BrickRow>().RowHeight;
        }
	}

	/// <summary>
	/// A row of bricks has been destroyed
	/// </summary>
	public void RowDestroyed ()
	{
		// Is that *all* the bricks?
		if (--_rowCount == 0) {
			// Make sure a game is in progress, ie. this is not a result of the Drop effect when a game is over
			if (isInPlay) {
				// Level Up!
				BuildLevel(++_level);
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
		SetBallsLeft(--_ballsLeft);
		
		if (_ballsLeft == 0) {
			GameOver();
		} else {
			m_Paddle.NewBall();
		}
	}
	
	/// <summary>
	/// Set the number of balls
	/// </summary>
	/// <param name="balls">Balls.</param>
	void SetBallsLeft(int balls)
	{
		_ballsLeft = balls;
		// Update the display counter
		m_BallsLeftUI.text = new string('*', balls);
	}
	
	/// <summary>
	/// The game is over.
	/// </summary>
	void GameOver ()
	{
		// Show GAME OVER message
		m_GameOverMessage.SetActive(true);
		m_PlayAgainButton.gameObject.SetActive(false);
		isInPlay = false;
		
		// Drop all the bricks off the screen!
		Physics.gravity = new Vector3(0f, -10f, 0f);

		var bricks = m_BricksContainer.GetComponentsInChildren<Brick>();
		for (int i = 0; i < bricks.Length; i++) {
			bricks[i].Drop();
		}

		// Show PLAY AGAIN button (after a short delay)
		StartCoroutine(ShowPlayAgain());
	}
	
	/// <summary>
	/// Show "PLAY AGAIN" button after a short delay
	/// </summary>
	IEnumerator ShowPlayAgain()
	{
		yield return new WaitForSeconds(3);
		m_PlayAgainButton.SetActive(true);
	}
	
	//----------------------------------------------------

	/// <summary>
	/// Shrink the paddle.
	/// Called by the current Ball, when it hits the top wall
	/// </summary>
	public void ShrinkPaddle()
	{
		m_Paddle.Shrink();
	}	
	
	//----------------------------------------------------
	
	/// <summary>
	/// Add points to the player's current score.
	/// </summary>
	/// <param name="points">Points.</param>
	public void AddToScore(int points)
	{
		_score += points;

		// Update the hiscore if it's been beaten
		if (_score > _hiscore) {
			_hiscore = _score;
			m_HiScoreUI.text = _hiscore.ToString("D6");
			//TODO: Make it more eventful when beating the hiscore.  Consider waiting until the game is over to announce.
		}
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
	
	//----------------------------------------------------
	
	
	void Update()
	{
		if (_score != _scoreDisplayed) {
			if (_score < _scoreDisplayed) {
				// Only when the score is reset will it be less than what's currently displayed.
				// In this case, update the display immediately.
				_scoreDisplayed = _score;
			} else {
				// Animate the value displayed until it matches the current score
				++_scoreDisplayed;
			}
			m_ScoreUI.text = _scoreDisplayed.ToString("D6");
		}

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
		PlayerPrefs.SetInt("hiscore", _hiscore);
		PlayerPrefs.Save();
	}
	
}
