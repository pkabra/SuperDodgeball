using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum StageTypes { normal, ice }

public class GameEngine : MonoBehaviour {
	
	public class Controller {
		public Player	player;
		public float	lastPlayerChangeTime = 0f;
		public float	lastLeftKeyPress = -1f;
		public float	lastRightKeyPress = -1f;
		
		// These were put in the class scope so that we could change them in the Update()
		// then apply them in FixedUpdate(). This fixes a problem I was having with the controls
		// not responding everyframe.
		
		public bool a = false;
		public bool b = false;
		public float lastAPress = -1f;
		public float lastBPress = -1f;
		
		public float h = 0f;
		public float y = 0f;
		
		public void ChangeControlTo(Player p) {
			if (player != null) {
				player.AIControl = true;
				player.Movement (0f, 0f); // To signal that the player ain't moving no more.
				//player.vel = Vector3.zero;
			}
			p.AIControl = false;
			player = p;
			lastPlayerChangeTime = Time.time;
		}
	}
	
	public static List<Player>	team1;
	public static List<Player> 	team2;
	
	public Text					levelText = null;
	public static int			levelTextTween = 1;
	
	public static Controller 	player1 = new Controller();
	public static Controller	player2 = new Controller();
	
	public static Ball			ball;
	public static Sideline      sideline;
	public static Player        passTarget;
	public static Player        team1pos2;
	public static Player        team1pos3;
	public static Player        team1pos4;
	public static Player        team2pos2;
	public static Player        team2pos3;
	public static Player        team2pos4;
	
	public static float			gravity = -0.9f;
	
	public static bool			resetBallOn = false;
	
	public static StageTypes	stageType = StageTypes.normal;
	
	//private int temp = 0; // used for printing only once per second in some places
	
	
	
