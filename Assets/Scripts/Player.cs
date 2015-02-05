using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum KineticStates{none, stand, walk, run, crouch, jump ,runjump, fall, stun, laying};
public enum ActionStates{none, throwing, catching, passing, holding};
public enum PlayerFacing{northEast, east, southEast, southWest, west, northWest};
public enum AniState{Standing, StandingNorth, StandingSouth, Walking, Running,
	Throwing, Passing, Crouching, Jumping, JumpThrowing, Falling, Laying, Windup, Catching, PassTargetAni};

public class ActionState {
	public ActionStates state = ActionStates.none;
	public float startTime = 0f;
}

public class KineticState {
	public KineticStates state = KineticStates.walk;
	public float startTime = 0f;
}

public class Player : MonoBehaviour {
	public bool             AIControl = true; // Is this player under AI control?
	public AIHandler        playerAI = null;
	public bool             goneOverboard = false;
	
	public Vector3          pos0 = Vector3.zero; // previous frame position
	public Vector3          vel = Vector3.zero;
	public float            hp = 48f;
	public ActionStates     myAstate = ActionStates.none;
	public KineticStates    myKState = KineticStates.none;
	public PowerMode        GroundAbility = PowerMode.none;
	public PowerMode        JumpAbility = PowerMode.none;
	public float            height = 0.0f; // height above ground plane
	public float            heightVel = 0f;
	public float            heightHitbox = 4f;
	public float            bounciness = 0.85f;
	
	public float            lastH = 0f;
	public float            lastV = 0f;
	
	public KineticState     kState = new KineticState();
	public ActionState      aState = new ActionState();
	public PlayerFacing     facing = PlayerFacing.east;
	public int              fieldPosition = 0;
	public int              team = 0;
	public bool             isShielded = false;
	public bool             goAround = false; // Around the World, Around the WOOOORLD!!!
	public bool             windupThr = false;
	public bool             windupPass = false;
	public bool            	jumpWindupA = false;
	public bool            	jumpWindupB = false;
	
	public float            jumpVelX = 0f;
	public float            jumpVelY = 0f;
	
	public Transform        spriteHolderTrans = null;
	private Shield          shieldHolder = null;
	public Animator         animator = null;
	private int             aniStateID = 0;
	public AniState         aniState = AniState.Standing;
	public GameObject       HPBar = null;
	private HPUpdaterGUI    hpGui = null;
	
	private float           catchingTime = 0.8f;
	private float           catchAttemptBuffer = 0.3f;
	private float           tryCatchTime = 0f;
	private bool            xLock = false;
	private bool            yLock = false;
	public bool             noBallHit = false;
	private bool            dead = false;
	public bool            noInput = false;
	
	public KenState			kenState = KenState.idle; 
	
	public Ball				heldBall = null;

	public int runDir = 0;
	
	// Use this for initialization
	void Start () {
		if (CompareTag("Team1")) {
			if(fieldPosition == 1){
				GameEngine.team1.Add(this);
			} else if(fieldPosition == 2){
				GameEngine.team1pos2 = this;
			} else if(fieldPosition == 3){
				GameEngine.team1pos3 = this;
			} else if(fieldPosition == 4){
				GameEngine.team1pos4 = this;
			}
		} else {
			if(fieldPosition == 1){
				GameEngine.team2.Add(this);
			} else if(fieldPosition == 2){
				GameEngine.team2pos2 = this;
			} else if(fieldPosition == 3){
				GameEngine.team2pos3 = this;
			} else if(fieldPosition == 4){
				GameEngine.team2pos4 = this;
			}
		}
		spriteHolderTrans = this.transform.FindChild("SpriteHolder");
		shieldHolder = this.transform.FindChild("ShieldHolder").GetComponent<Shield>();
		shieldHolder.myPlayer = this;
		animator = this.gameObject.GetComponentInChildren<Animator>();
		aniStateID = Animator.StringToHash("state");
		if(this.fieldPosition == 1){
			hpGui = this.HPBar.GetComponentInChildren<HPUpdaterGUI>();
		}
		
		GameEngine.passTarget = this;
		
		playerAI = gameObject.GetComponent<AIHandler>();
	}
	
	public void StateThrowing(){
		// Player just threw the ball
		aState.state = ActionStates.throwing;
		aniState = AniState.Throwing;
		noInput = true;
		StartCoroutine(resetAStateNone(0.5f));
	}
	
