using UnityEngine;
using System.Collections;

public enum PlayerState{stand, walk, run, catching, passing};
public enum PlayerFacing{northEast, east, southEast, southWest, west, northWest};

public class Player : MonoBehaviour {
	public float catchingTime = 0.4f;
	public Vector3 pos0 = Vector3.zero;
	public Vector3 vel = Vector3.zero;
	public PlayerState state = PlayerState.stand;
	public PlayerFacing facing = PlayerFacing.east;
	private bool xLock = false;
	private bool yLock = false;
	
	// Use this for initialization
	void Start () {
		if (CompareTag("Team1")) {
			GameEngine.team1.Add(this);
		} else {
			GameEngine.team2.Add(this);
		}
	}
	
	IEnumerator AttemptCatch(){
		float endTime = Time.time + catchingTime;
		state = PlayerState.catching;
		while(Time.time < endTime){
			yield return null;
		}
		if(state == PlayerState.catching){
			state = PlayerState.stand;
		}
	}
	
	IEnumerator AttemptCatchAtTime(float catchTime){
		while(Time.time < catchTime){
			yield return null;
		}
		AttemptCatch ();
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
	
	// Update is called once per frame
	public void Movement (float h, float v) {
		// Protection from overriding collision resolution
		if(xLock){
			xLock = false;
			h = 0f;
		}
		if(yLock){
			yLock = false;
			v = 0f;
		}
		
		//Store previous position
		pos0 = this.transform.position;
		
		// Movement
		Vector3 pos1 = Vector3.zero;
		vel.x = h * 0.1f;
		vel.y = v * 0.1f;
		pos1 = pos0 + vel;
		this.transform.position = pos1;
	}
}
