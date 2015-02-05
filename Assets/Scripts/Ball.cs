using UnityEngine;
using System.Collections;

public enum BallState { rest, free, held, pass, thrown, powered, superpowered };
public enum PowerMode { none, fastball, wreckingball, wave, breaker, tsunami, corkscrew, vampire, hadouken};
public enum Trajectory { none, jump, low, mid, EastWestHigh, NorthSouthHigh };

public class Ball : MonoBehaviour {
	public Vector3 vel = Vector3.zero;
	public float height = 0.0f;
	public BallState state = BallState.rest;
	public PowerMode mode = PowerMode.none;
	public Trajectory trajectory = Trajectory.none;
	
	public Player holder = null;
	//public Player catcher = null;
	public Transform spriteHolderTrans = null;
	public Animator animator = null;
	private int aniStateID = 0;
	
	// All variables below this point might be able to be moved inside functions,
	// check on a case by case basis before doing so.
	public float passSpeedMult = 4f;
	public float throwSpeedMult = 5f;
	public float catchRadius = 0.5f;
	public float YCompOfZ = 0.5f; // The Y component of the Height axis, NOT the worldspace Z
	public float trajMult = 1.0f;
	public float maxHeight = 0.0f; // Max height of a trajectory
	
	public float heldOffsetX = 0.35f;
	private float heldHeight = 1.3f;
	private float totalTrajDist = 0.0f; // Total distance from origin to intended target
	private Vector3 passOrigin = Vector3.zero;
	private float heightVel = 0.0f; // velocity along the height axis, used for faking gravity
	private float gravity = -0.9f;
	private float bounciness = 0.85f;
	private float timeStamp = 0.0f;
	public int throwerTeam = 0;
	private int wreckingMode = 0;
	private int breakerMode = 0;
	public Vector3 prevPos = Vector3.zero;
	private Vector3 targetPos = Vector3.zero;
	
	private GameObject shadow = null;
	
	
	void Awake() {
		spriteHolderTrans = this.transform.FindChild("SpriteHolder");
		animator = this.gameObject.GetComponentInChildren<Animator>();
		SuperCamera.target = this.gameObject;
		aniStateID = Animator.StringToHash("state");
		shadow = this.transform.FindChild("ballShadow").gameObject;
	}
	
	// Use this for initialization
	void Start () {
		GameEngine.ballsack.Add(this);
		animator.SetInteger(aniStateID, (int)this.mode);
	}
	
	// Once per frame
	void FixedUpdate () {
		// Save prev position info
		prevPos = transform.position;
		
		shadow.renderer.enabled = true;
		
		// State specific things
		if(state == BallState.pass){
			PassTrajectoryLogic();
			if(state == BallState.pass){
				this.transform.position += vel * passSpeedMult * Time.fixedDeltaTime;
			} else if(state == BallState.free){
				FreeBallinLogic();
				this.transform.position += vel * Time.fixedDeltaTime;
			}
		} else if(state == BallState.thrown || state == BallState.powered || state == BallState.superpowered){
			if(trajectory == Trajectory.jump){
				if(!JumpThrowLogic())return;
			}
			if(state == BallState.powered || state == BallState.superpowered){
				PowerBallLogic();
			}
			this.transform.position += vel * throwSpeedMult * Time.fixedDeltaTime;
		} else if(state == BallState.free){
			FreeBallinLogic();
			this.transform.position += vel * Time.fixedDeltaTime;
			if (holder) {
				holder.heldBall = null;
				holder = null;
			}
		} else if(state == BallState.held){
			shadow.renderer.enabled = false;
		}
		
		if (state == BallState.rest) {
			if (holder) {
				holder.heldBall = null;
				holder = null;
			}
		}
		
		// Block for things that should always happen
		Vector3 heightOffset = new Vector3( 0, height * YCompOfZ, (transform.position.y - 0.2f) * 0.001f); // z portion orders sprites
		spriteHolderTrans.localPosition = heightOffset;
		animator.SetInteger(aniStateID, (int)this.mode);
		
	}
	
	
	
	void OnTriggerEnter(Collider other){
		BallCollisionLogic(other);
	}
	