	void OnTriggerEnter(Collider other){
		if(GameEngine.customStatic && team == 2) return;
		
		if(other.gameObject.tag == "InfieldBoundary"){
			if((kState.state == KineticStates.walk || kState.state == KineticStates.run) && !goneOverboard)
			{
				InfieldCollideLogic(other);
			}
		}
	}
	//	
	//	void OnTriggerStay(Collider other){
	//		if(other.gameObject.tag == "InfieldBoundary"){
	//			if(kState.state == KineticStates.walk && !goneOverboard)
	//			{
	//				InfieldCollideLogic(other);
	//			}
	//		}
	//	}
	
	// Handle Player Movement
	public void Movement (float h, float v) {
		if (kState.state != KineticStates.walk && kState.state != KineticStates.run) return;
		if (aState.state == ActionStates.catching) return;
		
		lastH = h;
		lastV = v;
		
		Vector3 pos1 = transform.position;
		if (fieldPosition != 1) {
			HandleSidelineMovement(h, v);
		} else {
			// Protection from overriding collision resolution
			if(xLock){
				xLock = false;
				h = 0f;
			}
			if(yLock){
				yLock = false;
				v = 0f;
			}
			
			if (kState.state != KineticStates.run && (Time.time - kState.startTime < 0.5f || Time.time - aState.startTime < 0.5f)) return;
			
			//Store previous position
			pos0 = this.transform.position;
			
			// Movement
			if (kState.state == KineticStates.run) {
				vel.x = runDir * 6f;
				vel.y = v * 1f;
			} else {
				vel.x = h * 2f;
				vel.y = v * 2f;
			}
		}
		
		if (vel.x < 0f || (team == 1 && fieldPosition == 4)) {
			if (v < 0f) {
				facing = PlayerFacing.southWest;
			} else if (v > 0f) {
				facing = PlayerFacing.northWest;
			} else {
				facing = PlayerFacing.west;
			}
		} else if (vel.x > 0f || (team == 2 && fieldPosition == 4)) {
			if (v < 0f) {
				facing = PlayerFacing.southEast;
			} else if (v > 0f) {
				facing = PlayerFacing.northEast;
			} else {
				facing = PlayerFacing.east;			
			}
		} else {
			if (v < 0f) {
				if (facing == PlayerFacing.east || facing == PlayerFacing.northEast) {
					facing = PlayerFacing.southEast;
				} else if (facing == PlayerFacing.west || facing == PlayerFacing.northWest) {
					facing = PlayerFacing.southWest;
				}
			} else if (v > 0f) {
				if (facing == PlayerFacing.east || facing == PlayerFacing.southEast) {
					facing = PlayerFacing.northEast;
				} else if (facing == PlayerFacing.west || facing == PlayerFacing.southWest) {
					facing = PlayerFacing.northWest;
				}
			}
		}
		
		if(h == 0f && v == 0f && kState.state == KineticStates.walk && aniState != AniState.Windup && aniState != AniState.Crouching){
			if(facing == PlayerFacing.east || facing == PlayerFacing.west ){
				aniState = AniState.Standing;
			} else if (facing == PlayerFacing.northEast || facing == PlayerFacing.northWest ){
				aniState = AniState.StandingNorth;
			} else if (facing == PlayerFacing.southEast || facing == PlayerFacing.southWest ){
				aniState = AniState.StandingSouth;
			}
		}
		
		pos1 += vel * Time.fixedDeltaTime;
		transform.position = pos1;
	}
	
	public void HandleSidelineMovement(float h, float v) {
		Vector3 pos = transform.position;
		if (fieldPosition < 4) {
			vel.x = h * 2f;
			if (team == 1) {
				if (pos.x > 9f || pos.x < 0f) {
					vel.x = 0f;
				}
			} else {
				if (pos.x < -9f || pos.x > 0f) {
					vel.x = 0f;
				}
			}
		} else {
			float slope = GameEngine.sideline.slopeRight;
			if (team == 2) {
				slope = GameEngine.sideline.slopeLeft;
			}
			vel.y = v * 2f;
			vel.x = v * 2f/slope;
			if (pos.y > 0.2f || pos.y < -3.25f) {
				vel.y = 0f;
			}
		}
	}
	
	public void PickupBall() {
		if(noInput) return;
		if (kState.state != KineticStates.walk && kState.state != KineticStates.run) return; 
		Ball theBall = GameEngine.GetClosestBall(transform.position);
		if(theBall == null){
			Crouch ();
			return;
		}

		float heightDifference = theBall.height - this.height; 
		float delta = Vector3.Distance(transform.position, theBall.transform.position);
		if (delta < 0.8f && ((heightDifference <= heightHitbox) && heightDifference >= 0f)) {
			theBall.StateHeld(this);
		} 
		
		Crouch();
	}
	
