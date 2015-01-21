using UnityEngine;
using System.Collections;

public enum KineticStates{none, stand, walk, run, crouch, jump ,runjump, fall, stun};
public enum ActionStates{none, throwing, catching, passing, holding};
public enum PlayerFacing{northEast, east, southEast, southWest, west, northWest};

public class ActionState {
	public ActionStates state = ActionStates.none;
	public float startTime = 0f;
}

public class KineticState {
	public KineticStates state = KineticStates.none;
	public float startTime = 0f;
}

public class Player : MonoBehaviour {

	public Vector3 pos0 = Vector3.zero; // previous frame position
	public Vector3 vel = Vector3.zero;
	public float height = 0.0f; // height above ground plane
	public float heightVel = 0f;
	public float bounciness = 0.85f;
	public KineticState kState = new KineticState();
	public ActionState aState = new ActionState();
	public PlayerFacing facing = PlayerFacing.east;

	public Transform spriteHolderTrans = null;

	public float catchingTime = 0.8f;
	public float catchAttemptBuffer = 0.3f;
	public float tryCatchTime = 0f;
	private bool xLock = false;
	private bool yLock = false;
	
	// Use this for initialization
	void Start () {
		if (CompareTag("Team1")) {
			GameEngine.team1.Add(this);
		} else {
			GameEngine.team2.Add(this);
		}
		spriteHolderTrans = this.transform.FindChild("SpriteHolder");
	}

	public void StateThrowing(){
		// Player just threw the ball
		aState.state = ActionStates.throwing;
	}

	
	void OnTriggerEnter(Collider other){
		
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
				Vector3 origin = pos0;
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
					//Cast ray
					Ray ray = new Ray(this.transform.position, dir);
					RaycastHit hit;
					other.Raycast(ray, out hit, 20.0f);
					Vector3 norm = hit.normal;
					//print (norm);
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
				xLock = true;
				desiredPos.x = other.transform.position.x - (boundaryHalfWidth + thisHalfWidth + 0.001f);
			} else if (hitOnRight){ // Hit right side
				xLock = true;
				desiredPos.x = other.transform.position.x + (boundaryHalfWidth + thisHalfWidth + 0.001f);
			} else if (hitOnBottom){ // Hit bottom
				yLock = true;
				desiredPos.y = other.transform.position.y - (boundaryHalfHeight + thisHalfHeight + 0.001f);
			} else if (hitOnTop){ // hit top
				yLock = true;
				desiredPos.y = other.transform.position.y + (boundaryHalfHeight + thisHalfHeight + 0.001f);
			}
			
			this.transform.position = desiredPos;
		}
	}
	
	// Handle Player Movement
	public void Movement (float h, float v) {
		if (kState.state != KineticStates.walk || kState.state != KineticStates.walk) return;
		// Protection from overriding collision resolution
		if(xLock){
			xLock = false;
			h = 0f;
		}
		if(yLock){
			yLock = false;
			v = 0f;
		}

		if (Time.time - kState.startTime < 0.5f || Time.time - aState.startTime < 0.5f) return;
		
		//Store previous position
		pos0 = this.transform.position;
		
		// Movement
		Vector3 pos1 = Vector3.zero;
		if (kState.state == KineticStates.run) {
			vel.x = h * 0.3f;
			vel.y = v * 0.05f;
		} else {
			vel.x = h * 0.1f;
			vel.y = v * 0.1f;
		}
		pos1 = pos0 + vel;

		Quaternion rot = transform.rotation;
		if (h < 0f) {
			if (v < 0f) {
				facing = PlayerFacing.southWest;
			} else if (v > 0f) {
				facing = PlayerFacing.northWest;
			} else {
				facing = PlayerFacing.west;
			}
			rot.y = 180f;
		} else if (h > 0f) {
			if (v < 0f) {
				facing = PlayerFacing.southEast;
			} else if (v > 0f) {
				facing = PlayerFacing.northEast;
			} else {
				facing = PlayerFacing.east;			
			}
			rot.y = 0f;
		}

		transform.position = pos1;
		transform.rotation = rot;
	}

	// Handle Player Picking up Ball
	// Edited to account for height -Steve
	public void PickupBall() {
		Ball theBall = GameEngine.ball;
		float heightDifference = theBall.height - this.height; 
		float delta = Vector3.Distance(transform.position, theBall.transform.position);

		if (delta < 1f && ((heightDifference <= theBall.heightHitbox) && (heightDifference >= (theBall.heightHitbox * -1f)))) {
			theBall.StateHeld (this);
		}

		kState.state = KineticStates.walk;
		kState.startTime = Time.time;

	}

	public void AttemptCatchAtTime(float catchTime){
		tryCatchTime = catchTime;
		print (tryCatchTime);
		StartCoroutine(AttemptCatchAtTimeCR());
	}

	public void ThrowAt(Vector3 targetPos){
		StateThrowing();
		StartCoroutine(TempNoCollide(0.15f));
	}

	IEnumerator AttemptCatch(){
		float endTime = Time.time + catchingTime;
		print ("hands up");
		aState.state = ActionStates.catching;
		while(Time.time < endTime){
			yield return null;
		}
		print ("hands down");
		if(aState.state == ActionStates.catching){
			kState.state = KineticStates.stand;
			aState.state = ActionStates.none;
		}
	}
	
	IEnumerator AttemptCatchAtTimeCR(){
		tryCatchTime -= catchAttemptBuffer;
		while(Time.time < tryCatchTime ){
			//print()
			yield return null;
		}
		StartCoroutine( AttemptCatch() );
	}


	public IEnumerator TempNoCollide(float secs){
		float endTime = Time.time + secs;
		this.collider.enabled = false;
		while(Time.time < endTime){
			yield return null;
		}
		this.collider.enabled = true;
	}

	public void PlayerHit(Ball other) {
		kState.state = KineticStates.fall;

		// Just take the velocity the ball was moving in and multiply by 3
		// We know the ball velocity will be constant
		// Will need to add a clause for power shots
		vel = other.vel * 3;
		
		// Added height for bounce off of a player
		heightVel = 10f;
	}

	void FixedUpdate() {
		if (this.kState.state == KineticStates.fall) {
			PlayerFallLogic();
			this.transform.position += vel * Time.fixedDeltaTime;
		}

		// Block for things that should always happen
		Vector3 heightOffset = new Vector3( 0, height * 0.5f + 1.2f, 0 );
		spriteHolderTrans.localPosition = heightOffset;
	}

	void PlayerFallLogic() {
		if(height < 0.05f && (heightVel < 0.1f && heightVel > -0.1f)){
			kState.state = KineticStates.walk;
			height = 0f;
			heightVel = 0f;
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
}