	void OnTriggerStay(Collider other){
		BallCollisionLogic(other);
	}
	
	public void StateHeld(Player newHolder){
		//Assign holder, set that player's state to 'holding', and give control to that player
		holder = newHolder;
		newHolder.heldBall = this;
		holder.aState.state = ActionStates.holding;
		GameEngine.ChangeControl(holder.tag);
		
		//Set ball state, velocity to zero, and turn off the collider while being held
		state = BallState.held;
		mode = PowerMode.none;
		vel = Vector3.zero;
		this.collider.enabled = false;
	}
	
	public void PassTo(Player target){
		//Create a normalized vector to the target with 0 z component
		Vector3 vecToTarg = target.transform.position - transform.position;
		vecToTarg.z = 0f;
		vel = vecToTarg.normalized;
		state = BallState.pass;
		height = holder.height * 0.5f + heldHeight;
		
		//Set trajectetory calculation info
		if(holder.fieldPosition == 1){
			if(target.fieldPosition == 4){
				vecToTarg -= vecToTarg.normalized;
				trajectory = Trajectory.EastWestHigh;
			} else if (target.fieldPosition == 3 || target.fieldPosition == 2){
				trajectory = Trajectory.mid;
			} else if (target.fieldPosition == 1){
				trajectory = Trajectory.low;
			} else {
				print ("Pass Logic Error");
			}
		} else if( holder.fieldPosition == 2 || holder.fieldPosition == 3){
			if(target.fieldPosition == 4){
				trajectory = Trajectory.low;
			} else if (target.fieldPosition == 3 || target.fieldPosition == 2){
				trajectory = Trajectory.NorthSouthHigh;
			} else if (target.fieldPosition == 1){
				trajectory = Trajectory.mid;
			} else {
				print ("Pass Logic Error");
			}
		} else if( holder.fieldPosition == 4){
			if(target.fieldPosition == 3 || target.fieldPosition == 2){
				trajectory = Trajectory.low;	
			} else if (target.fieldPosition == 1){
				trajectory = Trajectory.EastWestHigh;
			} else {
				print ("Pass Logic Error");
			}
		}
		
		passOrigin = this.transform.position;
		totalTrajDist = Mathf.Max(vecToTarg.magnitude - 0.2f, 0.2f);
		if(trajectory == Trajectory.EastWestHigh){
			if(target.fieldPosition == 4){
				totalTrajDist += 0.2f;
			} else {
				totalTrajDist -=0.2f;
			}
		}
		
		//Change holder info 
		holder.aState.state = ActionStates.passing;
		holder.aState.startTime = Time.time;
		StartCoroutine(holder.TempNoCollide(0.2f)); // Prevent holder from hitting self
		print ("heldball is null");
		holder.heldBall = null;
		holder = null;
		
		//determine time for ball to get to target
		float distToTarg = ((Vector2)this.transform.position - (Vector2)target.transform.position).magnitude;
		float timeToTarg = distToTarg / ((vel *  passSpeedMult).magnitude);
		target.AttemptCatchAtTime(Time.time + timeToTarg );
	}
	
	// Throw the ball at a target position at a velocity relative to the multiplier passed in.
	// The ball is not affected by gravity during a throw.
	public void ThrowToPos(Vector3 targPos, float velMult){
		if(!holder){
			newThrowToPos(targPos, velMult);
		} else {
			StartCoroutine(holder.WindUpThrow(targPos));
		}
	}
	
	public void newThrowToPos(Vector3 targPos, float velMult)
	{
		// release ball from holder and enable collider
		this.collider.enabled = true;
		targetPos = targPos;
		
		//Vector3 pos = this.transform.position;
		//transform.position = pos;
		//height = 1f;
		//pos.y -= 0.5f;
		
		// set new ball state and velocity in direction of target
		Vector3 dir = targPos - this.transform.position;
		dir.z = 0;
		vel = dir.normalized * velMult;
		
		if(holder && holder.height > 0.001f){
			trajectory = Trajectory.jump;
			totalTrajDist = dir.magnitude + 0.3f;
			maxHeight = holder.height * 0.5f + 1.3f;
			height = maxHeight;
		} else {
			trajectory = Trajectory.none;
		}
		
		if (holder) {
			// Player sets ball state when he throws, this done for powered shot implementation
			if(holder.team == 1){
				throwerTeam = 1;
			} else {
				throwerTeam = 2;
			}
			
			// set throwing player to 'throwing' state then remove holder
			holder.StateThrowing();
			holder.heldBall = null;
			holder = null;
		}
	}
	