	public void Crouch() {
		kState.state = KineticStates.crouch;
		Movement (0f,0f);
		aniState = AniState.Crouching;
		kState.startTime = Time.time;
		heightHitbox = 0.5f;
	}
	
	public void StandUp() {
		kState.state = KineticStates.walk;
		kState.startTime = Time.time;
		aniState = AniState.Standing;
		heightHitbox = 3.0f;
	}
	
	public void AttemptCatchAtTime(float catchTime){
		tryCatchTime = catchTime;
		//print (tryCatchTime);
		if(this.aState.state != ActionStates.catching){
			StartCoroutine(AttemptCatchAtTimeCR());
		} else {
			//DO NOTHING
			print ("Im doing nothing");
		}
	}
	
	public float ThrowAt(Vector3 targetPos){
		if(kState.state == KineticStates.run){
			if(GameEngine.gibsonMode || ((Time.time - kState.startTime) > 0.4f && (Time.time - kState.startTime) < 1.6f)){
				heldBall.state = BallState.powered;
				heldBall.mode = GroundAbility;
				heldBall.animator.SetInteger(aniStateID, (int)GroundAbility);
			} else {
				heldBall.state = BallState.thrown;
			}
		} else if(kState.state == KineticStates.walk){
			heldBall.state = BallState.thrown;
		} else if (kState.state == KineticStates.runjump) {
//			print ("running jump throw!");
//			print (jumpVelY);
			if (GameEngine.gibsonMode || (jumpVelY < 5f && jumpVelY > -5f)) {
				heldBall.state = BallState.superpowered;
				heldBall.mode = JumpAbility;
				heldBall.animator.SetInteger(aniStateID, (int)JumpAbility);
			} else {
				heldBall.state = BallState.thrown;
			}
		} else {
			heldBall.state = BallState.thrown;
		}
		StateThrowing();
		StartCoroutine(TempNoCollide(0.15f));
		return heldBall.getThrowSpeed();
	}
	
	IEnumerator AttemptCatch(){
		float endTime = Time.time + catchingTime;
		//print ("hands up");
		aState.state = ActionStates.catching;
		aniState = AniState.Catching;
		animator.SetInteger(aniStateID, (int)aniState);
		while(Time.time < endTime){
			yield return null;
		}
		//print ("hands down");
		if(aState.state == ActionStates.catching){
			aState.state = ActionStates.none;
			aniState = AniState.Standing;
			animator.SetInteger(aniStateID, (int)aniState);
		}
	}
	
	IEnumerator AttemptCatchAtTimeCR(){
		tryCatchTime -= catchAttemptBuffer;
		while(Time.time < tryCatchTime ){
			//print()
			yield return null;
		}
		if(!noInput){
			StartCoroutine( AttemptCatch() );
		}
	}
	
	public IEnumerator TempNoCollide(float secs){
		float endTime = Time.time + secs;
		this.noBallHit = true;
		if(this.isShielded) shieldHolder.collider.enabled = false;
		while(Time.time < endTime){
			yield return null;
		}
		if(this != null){
			this.noBallHit = false;
			if(this.isShielded) shieldHolder.collider.enabled = true;
		}
	}
	
	public IEnumerator resetAStateNone(float secs){
		float endTime = Time.time + secs;
		while(Time.time < endTime){
			yield return null;
		}
		noInput = false;
		if(this != null){
			this.aState.state = ActionStates.none;
			this.aniState = AniState.Standing;
			if (kState.state == KineticStates.run) {
				kState.state = KineticStates.walk;
			}
		}
	}
	
	public void PlayerHit(Ball other) {
		kState.state = KineticStates.fall;
		aState.state = ActionStates.none;
		aniState = AniState.Falling;
		noBallHit = true;
		noInput = true;
		
		float damage = Mathf.Ceil(Random.value * 8);
		if (other.state == BallState.superpowered) {
			damage += 14f;
		} else if (other.state == BallState.powered) {
			damage += 10f;
		}

		if (!GameEngine.gibsonMode || team == 2) {
			hp -= damage >= hp ? hp : damage;
		}
		
		hpGui.UpdateCover(hp);
		
		if (hp <= 0f) {
			dead = true;
			return;
		}
		
		// Just take the velocity the ball was moving in and multiply by 3
		// We know the ball velocity will be constant
		// Will need to add a clause for power shots
		if(other.state == BallState.powered){
			vel = other.vel * 4f;
		} else {
			vel = other.vel * 3f;
		}


		
		// Added height for bounce off of a player
		heightVel = 10f;
	}
	
