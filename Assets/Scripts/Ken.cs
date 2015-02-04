using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ken : MonoBehaviour {
	
	public Player	player;
	public bool 	hasTarget = false;
	public Vector3	target = new Vector3(1.6f, 0f, 0f);
	public float	lastTargetSetTime = -4f;
	public float 	lastBallThrow = -4f;
	
	public bool		initThrow = false;
	
	public GameObject ballPrefab = null;
	
	public float kenStateChangeTime = 0f;
	
	// Use this for initialization
	void Start () {
		player = gameObject.GetComponent<Player>();
	}
	
	void FixedUpdate() {
		if (hasTarget) {
			Move ();
		} else {
			PickNewPosition();
		}
		
		if (Input.GetKey(KeyCode.H) && (Time.time - lastBallThrow > 1f)) {
			Fire ();
		}
		
		List<Ball> ballsToDelete = new List<Ball>();
		foreach (Ball b in GameEngine.ballsack) {
			if (b.state == BallState.rest || b.transform.position.x < -9.65) {
				ballsToDelete.Add(b);
			}
		}
		
		foreach(Ball b in ballsToDelete) {
			GameEngine.ballsack.Remove(b);
			if (GameEngine.ball == b) GameEngine.ball = null;
			GameObject.Destroy(b.gameObject);
		}
		
		if (player.kenState != KenState.idle && Time.time - kenStateChangeTime > 1f) {
			player.kenState = KenState.idle;
		}
	}
	
	void Move() {
		float h = 0f;
		float v = 0f;
		if (player.transform.position.x + 0.4f < target.x) {
			h = 1f;
		} else if (player.transform.position.x - 0.4f > target.x) {
			h = -1f;
		}
		
		if (player.transform.position.y + 0.4f < target.y) {
			v = 1f;
		} else if (player.transform.position.y - 0.4f > target.y) {
			v = -1f;
		}
		
		if (h == 0f && v == 0f) {
			hasTarget = false;
		}
		
		player.Movement(h, v);
	}
	
	void PickNewPosition() {
		if (Time.time - lastTargetSetTime < 1f) return;
		if (!GameEngine.player1.player) return;
		
		float possibleY = GameEngine.player1.player.transform.position.y;
		if (Random.value <= 0.25f) {
			possibleY -= 1f;
		} else if (Random.value >= 0.75f) {
			possibleY += 1f;
		}
		if (possibleY > 0.2f) possibleY = 0.15f;
		if (possibleY < -3.25f) possibleY = -3.2f;
		target = player.transform.position;
		target.y = possibleY;
		lastTargetSetTime = Time.time;
		hasTarget = true;
	}
	
	void Fire() {
		if (!GameEngine.player1.player) return;
		
		GameObject a = Instantiate(ballPrefab) as GameObject;
		Ball b = a.GetComponent<Ball>();
		GameEngine.ballsack.Add(b);
		
		Vector3 ballPos = transform.position;
		ballPos.x -= 1f;
		b.transform.position = ballPos;
		
		Vector3 shotTarget = GameEngine.player1.player.transform.position;
		shotTarget.y = transform.position.y;
		GameEngine.ball = b;
		
		player.kenState = KenState.righty;
		kenStateChangeTime = Time.time;
		
		player.ThrowAt(shotTarget);
		b.ThrowToPos(shotTarget, 1f);
		lastBallThrow = Time.time;
	}
}
