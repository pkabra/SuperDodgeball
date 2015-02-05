using UnityEngine;
using System.Collections;

public class AIHandler : MonoBehaviour {
	
	public Player player;
	
	public float leftEdge = -7.5f;
	public float rightEdge = 7.3f;
	
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
		if (tempBall.holder && tempBall.holder.team != player.team) {
			// Run away!
			float runPos = 0f;
			float h = 0f;
			float v = 0f;
			if (player.team == 1) {
				if (tempBall.holder.fieldPosition == 4) {
					runPos = leftEdge + 2f;
				} else {
					runPos = leftEdge;
				}
				
				if (player.transform.position.x > runPos) {
					player.Movement(-1f, 0f);
				} else {
					player.FaceBall();
				}
			} else {
				if (tempBall.holder.fieldPosition == 4) {
					runPos = rightEdge - 2f;
				} else {
					runPos = rightEdge;
				}
				
				if (player.transform.position.x < runPos) {
					player.Movement(1f, 0f);
				} else {
					player.FaceBall();
				}
			}
			player.Movement(h, v);
		}
	}
	
	void HandleSidelinePlayer() {
		// Handle tactics for sideline players
		Ball ball = GameEngine.ballsack[0];
		float distance = ball.transform.position.x - player.transform.position.x;
		//print (GameEngine.sideline.isBeyondAny(ball.transform.position));
		if (player.team == 1) {
			if (GameEngine.sideline.isBeyondBottom(ball.transform.position)
			    && ball.transform.position.x > 0f
			    && player.fieldPosition == 3) {
				if (distance < -0.2f) {
					player.Movement(-1f, 0f);
				} else if (distance > 0.2f){
					player.Movement(1f, 0f);
				} else {
					player.PickupBall();
				}
			} else if (GameEngine.sideline.isBeyondTop(ball.transform.position)
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
			           !(GameEngine.sideline.isBeyondBottom(ball.transform.position) ||
			  GameEngine.sideline.isBeyondTop(ball.transform.position))) {
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
			if (GameEngine.sideline.isBeyondBottom(ball.transform.position)
			    && ball.transform.position.x < 0f
			    && player.fieldPosition == 3) {
				if (distance < -0.2f) {
					player.Movement(-1f, 0f);
				} else if (distance > 0.2f){
					player.Movement(1f, 0f);
				} else {
					player.PickupBall();
				}
			} else if (GameEngine.sideline.isBeyondTop(ball.transform.position)
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
			           !(GameEngine.sideline.isBeyondBottom(ball.transform.position) ||
			  GameEngine.sideline.isBeyondTop(ball.transform.position))) {
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