	void FixedUpdate() {
		if (this.kState.state == KineticStates.fall) {
			PlayerFallLogic();
			this.transform.position += vel * Time.fixedDeltaTime;
			if(this.goAround){
				AroundTheWorld(); // does a check to see if we need to screen wrap the player's fall
			}
		} 
		
		if (this.kState.state == KineticStates.jump || this.kState.state == KineticStates.runjump) {
			JumpLogic();
		}
		
		if (this.aState.state == ActionStates.holding) {
			if(this.team == 1){
				PassTargetingLogicTeam1();
			} else {
				PassTargetingLogicTeam2();
			}
			if(kState.state == KineticStates.crouch){
				heldBall.height = 0f;
			} else {
				heldBall.height = height * 0.5f + 1.3f;
			}
			Vector3 pos = transform.position;
			if (facing == PlayerFacing.west || facing == PlayerFacing.northWest || facing == PlayerFacing.southWest) {
				pos.x -= heldBall.heldOffsetX;
			} else {
				pos.x += heldBall.heldOffsetX;
			}
			//pos.y += holder.height * 0.2f + 0.5f;
			heldBall.transform.position = pos;
			//height = holder.height * 0.5f + 1.3f;
		}
		
		if(aState.state == ActionStates.holding || aState.state == ActionStates.none){
			if(aniState != AniState.Windup && vel != Vector3.zero && kState.state == KineticStates.walk){
				aniState = AniState.Walking;
			}
		}
		
		if (kState.state == KineticStates.crouch) {
			if (Time.time - kState.startTime > 0.35f) {
				StandUp();
			}
		}
		
		// Block for things that should always happen
		Vector3 keepInVec = Vector3.zero;
		if(this.fieldPosition == 1)
		{
			if(transform.position.y > 0.2f){
				keepInVec = transform.position;
				keepInVec.y = 0.2f;
				transform.position = keepInVec;
			} else if (transform.position.y < -3.25f){
				keepInVec = transform.position;
				keepInVec.y = -3.25f;
				transform.position = keepInVec;
			}
		}
		float newZ;
		if(facing == PlayerFacing.east || facing == PlayerFacing.northEast || facing == PlayerFacing.southEast){
			newZ = transform.position.y * 0.001f;
		} else {
			newZ = transform.position.y * -0.001f;
		}
		Vector3 heightOffset = new Vector3( 0, height * 0.5f + 1.2f, newZ);
		spriteHolderTrans.localPosition = heightOffset;
		if (GameEngine.customStatic && team == 2) { 
			animator.SetInteger(aniStateID, (int)kenState); 
		} else { 
			animator.SetInteger(aniStateID, (int)aniState); 
		} 

		// This is a collision with the edge of the screen
		// The player should bounce off this edge
		Quaternion rot = transform.rotation;
		if(fieldPosition == 1){
			if(transform.position.x < -12.25f){
				transform.position = new Vector3(-12.25f, transform.position.y, -1f);
				vel = new Vector3(vel.x * -1f, 0f, 0f) * 0.3f;
				if((int)facing < 3) facing = PlayerFacing.west;
				else facing = PlayerFacing.east;
			} else if(transform.position.x > 12.25f){
				transform.position = new Vector3(12.25f, transform.position.y, -1f);
				vel = new Vector3(vel.x * -1f, 0f, 0f) * 0.3f;
				heightVel += 5.0f;
				if((int)facing < 3) facing = PlayerFacing.west;
				else facing = PlayerFacing.east;
			}
		}

		if (facing == PlayerFacing.east || facing == PlayerFacing.northEast || facing == PlayerFacing.southEast) {
			rot.y = 0f;
		} else {
			rot.y = 180f;
		}
		transform.rotation = rot;
		
		myAstate = aState.state;
		myKState = kState.state;
	}
	
	public void Jump(float h) {
		jumpWindupA = false;
		jumpWindupB = false;
		
		if ((int)kState.state > 4) return;
		if (height > 0f) return;
		
		aniState = AniState.Jumping;
		
		if (kState.state == KineticStates.run) {
			kState.state = KineticStates.runjump;
			jumpVelX = 0f;
			if (runDir > 0) {
				jumpVelX = 0.1f;
			} else if (runDir < 0) {
				jumpVelX = -0.1f;
			}
			jumpVelY = 35f;
			print ("running jump!");
		} else {
			kState.state = KineticStates.jump;
			jumpVelX = 0f;
			if (h > 0f) {
				jumpVelX = 0.05f;
			} else if (h < 0f) {
				jumpVelX = -0.05f;
			}
			jumpVelY = 30f;
			print ("normal jump!");
		}
		height = 0.1f;
	}
	
