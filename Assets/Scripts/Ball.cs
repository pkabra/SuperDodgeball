using UnityEngine;
using System.Collections;

public enum BallState { rest, free, held, pass, thrown };
public enum Trajectory { none, low, mid, EastWestHigh, NorthSouthHigh };

public class Ball : MonoBehaviour {
	public Vector3 vel = Vector3.zero;
	public float passSpeedMult = 3f;
	public float throwSpeedMult = 5f;
	public float catchRadius = 0.5f;
	public float YCompOfZ = 0.5f; // The Y component of the Height axis, NOT the worldspace Z
	public float height = 0.0f;
	public float trajMult = 1.0f;
	public float maxHeight = 0.0f; // Max height of a trajectory
	
	public BallState state = BallState.rest;
	public Trajectory trajectory = Trajectory.none;
	
	public Player holder = null;
	public Player catcher = null;
	public Transform spriteHolderTrans = null;
	
	private float totalTrajDist = 0.0f; // Total distance from origin to intended target
	private Vector3 passOrigin = Vector3.zero;
	private float heightVel = 0.0f; // velocity along the height axis, used for faking gravity
	private float gravity = -0.07f;
	private float bounciness = 0.5f;
	private Vector3 prevPos = Vector3.zero;
	
	
	//private GameObject passTarg = null;
	// Use this for initialization
	void Start () {
		GameEngine.ball = this;
		spriteHolderTrans = this.transform.FindChild("SpriteHolder");
	}
	
	// Once per frame
	void FixedUpdate () {
		// Save prev position info
		prevPos = transform.position;
		
		// State specific things
		if(state == BallState.pass){
			PassTrajectoryLogic();
			this.transform.position += vel * passSpeedMult * Time.fixedDeltaTime;
		} else if(state == BallState.thrown){
			this.transform.position += vel * throwSpeedMult * Time.fixedDeltaTime;
		} else if(state == BallState.free){
			FreeBallinLogic();
			if(vel.magnitude < 0.02f){
				vel = Vector3.zero;
			}
			this.transform.position += vel * Time.fixedDeltaTime;
		} else if(state == BallState.rest){
			
		}
		
		// Block for things that should always happen
		Vector3 heightOffset = new Vector3( 0, height * YCompOfZ, 0 );
		spriteHolderTrans.localPosition = heightOffset;
		
	}


	
	void OnTriggerEnter(Collider other){
		if(state == BallState.held){ // Do nothing if ball held
			return;
		}

		int othersLayer = other.gameObject.layer; 
		Player pOther = other.GetComponent<Player>(); // maybe null if the 'other' is not a player

		if(state == BallState.pass){
			if(othersLayer == 9){ // If other is on the 'Players' layer
				// get Player component if one exists
				if(pOther.GetInstanceID() == catcher.GetInstanceID()){
					StateHeld(catcher);
				}
				
				foreach(Player p in GameEngine.team1) {
					if (pOther.GetInstanceID() == p.GetInstanceID()) {
						if (p.aState.state == ActionStates.catching) {
							// Caught
						} else {
							
						}
					}
				}
			} else if(othersLayer == 11){
				VerticalSurfaceBounce(other);
			}
		} else if (state == BallState.thrown){
			VerticalSurfaceBounce(other);
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
		
		Vector3 pos = new Vector3(0.77f, 0, 0); // Position of ball relative to holder
		
		//		if (holder.facing == PlayerFacing.east) {
		//			pos.x += 0.7f;
		//		} else if (holder.facing == PlayerFacing.west) {
		//			pos.x -= 0.7f;
		//		}
		//		this.transform.position = pos;
		
		//Set ball height and position for being held
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
		
		catcher = target;
		
		//Create a normalized vector to the target with 0 z component
		Vector3 toTarg = target.transform.position - this.transform.position;
		toTarg.z = 0f;
		vel = toTarg.normalized;
		state = BallState.pass;
		
		//Set trajectory calculation info
		trajectory = Trajectory.NorthSouthHigh;
		Vector3 originTemp = this.holder.transform.position;
		passOrigin = originTemp;
		totalTrajDist = toTarg.magnitude;
		
		//Change holder info 
		holder.aState.state = ActionStates.passing;
		holder.aState.startTime = Time.time;
		holder = null;
		
		
		//determine time for ball to get to target
		//		float distToTarg = ((Vector2)this.transform.position - (Vector2)target.transform.position).magnitude;
		//		float timeToTarg = distToTarg / ((vel *  passMult).magnitude);
		//		print (timeToTarg);
		//		target.AttemptCatchAtTime(Time.time + timeToTarg );
	}
	
	public void ThrowToPos(Vector3 targPos, float velMult){
		// release ball from holder and enable collider
		this.collider.enabled = true;
		transform.parent = null;
		
		// set new ball state and velocity in direction of target
		Vector3 dir = targPos - this.transform.position;
		dir.z = 0;
		vel = dir.normalized * velMult;
		state = BallState.thrown;
		
		// set throwing player to 'throwing' state then remove holder
		holder.StateThrowing();
		holder = null;
	}

	void VerticalSurfaceBounce(Collider other){
		//Set up a raycast
		Vector3 dir = vel.normalized;
		Ray ray = new Ray(prevPos, dir);
		RaycastHit hit;
		other.Raycast(ray, out hit, 20.0f);
		Vector3 norm = hit.normal;
		
		// Bounce off the surface
		vel = Vector3.Reflect(vel, norm) * bounciness;
		state = BallState.free;
	}
	
	void FreeBallinLogic(){
		// Put ball at rest if it is moving super slow
		if(height < 0.01f && (heightVel < 0.01f && heightVel > -0.01f)){
			state = BallState.rest;
			height = 0f;
			heightVel = 0f;
		} else {
			heightVel += gravity;
			height += heightVel;
			if(height < 0f){
				// Ball has hit the ground
				height = 0f;
				heightVel = -heightVel * bounciness;
				vel *= bounciness;
			}
		}
	}
	
	void PassTrajectoryLogic(){
		
		float distFromOrigin = (transform.position - passOrigin).magnitude;
		float halfDist = totalTrajDist / 2f;
		float distFromMidpoint = distFromOrigin - halfDist;
		float halfDistSqrd = Mathf.Pow(halfDist, 2f);
		float percentOfMax = 1f - (Mathf.Pow (distFromMidpoint,2f) / halfDistSqrd);
		float multOffset = 5.0f;
		float maxHeightMult = 0.06f * (halfDistSqrd + multOffset);
		
		if(trajectory == Trajectory.EastWestHigh){
			maxHeight = 14.0f * maxHeightMult;
			height = maxHeight * percentOfMax;
		} else if(trajectory == Trajectory.NorthSouthHigh){
			maxHeight = 24.0f * maxHeightMult;
			height = maxHeight * percentOfMax;
		} else if(trajectory == Trajectory.mid){
			maxHeight = 9.0f * maxHeightMult;
			height = maxHeight * percentOfMax;
		} else if(trajectory == Trajectory.low){
			maxHeight = 3.0f * maxHeightMult;
			height = maxHeight * percentOfMax;
		} else {
			print ("Error in PassTrajectoryLogic()");
		}
	}
	
}