	// This function contains the logic for how the ball should bounce off a vertical surface.
	// Outer boundaries and players are considered vertical surfaces. It should be possible 
	// for the ball to strike vertical surface anytime it is in motion.
	void VerticalSurfaceBounce(Collider other){
		//Set up a raycast
		Vector3 dir = (other.transform.position - this.transform.position).normalized;
		//		Vector3 origin = prevPos + transform.lossyScale.magnitude * dir;
		Ray ray = new Ray(prevPos, dir);
		RaycastHit hit;
		other.Raycast(ray, out hit, 20.0f);
		Vector3 norm = hit.normal;
		
		// Bounce off the surface
		vel = Vector3.Reflect(vel, norm) * 0.8f;
		state = BallState.free;
		mode = PowerMode.none;
		wreckingMode = 0;
		breakerMode = 0;
		
		// Added height for bounce off of a player
		if(other.gameObject.GetComponent<Player>()){
			heightVel = 10.0f;
		}
	}
	
	void PassPlayerBounce(Collider other){
		float dir = other.transform.position.x - this.transform.position.x;
		
		// Bounce off the surface
		vel = Vector3.Reflect(vel, new Vector3(-dir, 0f, 0f)) * 0.8f;
		state = BallState.free;
		
		// Added height for bounce off of a player
		if(other.gameObject.GetComponent<Player>()){
			heightVel = 10.0f;
		}
	}
	
	// This function allows the ball to fall responding to 'gravity' and bounce off the ground.
	// The ball loses velocity with each bounce relative to the 'bounciness' variable.
	void FreeBallinLogic(){
		
		// Put ball at rest if it is moving super slow
		if(height < 0.05f && (heightVel < 0.38f && heightVel > 0.33f)){
			state = BallState.rest;
			mode = PowerMode.none;
			wreckingMode = 0;
			breakerMode = 0;
			height = 0f;
			heightVel = 0f;
			if(GameEngine.resetBallOn && (GameEngine.sideline.isBeyondAny(transform.position))){
				ResetBall();
			}
		} else {
			heightVel += gravity;
			height += heightVel * Time.fixedDeltaTime;
			if(height < 0f){
				// Ball has hit the ground
				height = 0f;
				heightVel = -heightVel * bounciness * 0.8f;
				vel *= bounciness * 0.6f;
			}
		}
		
		if(vel.magnitude < 0.05f){
			vel = Vector3.zero;
		}
	}
	
	// The following trajectories have their heights above the ground implented as functions of distance
	// from the origin of the pass rather than gravity and time. The trajectory function is only applied when
	// the ball is in the 'pass' state. If the ball hits something while in 'pass' state it will transition
	// to the 'free' state and become affected by gravity.
	void PassTrajectoryLogic(){
		
		float distFromOrigin = (transform.position - passOrigin).magnitude;
		float halfDist = totalTrajDist / 2f;
		float distFromMidpoint = distFromOrigin - halfDist;
		float halfDistSqrd = Mathf.Pow(halfDist, 2f);
		float percentOfMax = 1f - (Mathf.Pow (distFromMidpoint,2f) / halfDistSqrd);
		float offset = 5.0f;
		float maxHeightMult = 0.06f * (halfDistSqrd + offset);
		
		offset = height; // reuse of offset variable
		
		// These are different types of trajectories. It seemed that passes from different
		// positions resulted in different trajectories. Feel free to adjust the unnamed multiplier
		// to get the appropriate look for your trajectory.
		if(trajectory == Trajectory.EastWestHigh){
			maxHeight = 5.5f * maxHeightMult;
			height = maxHeight * percentOfMax + heldHeight;
			StartCoroutine(BallTempNoCollide(0.3f));
		} else if(trajectory == Trajectory.NorthSouthHigh){
			maxHeight = 12.0f * maxHeightMult;
			height = maxHeight * percentOfMax + heldHeight;
			StartCoroutine(BallTempNoCollide(0.3f));
		} else if(trajectory == Trajectory.mid){
			maxHeight = 4.0f * maxHeightMult;
			height = maxHeight * percentOfMax + heldHeight;
			collider.enabled = true;
		} else if(trajectory == Trajectory.low){
			maxHeight = 3.0f * maxHeightMult;
			height = maxHeight * percentOfMax + heldHeight;
			collider.enabled = true;
		} else {
			print ("Error in PassTrajectoryLogic()");
		}
		
		if(height <= 0){
			heightVel = (height - offset) / Time.fixedDeltaTime;
			state = BallState.free;
		}
	}
	