	public IEnumerator JumpDelay(char c){
		float endTime = Time.time + 0.2f;
		if(c == 'A') {
			jumpWindupA = true;
		}
		else if(c == 'B') {
			jumpWindupB = true;
		}
		while(Time.time < endTime){
			yield return null;
		}
		jumpWindupA = false;
		jumpWindupB = false;
	}
	
	void JumpLogic() {
//		print ("jumping!");
//		print (height);
		jumpVelY += GameEngine.gravity*3;
		height += jumpVelY * Time.fixedDeltaTime;

		if (height < 0f) {
			print ("done with jump.");
			kState.state = KineticStates.walk;
			jumpVelX = 0f;
			jumpVelY = 0f;
			height = 0f;
		}
			
		Vector3 pos = transform.position;
		pos.x += jumpVelX;
		transform.position = pos;
	}

	IEnumerator RunJumpLand(){
		float endTime = Time.time + 0.15f;
		kState.state = KineticStates.crouch;
		aniState = AniState.Crouching;
		while(Time.time < endTime){
			yield return null;
		}
		kState.state = KineticStates.stand;
		aniState = AniState.Standing;
	}
	
	void PlayerFallLogic() {
		if(height < 0.05f && (heightVel < 0.1f && heightVel > -0.1f)){
			kState.state = KineticStates.laying;
			aniState = AniState.Laying;
			noBallHit = false;
			goAround = false;
			height = 0f;
			heightVel = 0f;
			StartCoroutine(LayingLogic ());
		} else {
			heightVel += GameEngine.gravity;
			height += heightVel * Time.fixedDeltaTime;
			if(height < 0f){
				// Ball has hit the ground
				height = 0f;
				heightVel = 0f;
				vel *= 0.9f;
			}
		}
		
		if(vel.magnitude < 0.05f){
			vel = Vector3.zero;
		}
	}

	IEnumerator LayingLogic() {
		float endTime = Time.time + 1.2f;
		while(Time.time < endTime){
			yield return null;
		}
		noInput = false; // restore input capability
		if(dead){
			PlayerKilled();
		}
		kState.state = KineticStates.stand;
		aState.state = ActionStates.none;
		aniState = AniState.Standing;
	}
	