	// Use this for initialization
	void Awake () {
		team1 = new List<Player>();
		team2 = new List<Player>();
		
		if (GameObject.Find ("Stage").CompareTag("IceStage")) {
			stageType = StageTypes.ice;
		} else {
			stageType = StageTypes.normal;
		}
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.G)){
			foreach (Player p in team1) {
				p.shieldEarned();
			}
			foreach (Player p in team2) {
				p.shieldEarned();
			}
		}

		if (player1.player == null) {
			player1.ChangeControlTo(team1[0]);
		}
		if (player2.player == null) {
			player2.ChangeControlTo(team2[0]);
		}
		
		//// Player 1 double tap detection
		// left
		if (player1.player.kState.state == KineticStates.walk || player1.player.kState.state == KineticStates.run) {
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
		}
		
		//// Player 2 double tap detection
		// left
		if (player2.player.kState.state == KineticStates.walk || player2.player.kState.state == KineticStates.run) {
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
		}
		
		//Controls
		player1.h = Input.GetAxisRaw("Horizontal");
		player1.y = Input.GetAxisRaw("Vertical");
		if( Input.GetKeyDown (KeyCode.X) ){
			player1.b = true;
			player1.lastBPress = Time.time;
			if (Input.GetKey (KeyCode.Comma) && (Time.time - player1.lastAPress) < 0.01f) {
				player1.a = true;
			}
		}
		if( Input.GetKeyDown (KeyCode.Z) ){
			player1.a = true;
			player1.lastAPress = Time.time;
			if (Input.GetKey(KeyCode.Period) && (Time.time - player1.lastBPress) < 0.01f) {
				player1.b = true;
			}
		}
		
		player2.h = Input.GetAxisRaw("Horizontal2");
		player2.y = Input.GetAxisRaw("Vertical2");
		if( Input.GetKeyDown (KeyCode.Period) ){
			player2.b = true;
			player2.lastBPress = Time.time;
			if (Input.GetKey (KeyCode.Comma) && (Time.time - player2.lastAPress) < 0.01f) {
				player2.a = true;
			}
		}
		if( Input.GetKeyDown (KeyCode.Comma) ){
			player2.a = true;
			player2.lastAPress = Time.time;
			if (Input.GetKey(KeyCode.Period) && (Time.time - player2.lastBPress) < 0.01f) {
				player2.b = true;
			}
		}
		
		if (player1.player.transform.position.x > -0.193f && player1.player.fieldPosition == 1) {
			player1.player.goneOverboard = true;
		} else {
			player1.player.goneOverboard = false;
		}
		
		if (player2.player.transform.position.x < 0.193f && player2.player.fieldPosition == 1) {
			player2.player.goneOverboard = true;
		} else {
			player2.player.goneOverboard = false;
		}
		
		if (Input.GetKeyDown(KeyCode.Tab)) {
			if (Application.loadedLevelName ==  "icestage") {
				Application.LoadLevel("classic_japan");
			} else {
				Application.LoadLevel("icestage");
			}
		}
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//		if(temp % 20 == 1){
		//			print (passTarget.fieldPosition);
		//		}
		//		++temp;
		//// Handle Player 1
		
		if (levelText && levelText.fontSize > 17) {
			levelText.fontSize -= levelTextTween;
		}
		
		if (player1.player == null) {
			player1.ChangeControlTo(team1[0]);
		}
		
		if (player1.player.goneOverboard) {
			player1.player.playerAI.returnBehindBoundary();
		} else {
			if (ball.holder && ball.holder.team == 1) {
				player1.ChangeControlTo(ball.holder);
			} else {
				foreach(Player p in team1) {
					if (player1.player.kState.state != KineticStates.walk) break;
					if (player1.player.aState.state != ActionStates.none) break;
					// Calculate distance between ball and player
					float deltaA = Vector3.Distance(p.transform.position, ball.transform.position);
					float deltaB = Vector3.Distance(player1.player.transform.position, ball.transform.position);
					
					if (deltaA + 1f < deltaB && Time.time - player1.lastPlayerChangeTime > 2f) {
						player1.ChangeControlTo(p);
					}
				}
			}
			
			// Controls
			if (player1.b && player1.a) {
				// Jump
				player1.player.Jump (player1.h);
			} else if (player1.a) {
				// Pickup or pass
				if (player1.player.aState.state == ActionStates.holding) {
					// Pass
					ball.PassTo(passTarget);
				} else if(player1.player.aState.state == ActionStates.passing) {
					// Pickup
					if (Time.time - player1.player.aState.startTime > 0.5f) {
						player1.player.aState.state = ActionStates.none;
						player1.player.PickupBall();
					}
				} else if(player1.player.kState.state != KineticStates.fall){
					if (ball.state == BallState.rest || ball.state == BallState.free) {
						player1.player.PickupBall();
					} else {
						player1.player.Crouch();
					}
				} 
			} else if (player1.b) {
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
					//targPos.y += 0.5f;
					float throwVel1 = player1.player.ThrowAt(targPos);
					ball.height = player1.player.height + 1.3f;
					ball.ThrowToPos(targPos, throwVel1);
				} else {
					// Pickup
					if (ball.state != BallState.rest && ball.state != BallState.free) {
						player1.player.AttemptCatchAtTime(Time.time);
					} else {
						player1.player.PickupBall();
					}
				}
			} else {
				player1.player.Movement(player1.h, player1.y);
			}
		}
		
		player1.a = false;
		player1.b = false;
		
		//// Handle Player 2
		if (player2.player == null) {
			player2.ChangeControlTo(team2[0]);
		}
		
		if (player2.player.goneOverboard) {
			player2.player.playerAI.returnBehindBoundary();
		} else {
			if (ball.holder && ball.holder.team == 2) {
				if (ball.holder.GetInstanceID() != player2.player.GetInstanceID()) {
					player2.ChangeControlTo(ball.holder);
				}
			} else {
				foreach(Player p in team2) {
					if (player2.player.kState.state != KineticStates.walk) break;
					if (player2.player.aState.state != ActionStates.none) break;
					// Calculate distance between ball and player
					float deltaA = Vector3.Distance(p.transform.position, ball.transform.position);
					float deltaB = Vector3.Distance(player2.player.transform.position, ball.transform.position);
					
					if (deltaA + 1f < deltaB && Time.time - player2.lastPlayerChangeTime > 2f) {
						player2.ChangeControlTo(p);
					}
				}
			}
			
			// Controls
			if (player2.a && player2.b) {
				// Jump
				player2.player.Jump(player2.h);
			} else if (player2.a) {
				// Pickup or pass
				if (player2.player.aState.state == ActionStates.holding) {
					// Pass
					ball.PassTo(passTarget);
				} else if(player2.player.aState.state == ActionStates.passing) {
					// Pickup
					if (Time.time - player2.player.aState.startTime > 0.5f) {
						player2.player.aState.state = ActionStates.none;
						player2.player.PickupBall();
					}
				} else if(player1.player.kState.state != KineticStates.fall){
					if (ball.state == BallState.rest || ball.state == BallState.free) {
						player2.player.PickupBall();
					} else {
						player2.player.Crouch();
					}
				}
			} else if (player2.b) {
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
					//targPos.y += 0.5f;
					float throwVel2 = player2.player.ThrowAt(targPos);
					ball.height = player2.player.height * 0.5f + 1.3f;
					ball.ThrowToPos(targPos, throwVel2);
				} else {
					// Pickup
					if (ball.state != BallState.rest && ball.state != BallState.free) {
						player2.player.AttemptCatchAtTime(Time.time);
					} else {
						player2.player.PickupBall();
					}
				}
			} else {
				player2.player.Movement(player2.h, player2.y);
			}
		}
		
		player2.a = false;
		player2.b = false;
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
