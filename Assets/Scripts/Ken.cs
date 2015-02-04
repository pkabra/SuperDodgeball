using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum KenState {idle, lefty, righty, power, spinkick, uppercut, highkick, forwardhigh, hadouken, medkick}; 

public class Ken : MonoBehaviour {
	
	public Player	player;
	public bool 	hasTarget = false;
	public Vector3	target = new Vector3(1.6f, 0f, 0f);
	public float	lastTargetSetTime = -4f;
	public float 	lastBallThrow = -4f;

	public int[] kenAnimationFrames = new int[10] {0, 0, 9, 0, 33, 12, 11, 12, 13, 6};
	private int framesSinceLastChange = 0;
	private int framesTillAniEnd = 0;
	
	public bool		initThrow = false;
	//private bool    firstFrame = true;
	
	public GameObject ballPrefab = null;
	public RuntimeAnimatorController   hadoukenAnimator;
	
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
		
		if (player.kenState == KenState.idle && framesSinceLastChange > 20) {
			// Don't fire 20% of the time
			int rand = Random.Range(0, 13);
			if (rand < 11) {
				KenState mode = KenState.righty;
				switch (rand) {
				case 0: mode = KenState.hadouken;
					break;
				case 2: mode = KenState.uppercut;
					break;
				case 4: mode = KenState.highkick;
					break;
				case 6: mode = KenState.forwardhigh;
					break;
				case 8: mode = KenState.spinkick;
					break;
				default: mode = KenState.righty;
					break;
				}
				if (mode == KenState.righty) {
					mode = Random.Range(0, 2) == 0 ? KenState.righty : KenState.medkick;
				}
				framesTillAniEnd = kenAnimationFrames[(int)mode];
				framesSinceLastChange = 0;
				Fire (mode);
			}
		}
		
		DestroyTheBalls();
		
		if (player.kenState != KenState.idle && framesSinceLastChange >= framesTillAniEnd) {
			player.kenState = KenState.idle;
			framesSinceLastChange = 0;
		}
		
		framesSinceLastChange++;
	}
	
	void DestroyTheBalls() {
		List<Ball> ballsToDelete = new List<Ball>();
		foreach (Ball b in GameEngine.ballsack) {
			if (b.state == BallState.rest || b.transform.position.x < -9.65) {
				ballsToDelete.Add(b);
			}
		}
		
		foreach(Ball b in ballsToDelete) {
			GameEngine.ballsack.Remove(b);
			GameObject.Destroy(b.gameObject);
		}
	}
	
	void Move() {
		if(player == null) return;
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
	
	void Fire(KenState inMode) { // set up to recieve a PowerMode
		// Set new state and timestamp it
		player.kenState = inMode;
		kenStateChangeTime = Time.time;

		// Type of throw execution logic switch
		switch(inMode){
		case KenState.hadouken:
			StartCoroutine(HadoukenLogic(PowerMode.hadouken, 4));
			break;
		case KenState.forwardhigh:
			StartCoroutine(HadoukenLogic(PowerMode.wave, 4));
			break;
		case KenState.medkick:
			StartCoroutine(HadoukenLogic(PowerMode.none, 4));
			break;
		case KenState.highkick:
			StartCoroutine(HadoukenLogic(PowerMode.fastball, 4));
			break;
		case KenState.uppercut:
			StartCoroutine(HadoukenLogic(PowerMode.tsunami, 4));
			break;
		case KenState.spinkick:
			StartCoroutine(HadoukenLogic(PowerMode.breaker, 4)); // TODO THIS NEEDS ITS OWN FUNCTION
			break;
		case KenState.righty:
			StartCoroutine(HadoukenLogic(PowerMode.none, 4));
			break;
		default:
			print ("Didn't do anything in Fire()");
			break;
		}
//		if (!GameEngine.player1.player) return;
//		
//		GameObject a = Instantiate(ballPrefab) as GameObject;
//		Ball b = a.GetComponent<Ball>();
//		GameEngine.ballsack.Add(b);
//		player.heldBall = b;
//		
//		Vector3 ballPos = transform.position;
//		ballPos.x -= 1f;
//		b.transform.position = ballPos;
//		
//		Vector3 shotTarget = GameEngine.player1.player.transform.position;
//		shotTarget.y = transform.position.y;
//		
//		player.kenState = KenState.righty;
//		kenStateChangeTime = Time.time;
//		
//		player.ThrowAt(shotTarget);
//		b.ThrowToPos(shotTarget, 1f);
//		lastBallThrow = Time.time;
	}

	IEnumerator HadoukenLogic(PowerMode mode_in, int throwFrame){
		if (!GameEngine.player1.player) yield break;

		lastBallThrow = Time.time;
		
		Vector3 StartPos = this.transform.position;
		StartPos.x -= 1f;

		// Set shot target
		Vector3 shotTarget = GameEngine.player1.player.transform.position;
		shotTarget.y = transform.position.y;

		// Allow first several frames of animation to execute
		while(framesSinceLastChange < throwFrame){
			yield return null;
		}

		Ball b = makeBallAtPos(StartPos);
		player.heldBall = b;

		// Throw the ball and set ball state
		player.ThrowAt(shotTarget);
		b.ThrowToPos(shotTarget, 1f);
		b.state = BallState.powered;
		b.height = 1.5f;
		b.mode = mode_in;
	}

	Ball makeBallAtPos(Vector3 pos){
		GameObject a = Instantiate(ballPrefab, pos, transform.rotation) as GameObject;
		Ball b = a.GetComponent<Ball>();
		GameEngine.ballsack.Add(b);
		return b;
	}

}