	void InfieldCollideLogic(Collider other){
		if(other.gameObject.tag == "InfieldBoundary"){
			//print ("Boundary Collision Detected");
			float boundaryHalfWidth = other.transform.lossyScale.x / 2.0f;
			float boundaryHalfHeight = other.transform.lossyScale.y / 2.0f;
			float thisHalfWidth = this.transform.lossyScale.x / 2.0f;
			float thisHalfHeight = this.transform.lossyScale.y / 2.0f;
			Vector3 desiredPos = this.transform.position;
			bool hitOnLeft = false;
			bool hitOnRight = false;
			bool hitOnTop = false;
			bool hitOnBottom = false;
			
			//print (vel);
			if((vel.x < 0.0001f && vel.x > -0.0001f) || (vel.y < 0.0001f && vel.y > -0.0001f)){ // Is non-diag vector, no raycast needed
				if(vel.x > 0f){ // Hit left side 
					hitOnLeft = true;
				} else if (vel.x < 0f){ // Hit right side
					hitOnRight = true;
				} else if (vel.y > 0f){ // Hit bottom
					hitOnBottom = true;
				} else if (vel.y < 0f){ // hit top
					hitOnTop = true;
				} else {
					print ("Player is not moving, should not hit boundary");
				}
				//print (desiredPos);
			} else { // travel is diagonal
				
				
				//Determine correct origin and direction for ray
				Vector3 dir = vel.normalized;
				Vector3 origin = this.transform.position - dir * 0.1f;
				bool rayNeeded = false;
				if(vel.x > 0f){
					if(vel.y > 0f){ //NE
						if((pos0.y + thisHalfHeight) > (other.transform.position.y - boundaryHalfHeight)){
							hitOnLeft = true;
						} else if((pos0.x + thisHalfWidth) > (other.transform.position.x - boundaryHalfWidth)){
							hitOnBottom = true;
						} else {
							rayNeeded = true;
							origin.x += thisHalfWidth;
							origin.y += thisHalfHeight;
						}
					}else if(vel.y < 0f){ //SE
						if((pos0.y - thisHalfHeight) < (other.transform.position.y + boundaryHalfHeight)){
							hitOnLeft = true;
						} else if((pos0.x + thisHalfWidth) > (other.transform.position.x - boundaryHalfWidth)){
							hitOnTop = true;
						} else {
							rayNeeded = true;
							origin.x += thisHalfWidth;
							origin.y -= thisHalfHeight;
						}
					}
				} else if(vel.y > 0f){ //NW
					if((pos0.y + thisHalfHeight) > (other.transform.position.y - boundaryHalfHeight)){
						hitOnRight = true;
					} else if((pos0.x - thisHalfWidth) < (other.transform.position.x + boundaryHalfWidth)){
						hitOnBottom = true;
					} else {
						rayNeeded = true;
						origin.x -= thisHalfWidth;
						origin.y += thisHalfHeight;
					}
				} else if(vel.y < 0f){ //SW
					if((pos0.y - thisHalfHeight) < (other.transform.position.y + boundaryHalfHeight)){
						hitOnRight = true;
					} else if((pos0.x - thisHalfWidth) < (other.transform.position.x + boundaryHalfWidth)){
						hitOnTop = true;
					} else {
						rayNeeded = true;
						origin.x -= thisHalfWidth;
						origin.y -= thisHalfHeight;
					}
				} else {
					print ("Error finding Raycast origin in Player.OnTriggerEnter()");
				}
				
				if(rayNeeded){
					print ("casting Ray.");
					//Cast ray
					Ray ray = new Ray(origin, dir);
					RaycastHit hit;
					other.Raycast(ray, out hit, 20.0f);
					Vector3 norm = hit.normal;
					print (norm);
					if(norm.x > 0){
						//print ("right side");
						hitOnRight = true;
					} else if( norm.x < 0){	
						//print ("left side");
						hitOnLeft = true;
					} else if( norm.y > 0){
						//print ("top side");
						hitOnTop = true;
					} else if ( norm.y < 0){
						//print ("bottom side");
						hitOnBottom = true;
					} else {
						print ("Raycast sucks.");
					}
				}
			}
			
			if(hitOnLeft){ // Hit left side 
				//print ("hit left");
				xLock = true;
				vel.x = 0f;
				desiredPos.x = other.transform.position.x - (boundaryHalfWidth + thisHalfWidth + 0.01f);
			} else if (hitOnRight){ // Hit right side
				//print ("hit right");
				xLock = true;
				vel.x = 0f;
				desiredPos.x = other.transform.position.x + (boundaryHalfWidth + thisHalfWidth + 0.01f);
			} else if (hitOnBottom){ // Hit bottom
				//print ("hit bottom");
				yLock = true;
				vel.y = 0f;
				desiredPos.y = other.transform.position.y - (boundaryHalfHeight + thisHalfHeight + 0.01f);
			} else if (hitOnTop){ // hit top
				//print ("hit top");
				yLock = true;
				vel.y = 0f;
				desiredPos.y = other.transform.position.y + (boundaryHalfHeight + thisHalfHeight + 0.01f);
			}
			
			//			if( Vector3.Distance(this.transform.position,pos0) < Vector3.Distance(desiredPos,pos0)){
			//				desiredPos.x = this.transform.position.x; // helps resolve colliding with more than one collider
			//				desiredPos.y = this.transform.position.y;
			//			}

			if (heldBall && aniState != AniState.Throwing && aniState != AniState.JumpThrowing &&
			    aniState != AniState.Windup && aniState != AniState.Passing &&
			    (hitOnTop || hitOnBottom || hitOnLeft || hitOnRight)) {
				DropBall();
			}
			
			this.transform.position = desiredPos;
			
			if (aState.state == ActionStates.holding){
				Vector3 pos = transform.position;
				if (facing == PlayerFacing.west || facing == PlayerFacing.northWest || facing == PlayerFacing.southWest) {
					pos.x -= heldBall.heldOffsetX;
				} else {
					pos.x += heldBall.heldOffsetX;
				}
				//pos.y += height * 0.2f + 0.5f;
				heldBall.transform.position = pos;
			}
		}
	}

	public void DropBall() {
		heldBall.holder = null;
		heldBall.mode = PowerMode.none;
		heldBall.state = BallState.free;
		heldBall = null;
		aState.state = ActionStates.none;
	}
	
