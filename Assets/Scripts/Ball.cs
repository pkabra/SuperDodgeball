using UnityEngine;
using System.Collections;

public enum BallState { rest, free, held, pass, thrown };

public class Ball : MonoBehaviour {
	public float grav = -10.0f;
	public float passSpeedMult = 0.08f;
	public float throwSpeedMult = 0.08f;
	public float catchRadius = 0.5f;
	public Vector3 vel = Vector3.zero;
	public Vector3 accel = Vector3.zero;
	public Player holder = null;
	public Player catcher = null;
	public BallState state = BallState.rest;

	//private GameObject passTarg = null;
	// Use this for initialization
	void Start () {
		GameEngine.ball = this;
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
		holder = newHolder;
		holder.aState.state = ActionStates.holding;
		GameEngine.ChangeControl(holder.tag);
		Vector3 pos = holder.transform.position;
		if (holder.facing == PlayerFacing.east) {
			pos.x += 0.7f;
		} else if (holder.facing == PlayerFacing.west) {
			pos.x -= 0.7f;
		}
		this.transform.position = pos;
		this.transform.parent = holder.transform;
		print (transform.position.z);
		state = BallState.held;
		vel = Vector3.zero;
		this.collider.enabled = false;
	}
		
	// Once per frame
	void FixedUpdate () {
		if(state == BallState.pass){
			this.transform.position += vel * passSpeedMult * Time.fixedTime;
		} else if(state == BallState.rest){
			
		} else if(state == BallState.thrown){
			this.transform.position += vel * throwSpeedMult * Time.fixedTime;
		}
		
	}
	
	public void PassTo(Player target){
		this.collider.enabled = true;
		transform.parent = null;
		Vector3 scale = new Vector3(1f,1f,1f);
		transform.localScale = scale;

		catcher = target;

		//Create a normalized vector to the target with 0 z component
		Vector3 toTarg = target.transform.position - this.transform.position;
		toTarg.z = 0f;
		toTarg.Normalize();
		vel = toTarg;
		state = BallState.pass;
		holder.aState.state = ActionStates.passing;
		holder.aState.startTime = Time.time;
		holder = null;
	
		
		//determine time for ball to get to target
//		float distToTarg = ((Vector2)this.transform.position - (Vector2)target.transform.position).magnitude;
//		float timeToTarg = distToTarg / ((vel *  passMult).magnitude * 100f);
//		print (timeToTarg);
//		target.AttemptCatchAtTime(Time.time + timeToTarg );
	}

	public void ThrowToPos(Vector3 targPos, float velMult){
		targPos.z = 0;
		vel = targPos.normalized * velMult;
		state = BallState.thrown;
	}

}
