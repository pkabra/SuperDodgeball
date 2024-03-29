﻿using UnityEngine;
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
				//player.vel = Vector3.zero;
			}
			p.AIControl = false;
			player = p;
			lastPlayerChangeTime = Time.time;
		}
	}
	
	public static List<Player>	team1;
	public static List<Player> 	team2;
	public static List<Ball>    ballsack;
	
	public Text					levelText = null;
	public static int			levelTextTween = 1;
	
	public static Controller 	player1 = new Controller();
	public static Controller	player2 = new Controller();
	
	public static Camera        cam;
	
	public static Sideline      sideline;
	public static Player        passTarget;
	public static Player        team1pos2;
	public static Player        team1pos3;
	public static Player        team1pos4;
	public static Player        team2pos2;
	public static Player        team2pos3;
	public static Player        team2pos4;
	
	public static float			gravity = -0.7f;
	
	public static bool			resetBallOn = false;
	public bool                 shieldsEnabled = false;
	
	public bool					singlePlayer = false;
	public static bool 			limitTeam2AI = false;
	public static ComputerOpponent	computer;
	public GameObject           angel = null;
	public static GameObject    angelPrefab = null;
	
	public bool					customLevel = false; 
	public static bool			customStatic = false; 

	public static bool			gibsonMode = false;
	public Text					gibsonModeSign;
	
	// Use this for initialization
	void Awake () {
		team1 = new List<Player>();
		team2 = new List<Player>();
		ballsack = new List<Ball>();
		
		cam = this.GetComponentInParent<Camera> ();
		
		if (singlePlayer) {
			computer = GameObject.Find ("Team2Container").GetComponent<ComputerOpponent>();
			limitTeam2AI = true;
		}
		angelPrefab = angel;
		customStatic = customLevel; 
	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.LoadLevel("open_screen");
		}
		
		if (!customLevel) {
			HandleClassicControls();
		} else {
			CustomControls();
		}
		Cheats ();
	}

	void Cheats() {
		if(Input.GetKeyDown(KeyCode.H)){
			foreach(Player p in team1) {
				p.hp = 48;
			}
		}

		if (Input.GetKeyDown(KeyCode.G)) {
			gibsonMode = !gibsonMode;
			if (gibsonMode) {
				gibsonModeSign.text = "G";
			} else {
				gibsonModeSign.text = "";
			}
		}

		if (customLevel) {
			if (Input.GetKeyDown(KeyCode.T)) {
				player1.player.shieldEarned();
			}
		}

		if (!customLevel) {
			if (Input.GetKeyDown(KeyCode.R)) {
				ballsack[0].ResetBall();
			}
		}
	}
	
	void CustomControls() {
		float myLocalTime = Time.time;
		if (player1.player == null) {
			if (team1.Count == 0) return;
			player1.ChangeControlTo(team1[0]);
		}

		//// Player 1 double tap detection
		// left
		if (player1.player.kState.state == KineticStates.walk || player1.player.kState.state == KineticStates.run
		    && player1.player.fieldPosition == 1) {
			if(Input.GetKeyDown("a") || (Input.GetKeyDown(KeyCode.LeftArrow) && singlePlayer)) {
				if (myLocalTime - player1.lastLeftKeyPress < 0.2f) {
					if(player1.player.kState.state != KineticStates.run){
						player1.player.kState.startTime = myLocalTime;
					}
					player1.player.kState.state = KineticStates.run;
					player1.player.runDir = -1;
					if(player1.player.aniState != AniState.Windup){
						player1.player.aniState = AniState.Running;
					}
				} else if (player1.player.kState.state == KineticStates.run) {
					if (player1.player.vel.x > 0f) {
						player1.player.kState.state = KineticStates.walk;
						player1.player.kState.startTime = Time.time;
						if(player1.player.aniState != AniState.Windup){
							player1.player.aniState = AniState.Walking;
						}
					}
				}
				player1.lastLeftKeyPress = myLocalTime;
			}
			// right
			if(Input.GetKeyDown("d") || (Input.GetKeyDown(KeyCode.RightArrow) && singlePlayer)) {
				if (Time.time - player1.lastRightKeyPress < 0.2f) {
					if(player1.player.kState.state != KineticStates.run){
						player1.player.kState.startTime = myLocalTime;
					}
					player1.player.kState.state = KineticStates.run;
					player1.player.runDir = 1;
					if(player1.player.aniState != AniState.Windup){
						player1.player.aniState = AniState.Running;
					}
				} else if (player1.player.kState.state == KineticStates.run) {
					if (player1.player.vel.x < 0f) {
						player1.player.kState.state = KineticStates.walk;
						player1.player.kState.startTime = Time.time;
						if(player1.player.aniState != AniState.Windup){
							player1.player.aniState = AniState.Walking;
						}
					}
				}
				player1.lastRightKeyPress = Time.time;
			}
		}

		//Controls
		player1.h = Input.GetAxisRaw("Horizontal");
		player1.y = Input.GetAxisRaw("Vertical");
		if (singlePlayer) {
			if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
				player1.h = Input.GetAxisRaw("Horizontal2");
			}
			if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) {
				player1.y = Input.GetAxisRaw("Vertical2");
			}
		}
		if( Input.GetKeyDown (KeyCode.X) || (Input.GetKeyDown(KeyCode.Period) && singlePlayer) ){
			player1.b = true;
			player1.lastBPress = Time.time;
			if ((Input.GetKey (KeyCode.Z) || (Input.GetKey(KeyCode.Comma) && singlePlayer)) && (Time.time - player1.lastAPress) < 0.01f) {
				player1.a = true;
			}
		}
		if( Input.GetKeyDown (KeyCode.Z) || (Input.GetKeyDown(KeyCode.Comma) && singlePlayer) ){
			player1.a = true;
			player1.lastAPress = Time.time;
			if ((Input.GetKey(KeyCode.X) || (Input.GetKey(KeyCode.Period) && singlePlayer)) && (Time.time - player1.lastBPress) < 0.01f) {
				player1.b = true;
			}
		}
		
		foreach (Player player in team1) {
			if (sideline.isBeyondLeft(player.transform.position) && player.fieldPosition == 1) {
				player.goneOverboard = true;
			} else if(!player.playerAI.inReturnMode) {
				player.goneOverboard = false;
			}
		}

		HandleOverRun(player1);
	}

	void HandleOverRun(Controller p) {
		if (p.player.fieldPosition != 1) return;
		if ((p.player.team == 1 && p.player.transform.position.x > -0.1f) ||
		    (p.player.team == 2 && p.player.transform.position.x < 0.1f)) {
			if (p.player.kState.state == KineticStates.run) {
				if (p.player.aniState == AniState.Windup ||
					p.player.aniState == AniState.Throwing ||
				    p.player.aniState == AniState.JumpThrowing ||
				    p.player.aniState == AniState.Passing) {
					p.player.goneOverboard = true;
				} else {
					p.player.kState.state = KineticStates.walk;
					p.player.kState.startTime = Time.time;
					if(p.player.aniState != AniState.Windup){
						p.player.aniState = AniState.Walking;
					}
					if (p.player.heldBall) {
						p.player.DropBall();
					}
				}
			} else if (p.player.kState.state == KineticStates.walk) {
				p.player.goneOverboard = true;
			}
		}
	}
	
	void HandleClassicControls() {
		float myLocalTime = Time.time;
		
		if (player1.player == null) {
			player1.ChangeControlTo(team1[0]);
		}
		if (player2.player == null) {
			player2.ChangeControlTo(team2[0]);
		}
		
		//// Player 1 double tap detection
		// left
		if (player1.player.kState.state == KineticStates.walk || player1.player.kState.state == KineticStates.run
		    && player1.player.fieldPosition == 1) {
			if(Input.GetKeyDown("a") || singlePlayer && (Input.GetKeyDown(KeyCode.LeftArrow))) {
				if (myLocalTime - player1.lastLeftKeyPress < 0.2f) {
					if(player1.player.kState.state != KineticStates.run){
						player1.player.kState.startTime = myLocalTime;
					}
					player1.player.kState.state = KineticStates.run;
					player1.player.runDir = -1;
					if(player1.player.aniState != AniState.Windup){
						player1.player.aniState = AniState.Running;
					}
				} else if (player1.player.kState.state == KineticStates.run) {
					if (player1.player.vel.x > 0f) {
						player1.player.kState.state = KineticStates.walk;
						player1.player.kState.startTime = Time.time;
						if(player1.player.aniState != AniState.Windup){
							player1.player.aniState = AniState.Walking;
						}
					}
				}
				player1.lastLeftKeyPress = myLocalTime;
			}
			// right
			if(Input.GetKeyDown("d") || singlePlayer && (Input.GetKeyDown(KeyCode.RightArrow))) {
				if (Time.time - player1.lastRightKeyPress < 0.2f) {
					if(player1.player.kState.state != KineticStates.run){
						player1.player.kState.startTime = myLocalTime;
					}
					player1.player.kState.state = KineticStates.run;
					player1.player.runDir = 1;
					if(player1.player.aniState != AniState.Windup){
						player1.player.aniState = AniState.Running;
					}
				} else if (player1.player.kState.state == KineticStates.run) {
					if (player1.player.vel.x < 0f) {
						player1.player.kState.state = KineticStates.walk;
						player1.player.kState.startTime = Time.time;
						if(player1.player.aniState != AniState.Windup){
							player1.player.aniState = AniState.Walking;
						}
					}
				}
				player1.lastRightKeyPress = Time.time;
			}
		}
		
		//// Player 2 double tap detection
		// left
		if (player2.player.kState.state == KineticStates.walk || player2.player.kState.state == KineticStates.run
		    && player2.player.fieldPosition == 1) {
			if(Input.GetKeyDown("left") && !singlePlayer) {
				if (Time.time - player2.lastLeftKeyPress < 0.2f) {
					if(player2.player.kState.state != KineticStates.run){
						player2.player.kState.startTime = Time.time;
					}
					player2.player.kState.state = KineticStates.run;
					player2.player.runDir = -1;
					if(player2.player.aniState != AniState.Windup){
						player2.player.aniState = AniState.Running;
					}
				} else if (player2.player.kState.state == KineticStates.run) {
					if (player2.player.vel.x > 0f) {
						player2.player.kState.state = KineticStates.walk;
						player2.player.kState.startTime = Time.time;
						if(player2.player.aniState != AniState.Windup){
							player2.player.aniState = AniState.Walking;
						}
					}
				}
				player2.lastLeftKeyPress = Time.time;
			}
			// right
			if(Input.GetKeyDown("right") && !singlePlayer) {
				if (Time.time - player2.lastRightKeyPress < 0.2f) {
					if(player2.player.kState.state != KineticStates.run){
						player2.player.kState.startTime = Time.time;
					}
					player2.player.kState.state = KineticStates.run;
					player2.player.runDir = 1;
					if(player2.player.aniState != AniState.Windup){
						player2.player.aniState = AniState.Running;
					}
				} else if (player2.player.kState.state == KineticStates.run) {
					if (player2.player.vel.x < 0f) {
						player2.player.kState.state = KineticStates.walk;
						player2.player.kState.startTime = Time.time;
						if(player2.player.aniState != AniState.Windup){
							player2.player.aniState = AniState.Walking;
						}
					}
				}
				player2.lastRightKeyPress = Time.time;
			}
		}
		
		//Controls
		player1.h = Input.GetAxisRaw("Horizontal");
		player1.y = Input.GetAxisRaw("Vertical");
		if (singlePlayer) {
			if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
				player1.h = Input.GetAxisRaw("Horizontal2");
			}
			if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) {
				player1.y = Input.GetAxisRaw("Vertical2");
			}
		}
		if( Input.GetKeyDown (KeyCode.X) || (Input.GetKeyDown(KeyCode.Period) && singlePlayer) ){
			player1.b = true;
			player1.lastBPress = Time.time;
			if ((Input.GetKey (KeyCode.Z) || (Input.GetKey(KeyCode.Comma) && singlePlayer)) && (Time.time - player1.lastAPress) < 0.01f) {
				player1.a = true;
			}
		}
		if( Input.GetKeyDown (KeyCode.Z) || (Input.GetKeyDown(KeyCode.Comma) && singlePlayer) ){
			player1.a = true;
			player1.lastAPress = Time.time;
			if ((Input.GetKey(KeyCode.X) || (Input.GetKey(KeyCode.Period) && singlePlayer)) && (Time.time - player1.lastBPress) < 0.01f) {
				player1.b = true;
			}
		}
		
		if (!singlePlayer) {
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
		}
		
		foreach (Player player in team1) {
			if (sideline.isBeyondLeft(player.transform.position) && player.fieldPosition == 1) {
				player.goneOverboard = true;
			} else if(!player.playerAI.inReturnMode) {
				player.goneOverboard = false;
			}
		}
		
		foreach (Player player in team2) {
			if (sideline.isBeyondRight(player.transform.position) && player.fieldPosition == 1) {
				player.goneOverboard = true;
			} else if(!player.playerAI.inReturnMode) {
				player.goneOverboard = false;
			}
		}

		HandleOverRun(player1);
		HandleOverRun(player2);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!customLevel) {
			HandleClassicBehaviour();
			if (player1.player.kState.state == KineticStates.run) {
				player1.player.Movement(0f, player1.y);
			}
			if (player2.player.kState.state == KineticStates.run) {
				player2.player.Movement(0f, player2.y);
			}
		} else {
			CustomBehaviour();
			if (player1.player.kState.state == KineticStates.run) {
				player1.player.Movement(0f, player1.y);
			}
		}
	}
	
	void CustomBehaviour() {
		if (player1.player == null) {
			player1.ChangeControlTo(team1[0]);
		}
		if (player1.player.goneOverboard) {
			player1.player.playerAI.returnBehindBoundary();
		} else {
			// Controls
			if (player1.b && player1.a) {
				// Jump
				player1.player.Jump (player1.h);
			} else if (player1.a) {
				// Pickup or pass
				if(player1.player.kState.state != KineticStates.fall){
					player1.player.Crouch();
				}

				if (player1.player.jumpWindupB) {
					player1.player.Jump (player1.h);
				} else if(player1.player.jumpWindupA == false){
					StartCoroutine(player1.player.JumpDelay('A'));
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
					//					float throwVel1 = player1.player.ThrowAt(targPos);
					////					ball.height = player1.player.height + 1.3f;
					//					ball.ThrowToPos(targPos, throwVel1);
					if(player1.player.aniState != AniState.Windup){
						player1.player.heldBall.ThrowToPos(targPos, 0f);
					}
				} else {
					// Pickup
					bool freeBall = false;
					foreach (Ball b in ballsack) {
						if (b.state != BallState.rest && b.state != BallState.free) {
							freeBall = true;
							break;
						}
					}
					
					if (freeBall) {
						player1.player.AttemptCatchAtTime(Time.time);
					} else {
						player1.player.PickupBall();
					}
				}

				if (player1.player.jumpWindupA) {
					player1.player.Jump (player1.h);
				} else if(player1.player.jumpWindupB == false){
					StartCoroutine(player1.player.JumpDelay('B'));
				}
			} else if (player1.player.kState.state != KineticStates.run) {
				player1.player.Movement(player1.h, player1.y);
			}
		}
		
		player1.a = false;
		player1.b = false;
	}
	
	void HandleClassicBehaviour() {
		Ball tempBall = ballsack[0];
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
			bool held1 = false;
			foreach(Ball b in ballsack) {
				if (b.holder && b.holder.team == 1) {
					player1.ChangeControlTo(b.holder);
					held1 = true;
					break;
				}
			}
			
			if (!held1) {
				foreach(Player p in team1) {
					if (player1.player.kState.state != KineticStates.walk) break;
					if (player1.player.aState.state != ActionStates.none) break;
					// Calculate distance between ball and player
					float deltaA = Vector3.Distance(p.transform.position, tempBall.transform.position);
					float deltaB = Vector3.Distance(player1.player.transform.position, tempBall.transform.position);
					
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
					if(player1.player.aniState != AniState.Windup){
						// Pass
						StartCoroutine(player1.player.WindUpPass());
					}
				} else if(player1.player.aState.state == ActionStates.passing) {
					// Pickup
					if (Time.time - player1.player.aState.startTime > 0.5f) {
						player1.player.aState.state = ActionStates.none;
						player1.player.PickupBall();
					}
				} else if(player1.player.kState.state != KineticStates.fall){
					player1.player.AttemptCatchAtTime(Time.time);
				} else if(player1.player.kState.state != KineticStates.fall){
					if (tempBall.state == BallState.rest) {
						player1.player.PickupBall();
					} else {
						player1.player.Crouch();
					}
				} 

				if (player1.player.jumpWindupB) {
					player1.player.Jump (player1.h);
				} else if(player1.player.jumpWindupA == false){
					StartCoroutine(player1.player.JumpDelay('A'));
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
					//					float throwVel1 = player1.player.ThrowAt(targPos);
					////					ball.height = player1.player.height + 1.3f;
					//					ball.ThrowToPos(targPos, throwVel1);
					if(player1.player.aniState != AniState.Windup){
						player1.player.heldBall.ThrowToPos(targPos, 0f);
					}
				} else {
					// Pickup
					if (tempBall.state != BallState.rest) {
						player1.player.AttemptCatchAtTime(Time.time);
					} else {
						player1.player.PickupBall();
					}
				}

				if (player1.player.jumpWindupA) {
					player1.player.Jump (player1.h);
				} else if(player1.player.jumpWindupB == false){
					StartCoroutine(player1.player.JumpDelay('B'));
				}
			} else if (player1.player.kState.state != KineticStates.run){
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
			bool held2 = false;
			foreach(Ball b in ballsack) {
				if (b.holder && b.holder.team == 2) {
					player2.ChangeControlTo(b.holder);
					held2 = true;
					break;
				}
			}
			
			if (!held2) {
				foreach(Player p in team2) {
					if (player2.player.kState.state != KineticStates.walk) break;
					if (player2.player.aState.state != ActionStates.none) break;
					// Calculate distance between ball and player
					float deltaA = Vector3.Distance(p.transform.position, tempBall.transform.position);
					float deltaB = Vector3.Distance(player2.player.transform.position, tempBall.transform.position);
					
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
					if(player2.player.aniState != AniState.Windup){
						// Pass
						StartCoroutine(player2.player.WindUpPass());
					}
				} else if(player2.player.aState.state == ActionStates.passing) {
					// Pickup
					if (Time.time - player2.player.aState.startTime > 0.5f) {
						player2.player.aState.state = ActionStates.none;
						player2.player.PickupBall();
					}
				} else if(player2.player.kState.state != KineticStates.fall){
					player2.player.AttemptCatchAtTime(Time.time);
				} else if(player2.player.kState.state != KineticStates.fall){
					if (tempBall.state == BallState.rest) {
						player2.player.PickupBall();
					} else {
						player2.player.Crouch();
					}
				}

				if (player2.player.jumpWindupB) {
					player2.player.Jump (player1.h);
				} else if(player2.player.jumpWindupA == false){
					StartCoroutine(player2.player.JumpDelay('A'));
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
					//					float throwVel2 = player2.player.ThrowAt(targPos);
					//					ball.height = player2.player.height * 0.5f + 1.3f;
					//					ball.ThrowToPos(targPos, throwVel2);
					if(player2.player.aniState != AniState.Windup){
						player2.player.heldBall.ThrowToPos(targPos, 0f);
					}
				} else {
					// Pickup
					if (tempBall.state != BallState.rest) {
						player2.player.AttemptCatchAtTime(Time.time);
					} else {
						player2.player.PickupBall();
					}
				}
				
				if (player2.player.jumpWindupA) {
					player2.player.Jump (player1.h);
				} else if(player2.player.jumpWindupB == false){
					StartCoroutine(player2.player.JumpDelay('B'));
				}
			} else if (player2.player.kState.state != KineticStates.run) {
				player2.player.Movement(player2.h, player2.y);
			}
		}
		
		player2.a = false;
		player2.b = false;
		if (customLevel) {
			player2.h = 0f;
			player2.y = 0f;
		}
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
	
	public static Ball GetClosestBall(Vector3 poi){
		float minDist = 1234.0f;
		Ball closest = null;
		foreach(Ball b in ballsack){
			float dist = Vector3.Distance(b.transform.position, poi);
			if(dist < minDist){
				closest = b;
				minDist = dist;
			}
		}
		return closest;
	}
}