	void PassTargetingLogicTeam1(){
		if(this.fieldPosition == 1 ){
			if(this.facing == PlayerFacing.east){
				GameEngine.passTarget = GameEngine.team1pos4;
			} else if(this.facing == PlayerFacing.northEast){
				GameEngine.passTarget = GameEngine.team1pos2;
			} else if(this.facing == PlayerFacing.southEast){
				GameEngine.passTarget = GameEngine.team1pos3;
			} else {
				PickClosestPos1Teammate(GameEngine.team1);
			}
		} else if(this.fieldPosition == 2){
			if(this.facing == PlayerFacing.east){
				GameEngine.passTarget = GameEngine.team1pos4;
			} else if(this.facing == PlayerFacing.southEast || this.facing == PlayerFacing.southWest){
				GameEngine.passTarget = GameEngine.team1pos3;
			} else if(this.facing == PlayerFacing.west){
				PickClosestPos1Teammate(GameEngine.team1);
			}
		} else if(this.fieldPosition == 3){
			if(this.facing == PlayerFacing.east){
				GameEngine.passTarget = GameEngine.team1pos4;
			} else if(this.facing == PlayerFacing.northEast || this.facing == PlayerFacing.northWest){
				GameEngine.passTarget = GameEngine.team1pos2;
			} else if(this.facing == PlayerFacing.west){
				PickClosestPos1Teammate(GameEngine.team1);
			} else {
				print ("Error in pass target logic");
			}
		} else if(this.fieldPosition == 4){
			if(this.facing == PlayerFacing.northWest){
				GameEngine.passTarget = GameEngine.team1pos2;
			} else if(this.facing == PlayerFacing.southWest){
				GameEngine.passTarget = GameEngine.team1pos3;
			} else if(this.facing == PlayerFacing.west){
				PickClosestPos1Teammate(GameEngine.team1);
			}
		}
	}
	
	void PassTargetingLogicTeam2(){
		if(this.fieldPosition == 1 ){
			if(this.facing == PlayerFacing.west){
				GameEngine.passTarget = GameEngine.team2pos4;
			} else if(this.facing == PlayerFacing.northWest){
				GameEngine.passTarget = GameEngine.team2pos2;
			} else if(this.facing == PlayerFacing.southWest){
				GameEngine.passTarget = GameEngine.team2pos3;
			} else {
				PickClosestPos1Teammate(GameEngine.team2);
			}
		} else if(this.fieldPosition == 2){
			if(this.facing == PlayerFacing.west){
				GameEngine.passTarget = GameEngine.team2pos4;
			} else if(this.facing == PlayerFacing.southEast || this.facing == PlayerFacing.southWest){
				GameEngine.passTarget = GameEngine.team2pos3;
			} else if(this.facing == PlayerFacing.east){
				PickClosestPos1Teammate(GameEngine.team2);
			} else {
				print ("Error in pass target logic team2 pos2");
			}
		} else if(this.fieldPosition == 3){
			if(this.facing == PlayerFacing.west){
				GameEngine.passTarget = GameEngine.team2pos4;
			} else if(this.facing == PlayerFacing.northEast || this.facing == PlayerFacing.northWest){
				GameEngine.passTarget = GameEngine.team2pos2;
			} else if(this.facing == PlayerFacing.east){
				PickClosestPos1Teammate(GameEngine.team2);
			} else {
				print ("Error in pass target logic team2 pos3");
			}
		} else if(this.fieldPosition == 4){
			if(this.facing == PlayerFacing.northEast){
				GameEngine.passTarget = GameEngine.team2pos2;
			} else if(this.facing == PlayerFacing.southEast){
				GameEngine.passTarget = GameEngine.team2pos3;
			} else if(this.facing == PlayerFacing.east){
				PickClosestPos1Teammate(GameEngine.team2);
			} else {
				print ("Error in pass target logic team2 pos4");
			}
		}
	}
	
	void PickClosestPos1Teammate(List<Player> list)
	{
		Player closest = null;
		float shortestDist = 1000f;
		foreach(Player p in list){
			if(p == this) continue;
			float testDist = Vector3.Distance(this.transform.position, p.transform.position);
			if(testDist < shortestDist)
			{
				shortestDist = testDist;
				closest = p;
			}
		}
		GameEngine.passTarget = closest;
	}
	
