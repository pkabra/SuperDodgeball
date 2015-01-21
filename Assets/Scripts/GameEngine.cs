using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEngine : MonoBehaviour {

	public class Controller {
		public Player	player;
		public float	lastPlayerChangeTime = 0f;
		public float	lastLeftKeyPress = -1f;
		public float	lastRightKeyPress = -1f;
	}
	
	public static List<Player>	team1;
	public static List<Player> 	team2;

	public static Controller 	player1 = new Controller();
	public static Controller	player2 = new Controller();
	
	public static Ball			ball;

	public static float			gravity = -0.9f;

	// These were put in the class scope so that we could change them in the Update()
	// then apply them in FixedUpdate(). This fixes a problem I was having with the controls
	// not responding everyframe. Perhaps these could be moved to the 'Controller' class?
	private float h1 = 0.0f;
	private float y1 = 0.0f;
	private bool b1 = false;
	private bool a1 = false;
	private float h2 = 0.0f;
	private float y2 = 0.0f;
	private bool b2 = false;
	private bool a2 = false;
	
	// Use this for initialization
	void Awake () {
		team1 = new List<Player>();
		team2 = new List<Player>();
	}

	void Update () {
		//// Player 1 double tap detection
		// left
		if(Input.GetKeyDown("a")) {
			if (Time.time - player1.lastLeftKeyPress < 0.2f) {
				player1.player.kState.state = KineticStates.run;
			}
			player1.lastLeftKeyPress = Time.time;
		}
		if (Input.GetKeyUp("a")) {
			player1.player.kState.state = KineticStates.walk;
		}
		// right
		if(Input.GetKeyDown("d")) {
			if (Time.time - player1.lastRightKeyPress < 0.2f) {
				player1.player.kState.state = KineticStates.run;
			}
			player1.lastRightKeyPress = Time.time;
		}
		if (Input.GetKeyUp("d")) {
			player1.player.kState.state = KineticStates.walk;
		}

		//// Player 2 double tap detection
		// left
		if(Input.GetKeyDown("left")) {
			if (Time.time - player2.lastLeftKeyPress < 0.2f) {
				player2.player.kState.state = KineticStates.run;
			}
			player2.lastLeftKeyPress = Time.time;
		}
		if (Input.GetKeyUp("left")) {
			player2.player.kState.state = KineticStates.walk;
		}
		// right
		if(Input.GetKeyDown("right")) {
			if (Time.time - player2.lastRightKeyPress < 0.2f) {
				player2.player.kState.state = KineticStates.run;
			}
			player2.lastRightKeyPress = Time.time;
		}
		if (Input.GetKeyUp("right")) {
			player2.player.kState.state = KineticStates.walk;
		}

		//Controls
		h1 = Input.GetAxisRaw("Horizontal");
		y1 = Input.GetAxisRaw("Vertical");
		if( Input.GetKeyDown (KeyCode.X) ){
			b1 = true;
		}
		if( Input.GetKeyDown (KeyCode.Z) ){
			a1 = true;
		}

		h2 = Input.GetAxisRaw("Horizontal2");
		y2 = Input.GetAxisRaw("Vertical2");
		if( Input.GetKeyDown (KeyCode.Period) ){
			b2 = true;
		}
		if( Input.GetKeyDown (KeyCode.Comma) ){
			a2 = true;
		}

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//// Handle Player 1
		if (player1.player == null) {
			player1.player = team1[0];
			player1.lastPlayerChangeTime = Time.time;
		}

		foreach(Player p in team1) {
			if (player1.player.kState.state == KineticStates.run) break;
			// Calculate distance between ball and player
			float deltaA = Vector3.Distance(p.transform.position, ball.transform.position);
			float deltaB = Vector3.Distance(player1.player.transform.position, ball.transform.position);

			if (deltaA + 1f < deltaB && Time.time - player1.lastPlayerChangeTime > 4f) {
				player1.player = p;
				player1.lastPlayerChangeTime = Time.time;
			}
		}

		// Controls
		if (b1 && a1) {
			// Jump
		} else if (a1) {
			// Pickup or pass
			if (player1.player.aState.state == ActionStates.holding) {
				// Pass
				foreach(Player p in team1) {
					if (p.aState.state != ActionStates.holding) {
						ball.PassTo(p);
						break;
					}
				}
			} else if(player1.player.aState.state == ActionStates.passing) {
				// Pickup
				if (Time.time - player1.player.aState.startTime > 0.5f) {
					player1.player.aState.state = ActionStates.none;
					player1.player.PickupBall();
				}
			} else {
				player1.player.PickupBall();
			}
		} else if (b1) {
			// Pickup or throw
			if (player1.player.aState.state == ActionStates.holding) {
				// Throw
				// Aim to closest player
				Vector3 targPos = team2[0].transform.position;
				foreach (Player p in team2) {
					if (Vector3.Distance(p.transform.position, player1.player.transform.position) < Vector3.Distance(targPos, player1.player.transform.position)) {
						targPos = p.transform.position;
					}
				}
				targPos.z = -1.0f;
				float throwVel1 = player1.player.ThrowAt(targPos);
				ball.ThrowToPos(targPos, throwVel1);
			} else {
				// Pickup
				player1.player.AttemptCatchAtTime(Time.time);
			}
		} else {
			player1.player.Movement(h1, y1);
		}

		a1 = false;
		b1 = false;

		//// Handle Player 2
		if (player2.player == null) {
			player2.player = team2[0];
			player2.lastPlayerChangeTime = Time.time;
		}
		
		foreach(Player p in team2) {
			if (player2.player.kState.state == KineticStates.run) break;
			if (player2.player.aState.state == ActionStates.passing) break;
			// Calculate distance between ball and player
			float deltaA = Vector3.Distance(p.transform.position, ball.transform.position);
			float deltaB = Vector3.Distance(player2.player.transform.position, ball.transform.position);
			
			if (deltaA + 1f < deltaB && Time.time - player2.lastPlayerChangeTime > 4f) {
				player2.player = p;
				player2.lastPlayerChangeTime = Time.time;
			}
		}
		
		// Controls
		if (b2 && a2) {
			// Jump
		} else if (a2) {
			// Pickup or pass
			if (player2.player.aState.state == ActionStates.holding) {
				// Pass
				foreach(Player p in team2) {
					if (p.aState.state != ActionStates.holding) {
						ball.PassTo(p);
						break;
					}
				}
			} else if(player2.player.aState.state == ActionStates.passing) {
				// Pickup
				if (Time.time - player2.player.aState.startTime > 0.5f) {
					player2.player.aState.state = ActionStates.none;
					player2.player.PickupBall();
				}
			} else {
				player2.player.PickupBall();
			}
		} else if (b2) {
			// Pickup or throw
			if (player2.player.aState.state == ActionStates.holding) {
				// Throw
				// Aim Needed
				Vector3 targPos = team1[0].transform.position;
				foreach (Player p in team1) {
					if (Vector3.Distance(p.transform.position, player2.player.transform.position) < Vector3.Distance(targPos, player2.player.transform.position)) {
						targPos = p.transform.position;
					}
				}
				targPos.z = -1.0f;
				float throwVel2 = player2.player.ThrowAt(targPos);
				ball.ThrowToPos(targPos, throwVel2);
			} else {
				// Pickup
				player2.player.AttemptCatchAtTime(Time.time);
			}
		} else {
			player2.player.Movement(h2, y2);
		}

		a2 = false;
		b2 = false;
	}

	public static void ChangeControl(string tag) {
		if (tag == "Team1") {
			foreach(Player p in team1) {
				if (p.aState.state == ActionStates.holding) {
					player1.player = p;
					player1.lastPlayerChangeTime = Time.time;
					break;
				}
			}
		} else {
			foreach(Player p in team2) {
				if (p.aState.state == ActionStates.holding) {
					player2.player = p;
					player2.lastPlayerChangeTime = Time.time;
					break;
				}
			}
		}
	}
}