	public IEnumerator BallTempNoCollide(float secs){
		float endTime = Time.time + secs;
		this.collider.enabled = false;
		while(Time.time < endTime){
			yield return null;
		}
		this.collider.enabled = true;
	}
	
	void BallOnBallAction(Collider other){
		Vector3 norm = other.transform.position + ((SphereCollider)other).center * other.transform.lossyScale.x - prevPos;
		norm.z = 0f;
		// Bounce off the surface
		vel = Vector3.Reflect(vel, norm);
		state = BallState.thrown;
		Shield theShield = other.GetComponent<Shield>();
		theShield.myPlayer.shieldUsed();
		theShield.disableShield();
		if(throwerTeam == 1){
			throwerTeam = 2;
		} else {
			throwerTeam = 1;
		}
	}
	
	bool JumpThrowLogic(){
		float distToTarget = Vector3.Distance(transform.position, targetPos);
		float offset = height;
		float sign = ((targetPos.x - transform.position.x) * vel.x) > 0f ? 1f : -1f;
		height = sign * maxHeight * distToTarget / totalTrajDist;
		if(height <= 0f){
			heightVel = 8.0f; // This is how hard the ball bounces off the ground
			vel *= 3.5f; // This is how much velocity the ball maintains after first bounce
			state = BallState.free;
			mode = PowerMode.none;
			this.transform.position += vel * bounciness * Time.fixedDeltaTime;
			return false;
		}
		return true;
	}
	
	void BallCollisionLogic(Collider other){
		if(state == BallState.held){ // Do nothing if ball held
			return;
		}
		
		int othersLayer = other.gameObject.layer; 
		Player pOther = other.GetComponent<Player>(); // maybe null if the 'other' is not a player
		
		// Special cases for when player not to be hit by ball
		if(pOther && pOther.noBallHit){
			return;
		}
		
		if(state == BallState.pass){
			if(othersLayer == 9){ // If other is on the 'Players' layer
				float heightDifference = height - pOther.height + heldHeight; // 
				if((heightDifference > pOther.heightHitbox) || heightDifference < 0f)
				{
					return; // Do nothing because the ball went over or under the player
				}else if(pOther.aState.state == ActionStates.catching && pOther.isFacingBall()){
					StateHeld(pOther);
				} else {
					PassPlayerBounce(other);
				}
			} else if(othersLayer == 11){
				VerticalSurfaceBounce(other);
			}
		} else if (state == BallState.thrown || state == BallState.powered || state == BallState.superpowered){
			if(pOther){
				float heightDifference = height - pOther.height + heldHeight; // 
				if(throwerTeam == pOther.team ){
					return; // Do not hit own player with thrown ball
				}
				
				if(state == BallState.powered && mode == PowerMode.wreckingball){
					if(wreckingMode == 0){
						timeStamp = Time.time;
						wreckingMode = 1;
					} 
					pOther.PlayerHit(this);
					return;
				}
				
				if((heightDifference > pOther.heightHitbox) || heightDifference < 0f){
					return; // Do nothing because the ball went over or under the player
				} else if(pOther.aState.state == ActionStates.catching && pOther.isFacingBall()){
					if(state != BallState.thrown && GameEngine.customStatic){
						pOther.shieldEarned();
					}
					StateHeld(pOther);
				} else {
					if(pOther.fieldPosition == 1){
						if(!pOther.isShielded || !pOther.isFacingBall()){
							pOther.PlayerHit(this);
						} else {
							// Do nothing, the player is shielded and facing the ball
							// The shield will take care of itself
						}
					}
					VerticalSurfaceBounce(other);
				}
			} else if(othersLayer == 11){
				VerticalSurfaceBounce(other);
			} else if(othersLayer == 14){ // The ball has hit a shield!
				BallOnBallAction(other); 
			}
		} else if (state == BallState.free){
			if(pOther){
				if(pOther.aState.state == ActionStates.catching && pOther.isFacingBall()){
					StateHeld(pOther);
				}
			} else {
				VerticalSurfaceBounce(other);
			}
		}
	}
	
