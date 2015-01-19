using UnityEngine;
using System.Collections;

public enum BallState { rest, free, held, pass, thrown };
public enum Trajectory { low, mid, high };

public class Ball : MonoBehaviour {
	public float grav = -10.0f;
	public float passSpeedMult = 0.08f;
	public float throwSpeedMult = 0.08f;
	public float catchRadius = 0.5f;
	public Vector3 vel = Vector3.zero;
	public float height = 0.0f;
	public float YCompOfZ = 0.5f; // The Y component of the Height axis, NOT the worldspace Z
	public Vector3 accel = Vector3.zero;
	public Player holder = null;
	public Player catcher = null;
	public BallState state = BallState.rest;

	public Transform spriteHolderTrans = null;

	//private GameObject passTarg = null;
	// Use this for initialization
	void Start () {
		GameEngine.ball = this;
		spriteHolderTrans = this.transform.FindChild("SpriteHolder");
	}

	void OnTriggerEnter(Collider other){
		if(state == BallState.held){ // Do nothing if ball held
			return;
		}

		if(other.gameObject.layer == 9){ // If other is on the 'Players' layer
			// get Player component if one exists
			Player pOther = other.GetComponent<Player>();
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
		}else if(other.gameObject.layer == 11){ // If other is on the 'OutfieldBoundary' layer
			//TEMP
			vel = Vector3.zero;
			state = BallState.free;
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
		
	// Once per frame
	void FixedUpdate () {
		// Block for things that should always happen
		Vector3 heightOffset = new Vector3( 0, height * YCompOfZ, 0 );
		spriteHolderTrans.localPosition = heightOffset;

		// State specific things
		if(state == BallState.pass){
			this.transform.position += vel * passSpeedMult * Time.fixedDeltaTime;
		} else if(state == BallState.rest){
			
		} else if(state == BallState.thrown){
			this.transform.position += vel * throwSpeedMult * Time.fixedDeltaTime;
		}
		
	}
	
	public void PassTo(Player target){
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
		Vector3 dir = targPos - this.transform.position;
		dir.z = 0;
		vel = dir.normalized * velMult;
		state = BallState.thrown;
	}

}
