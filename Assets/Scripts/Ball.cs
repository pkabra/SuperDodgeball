using UnityEngine;
using System.Collections;

public enum BallState { rest, free, held, pass, thrown, powered, superpowered };
public enum Trajectory { none, low, mid, EastWestHigh, NorthSouthHigh };

public class Ball : MonoBehaviour {
	public Vector3 vel = Vector3.zero;
	public float height = 0.0f;
	public BallState state = BallState.rest;
	public Trajectory trajectory = Trajectory.none;
	
	public Player holder = null;
	public Player catcher = null;
	public Transform spriteHolderTrans = null;
	public Animator animator = null;
	private int aniStateID = 0;
	
	// All variables below this point might be able to be moved inside functions,
	// check on a case by case basis before doing so.
	public float passSpeedMult = 3f;
	public float throwSpeedMult = 5f;
	public float catchRadius = 0.5f;
	public float YCompOfZ = 0.5f; // The Y component of the Height axis, NOT the worldspace Z
	public float trajMult = 1.0f;
	public float maxHeight = 0.0f; // Max height of a trajectory
	
	public float heldOffsetX = 0.5f;
	private float heldHeight = 1.3f;
	private float totalTrajDist = 0.0f; // Total distance from origin to intended target
	private Vector3 passOrigin = Vector3.zero;
	private float heightVel = 0.0f; // velocity along the height axis, used for faking gravity
	private float gravity = -0.9f;
	private float bounciness = 0.85f;
	private int throwerTeam = 0;
	public Vector3 prevPos = Vector3.zero;

	private GameObject shadow = null;
	
	
	//private GameObject passTarg = null;
	// Use this for initialization
	void Start () {
		GameEngine.ball = this;
		spriteHolderTrans = this.transform.FindChild("SpriteHolder");
		animator = this.gameObject.GetComponentInChildren<Animator>();
		SuperCamera.target = this.gameObject;
		aniStateID = Animator.StringToHash("state");
		shadow = this.transform.FindChild("ballShadow").gameObject;
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
		} else if(state == BallState.thrown || state == BallState.powered ){
			this.transform.position += vel * throwSpeedMult * Time.fixedDeltaTime;
		} else if(state == BallState.free){
			FreeBallinLogic();
			this.transform.position += vel * Time.fixedDeltaTime;
		} else if(state == BallState.held){
			shadow.renderer.enabled = false;
			Vector3 pos = holder.transform.position;
			if (holder.facing == PlayerFacing.west || holder.facing == PlayerFacing.northWest || holder.facing == PlayerFacing.southWest) {
				pos.x -= heldOffsetX;
			} else {
				pos.x += heldOffsetX;
			}
			pos.y += holder.height * 0.2f + 0.5f;
			transform.position = pos;
			height = 0f;
		}
		
