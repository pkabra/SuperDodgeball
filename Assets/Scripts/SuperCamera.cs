using UnityEngine;
using System.Collections;

public class SuperCamera : MonoBehaviour {
	
	public float leftLimit = -4.94f;
	public float rightLimit = 4.94f;
	public static GameObject target = null;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float newXpos = target.transform.position.x;
		if (GameEngine.ball.holder) {
			newXpos = CalculateCameraPosition();
		} else if (GameEngine.ball.vel.x > 0f && transform.position.x > newXpos) {
			newXpos = transform.position.x;
		} else if (GameEngine.ball.vel.x < 0f && transform.position.x < newXpos) {
			newXpos = transform.position.x;
		}
		
		if(newXpos > rightLimit){ 
			newXpos = rightLimit; 
		} else if (newXpos < leftLimit){
			newXpos = leftLimit;
		}
		
		Vector3 newPos = new Vector3(newXpos, this.transform.position.y, this.transform.position.z);
		float lerpMag = (this.transform.position - newPos).magnitude < 0.1f ? 0.25f : 0.1f;
		if((this.transform.position - newPos).magnitude < 0.05f){
			return;
		}else {
			this.transform.position = Vector3.Lerp(this.transform.position, newPos, lerpMag);
		}
	}
	
	float CalculateCameraPosition() {
		Player player = GameEngine.ball.holder;
		if (player.team == 1) {
			if (player.facing == PlayerFacing.east ||
			    player.facing == PlayerFacing.northEast ||
			    player.facing == PlayerFacing.southEast) {
				return player.transform.position.x + 6.5f;
			}
			return player.transform.position.x;
		}
		
		if (player.facing == PlayerFacing.west ||
		    player.facing == PlayerFacing.northWest ||
		    player.facing == PlayerFacing.southWest) {
			return player.transform.position.x - 6.5f;
		}
		return player.transform.position.x;
	}
}