	void WaveBallLogic(){
		Vector3 waveVec = new Vector3(vel.x , Mathf.Sin((Time.time - timeStamp)* 10.0f + Mathf.PI / 2f), 0f);
		vel = waveVec;
	}
	
	void TsunamiLogic(){
		height = Mathf.Sin((Time.time - timeStamp)* 10.0f + Mathf.PI / 2f) + 1.3f;
	}
	
	void CorkscrewLogic(){
		Vector3 waveVec = new Vector3(vel.x , 0.5f * Mathf.Sin((Time.time - timeStamp)* 16.0f + Mathf.PI / 2f), 0f);
		vel = waveVec;
		height += Mathf.Sin((Time.time - timeStamp)* 10.0f + Mathf.PI / 2f);
	}
	
	void WreckingBallLogic(){
		mode = PowerMode.wreckingball;
		if(wreckingMode == 1){
			float t = Time.time - timeStamp;
			height += t * 3.0f;
			if(height > 24f){
				state = BallState.free;
				mode = PowerMode.none;
				wreckingMode = 0;
			}
		} else { return; }
	}
	
	void PowerBallLogic(){
		switch(mode){
		case PowerMode.fastball: 
			break; // current don't have to do anything for this powerthrow
		case PowerMode.wreckingball: 
			WreckingBallLogic();
			break;
		case PowerMode.wave:
			WaveBallLogic();
			break;
		case PowerMode.breaker:
			BreakerballLogic();
			break;
		case PowerMode.tsunami:
			TsunamiLogic();
			break;
		case PowerMode.corkscrew:
			CorkscrewLogic();
			break;
		default:
			print ("Didn't do anything in PowerBallLogic()");
			break;
		}
	}
	
	void BreakerballLogic(){
		if(breakerMode == 0){
			timeStamp = Time.time;
			if(throwerTeam == 1){
				vel = new Vector3(1.0f, 0f, 0f);
			} else {
				vel = new Vector3(-1.0f, 0f, 0f);
			}
			breakerMode = 1;
		}else if(breakerMode == 1){
			if(Time.time >= (timeStamp + 1.5f)){
				breakerMode = 2;
			}
		}else if(breakerMode == 2){
			Vector3 dir = targetPos - transform.position;
			dir.z = 0;
			vel = dir.normalized * 1.7f;
			breakerMode = 4;
		}else {
			// Do nothing
		}
		print (breakerMode);
	}
	
	public float getThrowSpeed(){
		if (!holder) return 1f; 
		float speed = 0f;
		if(state == BallState.thrown){
			if(this.holder.kState.state == KineticStates.run){
				speed = 1.2f;
			} else if( this.holder.kState.state == KineticStates.walk){
				speed = 1.0f;
			} else {
				speed = 0.9f;
			}
		} else {
			switch(mode){
			case PowerMode.breaker:
				speed = 0.9f;
				break;
			case PowerMode.fastball:
				speed = 1.9f;
				break;
			case PowerMode.wave:
				speed = 1.5f;
				break;
			case PowerMode.wreckingball:
				speed = 1.5f;
				break;
			case PowerMode.tsunami:
				speed = 1.5f;
				break;
			case PowerMode.corkscrew:
				speed = 1.75f;
				break;
			default:
				print ("Ran out of powermodes in getThrowSpeed()");
				break;
			}
		}
		return speed;
	}
	
	void ResetBall(){
		this.transform.position = new Vector3(0f, -1f, -1f);
	}
	
}
