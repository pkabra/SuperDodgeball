using UnityEngine;
using System.Collections;

public enum BallState { rest, free, held, pass, thrown, powered };
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
	public float heightHitbox = 1.6f; // The hitbox of the ball extends this amount up and down on height axis
	
	private float heldHeight = 0.77f;
	private float totalTrajDist = 0.0f; // Total distance from origin to intended target
	private Vector3 passOrigin = Vector3.zero;
	private float heightVel = 0.0f; // velocity along the height axis, used for faking gravity
	private float gravity = -0.9f;
	private float bounciness = 0.85f;
	private int throwerTeam = 0;
	public Vector3 prevPos = Vector3.zero;
	
	
	//private GameObject passTarg = null;
	// Use this for initialization
	void Start () {
		GameEngine.ball = this;
		spriteHolderTrans = this.transform.FindChild("SpriteHolder");
		animator = this.gameObject.GetComponentInChildren<Animator>();
		SuperCamera.target = this.gameObject;
		aniStateID = Animator.StringToHash("state");
	}
	
	// Once per frame
	void FixedUpdate () {
		// Save prev position info
		prevPos = transform.position;
		
		// State specific things
		if(state == BallState.pass){
			PassTrajectoryLogic();
			this.transform.position += vel * passSpeedMult * Time.fixedDeltaTime;
		} else if(state == BallState.thrown || state == BallState.powered ){
			this.transform.position += vel * throwSpeedMult * Time.fixedDeltaTime;
		} else if(state == BallState.free){
			FreeBallinLogic();
			this.transform.position += vel * Time.fixedDeltaTime;
		} else if(state == BallState.rest){
			
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
				if((heightDifference > heightHitbox) || (heightDifference < (heightHitbox * -1f)))
				{
					return; // Do nothing because the ball went over or under the player
				}else if(pOther.aState.state == ActionStates.catching){
					StateHeld(pOther);
				} else {
					VerticalSurfaceBounce(other);
				}
				
				//				foreach(Player p in GameEngine.team1) {
				//					if (pOther.GetInstanceID() == p.GetInstanceID()) {
				//						if (p.aState.state == ActionStates.catching) {
				//							// Caught
				//						} else {
				//							
				//						}
				//					}
				//				}
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
				if((heightDifference > heightHitbox) || (heightDifference < (heightHitbox * -1f)))
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
		}
		//		else if(other.gameObject.layer == 11){ // If other is on the 'OutfieldBoundary' layer
		//			//TEMP
		//			vel = Vector3.zero;
		//			state = BallState.free;
		//		}
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
		
		//		if (holder.facing == PlayerFacing.east) {
		//			pos.x += 0.7f;
		//		} else if (holder.facing == PlayerFacing.west) {
		//			pos.x -= 0.7f;
		//		}
		//		this.transform.position = pos;
		
		//Set ball height and position for being held
		Vector3 pos = new Vector3(heldHeight, 0, 0); // Position of ball relative to holder
		this.transform.parent = holder.transform;
		height = 1.3f;
		this.transform.localPosition = pos;
		
		//Set ball state, velocity to zero, and turn off the collider while being held
		state = BallState.held;
		vel = Vector3.zero;
		this.collider.enabled = false;
	}
	
	public void PassTo(Player target){
		// release ball from holder and enable collider
		this.collider.enabled = true;
		transform.parent = null;
		//Vector3 scale = new Vector3(1f,1f,1f);
		//transform.localScale = scale;
		
		//catcher = target;
		
		//Create a normalized vector to the target with 0 z component
		Vector3 vecToTarg = target.transform.position - transform.position;
		vecToTarg.z = 0f;
		vel = vecToTarg.normalized;
		state = BallState.pass;
		
		//Set trajectory calculation info
		if( target.fieldPosition == 1){
			trajectory = Trajectory.low;  
		} else if( target.fieldPosition == 2){
			// TODO add full pass logic
		}
		passOrigin = this.transform.position;
		//Vector3 originTemp = this.transform.position;
		//passOrigin = originTemp - vel * 0.5f; // Set Origin to 'multiplier' units behind passer
		
		totalTrajDist = Mathf.Max(vecToTarg.magnitude - 0.5f, 0.2f);
		//totalTrajDist = toTarg.magnitude;
		
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
		transform.parent = null;
		
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
		if(height < 0.05f && (heightVel < 0.1f && heightVel > -0.1f)){
			state = BallState.rest;
			height = 0f;
			heightVel = 0f;
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
	}
	
}
