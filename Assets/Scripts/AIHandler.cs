using UnityEngine;
using System.Collections;

public class AIHandler : MonoBehaviour {
	
	public Player player;
	
	public float leftEdge = -7.5f;
	public float rightEdge = 7.3f;
	
	// Use this for initialization
	void Start () {
		player = gameObject.GetComponent<Player>();
		print (player.team);
	}
	
	void FixedUpdate () {
		if (!player.AIControl) return;
		
		if (player.fieldPosition == 1) {
			HandleInfieldPlayer();
		} else {
			HandleSidelinePlayer();
		}
	}
	
	void HandleInfieldPlayer() {
		// Handle tactics for infield players
		if (GameEngine.ball.holder && GameEngine.ball.holder.team != player.team) {
			// Run away!
			if (player.team == 1) {
				if (player.transform.position.x > leftEdge) {
					player.Movement(-1f, 0f);
				} else {
					if (player.facing != PlayerFacing.east) {
						player.Movement(0.1f, 0f);
					}
				}
			} else {
				if (player.transform.position.x < rightEdge) {
					player.Movement(1f, 0f);
				} else {
					if (player.facing != PlayerFacing.west) {
						player.Movement(-0.1f, 0f);
					}
				}
			}
		}
	}
	
	void HandleSidelinePlayer() {
		// Handle tactics for sideline players
		Ball ball = GameEngine.ball;
		float distance = ball.transform.position.x - player.transform.position.x;
		print (GameEngine.sideline.isBeyondAny(ball.transform.position));
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
			           && player.fieldPosition == 4) {
				distance = ball.transform.position.y - player.transform.position.y;
				if (distance < -0.2f) {
					player.Movement(-1f, 0f);
				} else if (distance > 0.2f){
					player.Movement(1f, 0f);
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
			           && player.fieldPosition == 4) {
				distance = ball.transform.position.y - player.transform.position.y;
				if (distance < -0.2f) {
					player.Movement(-1f, 0f);
				} else if (distance > 0.2f){
					player.Movement(1f, 0f);
				} else {
					player.PickupBall();
				}
			}
		}
	}
}