		// Block for things that should always happen
		Vector3 heightOffset = new Vector3( 0, height * YCompOfZ, 0 );
		spriteHolderTrans.localPosition = heightOffset;
		animator.SetInteger(aniStateID, (int)this.state);
		
	}
	
	
	
	void OnTriggerEnter(Collider other){
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
				float heightDifference = height - pOther.height; // 
				if((heightDifference > pOther.heightHitbox) || heightDifference < 0f)
				{
					return; // Do nothing because the ball went over or under the player
				}else if(pOther.aState.state == ActionStates.catching){
					StateHeld(pOther);
				} else {
					VerticalSurfaceBounce(other);
				}
			} else if(othersLayer == 11){
				VerticalSurfaceBounce(other);
			}
		} else if (state == BallState.thrown || state == BallState.powered){
			if(pOther){
				float heightDifference = height - pOther.height; // 
				if((throwerTeam == 1 && pOther.CompareTag("Team1")) ||
				   (throwerTeam == 2 && pOther.CompareTag("Team2"))){
					return; // Do not hit own player with thrown ball
				}
				if((heightDifference > pOther.heightHitbox) || heightDifference < 0f)
				{
					return; // Do nothing because the ball went over or under the player
				} else if(pOther.aState.state == ActionStates.catching){
					StateHeld(pOther);
				} else {
					pOther.PlayerHit(this);
					VerticalSurfaceBounce(other);
				}
			} else {
				VerticalSurfaceBounce(other);
			}
		} else if (state == BallState.free){
			if(pOther){
				if(pOther.aState.state == ActionStates.catching){
					StateHeld(pOther);
				}
			} else {
				VerticalSurfaceBounce(other);
			}
		}
	}
	
	void OnTriggerStay(Collider other){
		if(state == BallState.held){ // Do nothing if ball held
			return;
		}
		
		// get Player component if one exists
		Player pOther = other.GetComponent<Player>();
		if(pOther){
			if(pOther.aState.state == ActionStates.catching){
				print ("Catching Player tryin to catch");
				StateHeld(pOther);
			}
		}
	}
	
	public void StateHeld(Player newHolder){
		//Assign holder, set that player's state to 'holding', and give control to that player
		holder = newHolder;
		holder.aState.state = ActionStates.holding;
		GameEngine.ChangeControl(holder.tag);
		
		//Set ball state, velocity to zero, and turn off the collider while being held
		state = BallState.held;
		vel = Vector3.zero;
		this.collider.enabled = false;
	}
	
	public void PassTo(Player target){
		// release ball from holder and enable collider
		this.collider.enabled = true;

		//Create a normalized vector to the target with 0 z component
		Vector3 vecToTarg = target.transform.position - transform.position;
		vecToTarg.z = 0f;
		vel = vecToTarg.normalized;
		state = BallState.pass;
		
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
		} else if( holder.fieldPosition == 2 || holder.fieldPosition == 2){
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
			if(target.fieldPosition == 3 || target.fieldPosition == 3){
				trajectory = Trajectory.low;	
			} else if (target.fieldPosition == 1){
				trajectory = Trajectory.EastWestHigh;
			} else {
				print ("Pass Logic Error");
			}
		}

		passOrigin = this.transform.position;
		totalTrajDist = Mathf.Max(vecToTarg.magnitude - 0.2f, 0.2f);
		
		//Change holder info 
		holder.aState.state = ActionStates.passing;
		holder.aState.startTime = Time.time;
		StartCoroutine(holder.TempNoCollide(0.2f)); // Prevent holder from hitting self
		holder = null;
			
		//determine time for ball to get to target
		float distToTarg = ((Vector2)this.transform.position - (Vector2)target.transform.position).magnitude;
		float timeToTarg = distToTarg / ((vel *  passSpeedMult).magnitude);
		target.AttemptCatchAtTime(Time.time + timeToTarg );
	}
	
	// Throw the ball at a target position at a velocity relative to the multiplier passed in.
	// The ball is not affected by gravity during a throw.
	public void ThrowToPos(Vector3 targPos, float velMult){
		// release ball from holder and enable collider
		this.collider.enabled = true;
		
		Vector3 pos = this.transform.position;
		pos.y -= 0.5f;
		transform.position = pos;
		height = 1f;
		
		// set new ball state and velocity in direction of target
		Vector3 dir = targPos - this.transform.position;
		dir.z = 0;
		vel = dir.normalized * velMult;
		// Player sets ball state when he throws, this done for powered shot implementation
		//state = BallState.thrown;
		if(holder.CompareTag("Team1")){
			throwerTeam = 1;
		} else {
			throwerTeam = 2;
		}
		
		// set throwing player to 'throwing' state then remove holder
		holder.StateThrowing();
		holder = null;
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
			height = 0f;
			heightVel = 0f;
			if(GameEngine.sideline.isBeyondTop(this.transform.position) ||
			   GameEngine.sideline.isBeyondBottom(this.transform.position) ||
			   GameEngine.sideline.isBeyondLeft(this.transform.position) ||
			   GameEngine.sideline.isBeyondRight(this.transform.position) ){
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
		float multOffset = 5.0f;
		float maxHeightMult = 0.06f * (halfDistSqrd + multOffset);
		
		// These are different types of trajectories. It seemed that passes from different
		// positions resulted in different trajectories. Feel free to adjust the unnamed multiplier
		// to get the appropriate look for your trajectory.
		if(trajectory == Trajectory.EastWestHigh){
			maxHeight = 12.0f * maxHeightMult;
			height = maxHeight * percentOfMax + heldHeight;
		} else if(trajectory == Trajectory.NorthSouthHigh){
			maxHeight = 16.0f * maxHeightMult;
			height = maxHeight * percentOfMax + heldHeight;
		} else if(trajectory == Trajectory.mid){
			maxHeight = 4.0f * maxHeightMult;
			height = maxHeight * percentOfMax + heldHeight;
		} else if(trajectory == Trajectory.low){
			maxHeight = 3.0f * maxHeightMult;
			height = maxHeight * percentOfMax + heldHeight;
		} else {
			print ("Error in PassTrajectoryLogic()");
		}

		if(height <= 0){
			state = BallState.free;
		}
	}

	void ResetBall(){
		this.transform.position = new Vector3(0f, -1f, -1f);
	}
	
}