	public bool isFacingBall(){
		Ball tempBall = GameEngine.GetClosestBall(transform.position);
		
		float xDirOfBall = tempBall.vel.x;
		
		if(xDirOfBall < 0.0f){
			if (this.facing == PlayerFacing.northEast || this.facing == PlayerFacing.east ||
			    this.facing == PlayerFacing.southEast){
				return true;
			} else {
				return false;
			}
		} else if (xDirOfBall > 0.0f){
			if (this.facing == PlayerFacing.northWest || this.facing == PlayerFacing.west ||
			    this.facing == PlayerFacing.southWest){
				return true;
			} else {
				return false;
			}
		} else {
			return true;
		}
	}
	
	public void FaceBall() {
		Ball tempBall = GameObject.Find("Ball").GetComponent<Ball>();
		if (tempBall.transform.position.x - transform.position.x < 0f) {
			if (facing == PlayerFacing.east ||
			    facing == PlayerFacing.northEast ||
			    facing == PlayerFacing.southEast) {
				if (tempBall.transform.position.y > 0.7f) {
					facing = PlayerFacing.northWest;
				} else if (tempBall.transform.position.y < -3.3f) {
					facing = PlayerFacing.southWest;
				} else {
					facing = PlayerFacing.west;
				}
			}
		} else {
			if (facing == PlayerFacing.west ||
			    facing == PlayerFacing.northWest ||
			    facing == PlayerFacing.southWest) {
				if (tempBall.transform.position.y > 0.7f) {
					facing = PlayerFacing.northEast;
				} else if (tempBall.transform.position.y < -3.3f) {
					facing = PlayerFacing.southEast;
				} else {
					facing = PlayerFacing.east;
				}
			}
		}
	}
	
	public void shieldEarned(){
		shieldHolder.enableShield();
		isShielded = true;
	}
	
	public void shieldUsed(){
		shieldHolder.disableShield();
		isShielded = false;
		StartCoroutine(TempNoCollide(0.5f));
	}
	
	public void AroundTheWorld(){
		float camLeftLimit = GameEngine.cam.transform.position.x - 7.2f;
		float camRightLimit = GameEngine.cam.transform.position.x + 7.2f;
		Vector3 newVec = this.transform.position;
		
		if(this.transform.position.x < camLeftLimit){
			newVec.x = camRightLimit;
			this.height += 5.0f;
			this.transform.position = newVec;
		} else if (this.transform.position.x > camRightLimit){
			newVec.x = camLeftLimit;
			this.height += 5.0f;
			this.transform.position = newVec;
		}
	}
	
	public IEnumerator WindUpThrow(Vector3 target){
		if(noInput) yield break;
		float endTime = Time.time + 0.12f;
		this.aniState = AniState.Windup;
		windupThr = true;
		while(Time.time < endTime){
			yield return null;
		}
		if(this.aniState == AniState.Windup){
			print ("holla");
			this.aniState = AniState.Throwing;
			float speed = ThrowAt(target);
			heldBall.newThrowToPos(target, speed);
		}
	}
	
	public IEnumerator WindUpPass(){
		if(noInput) yield break;
		float endTime = Time.time + 0.12f;
		this.aniState = AniState.Windup;
		windupPass = true;
		while(Time.time < endTime){
			yield return null;
		}
		if(this.aniState == AniState.Windup){
			print ("passing");
			this.aniState = AniState.Passing;
			heldBall.PassTo(GameEngine.passTarget);
			StartCoroutine(resetAStateNone(0.5f));
		}
	}

	IEnumerator DelayBallPickUp(Ball b){
		aniState = AniState.Crouching;
		kState.state = KineticStates.crouch;
		kState.startTime = Time.time;
		while (Time.time - kState.startTime <= 0.25f) {
			yield return null;
		}
		b.StateHeld (this);
		StandUp();
	}
	
	void PlayerKilled(){
		Vector3 angelSpot = transform.position;
		angelSpot.y += 1f;
		angelSpot.z = -2f;
		GameObject soul = Instantiate(GameEngine.angelPrefab, angelSpot, transform.rotation) as GameObject;
		
		if(team == 1){
			GameEngine.team1.Remove(this);
		} else if (team == 2){
			GameEngine.team2.Remove(this);
		}
		
		if(GameEngine.team1.Count == 0){
			if (Application.loadedLevelName == "classic_japan") {
				Application.LoadLevel("p2_win_2player_screen");
			}
			Application.LoadLevel("lose_screen");
		} else if( GameEngine.team2.Count == 0){
			if (Application.loadedLevelName == "classic_japan") {
				Application.LoadLevel("p1_win_2player_screen");
			}
			Application.LoadLevel("win_screen");
		}
		
		GameObject.Destroy(this.gameObject);
	}
}
