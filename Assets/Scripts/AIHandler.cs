using UnityEngine;
using System.Collections;

public class AIHandler : MonoBehaviour {
	
	public Player player;
	
	public float leftEdge = -7.5f;
	public float rightEdge = 7.3f;

	public Vector3 target = Vector2.zero;
	public bool hasTarget = false;

	public bool inReturnMode = false;
	bool returnFromSide = false;
	int extraSteps = 20;
	
	// Use this for initialization
	void Start () {
		player = gameObject.GetComponent<Player>();
		//print (player.team);
	}
	
	void FixedUpdate () {
		if (!player.AIControl) return;
		if (GameEngine.ballsack.Count == 0) return;

		if (player.goneOverboard) {
			returnBehindBoundary();
		} else {
			if (player.fieldPosition == 1) {
				if (player.team == 2 && GameEngine.limitTeam2AI) return;
				HandleInfieldPlayer();
			} else {
				HandleSidelinePlayer();
			}
		}
	}
	
	void HandleInfieldPlayer() {
		Ball tempBall = GameEngine.ballsack[0];
		// Handle tactics for infield players
		// Run away!
		float runPosX = 0f;
		float runPosY = 0f;
		if (player.team == 1) {
			if (tempBall.holder && tempBall.holder.fieldPosition == 4) {
				runPosX = leftEdge + 2f;
			} else {
				runPosX = leftEdge;
			}
		} else {
			if (tempBall.holder && tempBall.holder.fieldPosition == 4) {
				runPosX = rightEdge - 2f;
			} else {
				runPosX = rightEdge;
			}
		}

		if (tempBall.holder && tempBall.holder.fieldPosition == 3) {
			runPosY = 0f;
		} else {
			runPosY = -3f;
		}
		Move(runPosX, runPosY);
	}

	void Move(float runPosX, float runPosY) {
		if (hasTarget) {
			float h = 0f;
			float v = 0f;
			if (player.transform.position.x > target.x + 0.3f) {
				h = -1f;
			} else if (player.transform.position.x < target.x - 0.3f) {
				h = 1f;
			}

			if (player.transform.position.y > target.y + 0.3f) {
				v = -1f;
			} else if (player.transform.position.y < target.y - 0.3f) {
				v = 1f;
			}

			if (h == 0f && v == 0f) {
				player.FaceBall();
				hasTarget = false;
			}

			player.Movement(h, v);
		} else {
			if (runPosX < 0f) {
				target.x = runPosX + (Random.value * 4f);
			} else {
				target.x = runPosX - (Random.value * 4f);
			}

			if (runPosY > -1.5f) {
				target.y = runPosY - (Random.value * 2f);
			} else {
				target.y = runPosY + (Random.value * 2f);
			}

			if (Random.Range(0, 40) == 0) {
				hasTarget = true;
			}
		}
	}
	
	void HandleSidelinePlayer() {
		// Handle tactics for sideline players
		Ball ball = GameEngine.ballsack[0];
		float distance = ball.transform.position.x - player.transform.position.x;
		//print (GameEngine.sideline.isBeyondAny(ball.transform.position));
		if (player.team == 1) {
			if (ball.transform.position.y < -3.1f
			    && ball.transform.position.x > 0f
			    && player.fieldPosition == 3) {
				if (distance < -0.2f) {
					player.Movement(-1f, 0f);
				} else if (distance > 0.2f){
					player.Movement(1f, 0f);
				} else {
					player.PickupBall();
				}
			} else if (ball.transform.position.y > 0.5f
			           && ball.transform.position.x > 0f
			           && player.fieldPosition == 2) {
				if (distance < -0.2f) {
					player.Movement(-1f, 0f);
				} else if (distance > 0.2f){
					player.Movement(1f, 0f);
				} else {
					player.PickupBall();
				}
			} else if (GameEngine.sideline.isBeyondRight(ball.transform.position)
			           && player.fieldPosition == 4 &&
			           !(ball.transform.position.y < -3.1f ||
			  	ball.transform.position.y > 0.5f)) {
				distance = ball.transform.position.y - player.transform.position.y;
				if (distance < -0.2f) {
					player.Movement(0f, -1f);
				} else if (distance > 0.2f){
					player.Movement(0f, 1f);
				} else {
					player.PickupBall();
				}
			}
		} else {
			if (ball.transform.position.y < -3.1f
			    && ball.transform.position.x < 0f
			    && player.fieldPosition == 3) {
				if (distance < -0.2f) {
					player.Movement(-1f, 0f);
				} else if (distance > 0.2f){
					player.Movement(1f, 0f);
				} else {
					player.PickupBall();
				}
			} else if (ball.transform.position.y > 0.5f
			           && ball.transform.position.x < 0f
			           && player.fieldPosition == 2) {
				if (distance < -0.2f) {
					player.Movement(-1f, 0f);
				} else if (distance > 0.2f){
					player.Movement(1f, 0f);
				} else {
					player.PickupBall();
				}
			} else if (GameEngine.sideline.isBeyondLeft(ball.transform.position)
			           && player.fieldPosition == 4 &&
			           !(ball.transform.position.y < -3.1f ||
			  	ball.transform.position.y > 0.5f)) {
				distance = ball.transform.position.y - player.transform.position.y;
				if (distance < -0.2f) {
					player.Movement(0f, -1f);
				} else if (distance > 0.2f){
					player.Movement(0f, 1f);
				} else {
					player.PickupBall();
				}
			}
		}
		if (Random.Range(0, 20) == 0) player.FaceBall();
	}
	
	public void returnBehindBoundary() {
		if (player.kState.state == KineticStates.run) {
			player.kState.state = KineticStates.walk;
		}
		
		inReturnMode = true;
		
		if (player.team == 1) {
			if (GameEngine.sideline.isBeyondLeft(player.transform.position)) {
				returnFromSide = true;
				player.Movement(1f, 0f);
			} else if (returnFromSide) {
				extraSteps--;
				player.Movement(1f, 0f);
				if (extraSteps == 0) {
					returnFromSide = false;
					inReturnMode = false;
					extraSteps = 20;
				}
			} else {
				if (player.transform.position.x < 1f &&
				    player.facing == PlayerFacing.east &&
				    (player.aniState == AniState.Throwing ||
				 player.aniState == AniState.Windup ||
				 player.aniState == AniState.Passing)) {
					player.Movement(1f, 0f);
				} else if(player.transform.position.x > -0.193) {
					player.Movement(-1f, 0f);
				} else {
					inReturnMode = false;
				}
			}
		} else {
			if (GameEngine.sideline.isBeyondRight(player.transform.position)) {
				returnFromSide = true;
				player.Movement(-1f, 0f);
			} else if (returnFromSide) {
				extraSteps--;
				player.Movement(-1f, 0f);
				if (extraSteps == 0) {
					returnFromSide = false;
					inReturnMode = false;
					extraSteps = 20;
				}
			} else {
				if (player.transform.position.x > -1f &&
				    player.facing == PlayerFacing.west &&
				    (player.aniState == AniState.Throwing ||
				 player.aniState == AniState.Windup ||
				 player.aniState == AniState.Passing)) {
					player.Movement(-1f, 0f);
				} else if(player.transform.position.x < 0.193) {
					player.Movement(1f, 0f);
				} else {
					inReturnMode = false;
				}
			}
		}
	}
}