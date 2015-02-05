using UnityEngine;
using System.Collections;

public class Sideline : MonoBehaviour {
	public enum Side{none, left, right, top, bottom};

	public float slopeLeft = 0.0f;
	public float slopeRight = 0.0f;

	public Vector2 pointOnLeft = Vector2.zero;
	public Vector2 pointOnRight = Vector2.zero;

	public GameObject boundaryNE = null;
	public GameObject boundaryNW = null;
	public GameObject boundarySE = null;
	public GameObject boundarySW = null;

	// Use this for initialization
	void Start () {

		this.pointOnLeft.x = -10.31f;
		this.pointOnLeft.y = -3.02f;
		this.pointOnRight.x = 10.31f;
		this.pointOnRight.y = -3.02f;
			
		this.slopeLeft = (boundaryNW.transform.position.y - boundarySW.transform.position.y) / 
						 (boundaryNW.transform.position.x - boundarySW.transform.position.x);
		this.slopeRight = (boundaryNE.transform.position.y - boundarySE.transform.position.y) / 
			              (boundaryNE.transform.position.x - boundarySE.transform.position.x);

		GameEngine.sideline = this;
	}

	public bool isBeyondLeft(Vector3 pos){
		float xOnLine = pointOnLeft.x + ((pos.y - pointOnLeft.y) / this.slopeLeft);
	
		if(pos.x < xOnLine){
			//print ("I'm beyond the left boundary");
			return true;
		} else {
			return false;
		}
	}

	public bool isBeyondRight(Vector3 pos){
		float xOnLine = pointOnRight.x + ((pos.y - pointOnRight.y) / this.slopeRight);
		
		if(pos.x > xOnLine){
			//print ("I'm beyond the right boundary");
			return true;
		} else {
			return false;
		}
	}

	public bool isBeyondTop(Vector3 pos){
		if(pos.y > 1.15f){
			//print ("I'm beyond top boundary");
			return true;
		} else {
			return false;
		}
	}

	public bool isBeyondBottom(Vector3 pos){
		if(pos.y < -3.7f){
			//print ("I'm beyond bottom boundary");
			return true;
		} else {
			return false;
		}
	}

	public bool isBeyondAny(Vector3 pos){
		if (isBeyondTop(pos)) return true;
		if (isBeyondBottom(pos)) return true;
		if (isBeyondLeft(pos)) return true;
		if (isBeyondRight(pos)) return true;
		return false;
	}

}
