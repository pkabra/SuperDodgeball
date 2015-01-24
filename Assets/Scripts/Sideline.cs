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

		this.pointOnLeft.x = -10.8f;
		this.pointOnLeft.y = -6.07f;
		this.pointOnRight.x = 10.8f;
		this.pointOnRight.y = -6.07f;
			
		this.slopeLeft = this.pointOnLeft.y / this.pointOnLeft.x;
		this.slopeRight = this.pointOnRight.y / this.pointOnRight.x;

		GameEngine.sideline = this;

		print (slopeLeft);
		print (slopeRight);
	}

	public bool isBeyondLeft(Vector3 pos){
		float xOnLine = pointOnLeft.x + ((pos.y - pointOnLeft.y) / this.slopeLeft);
	
		if(pos.x < xOnLine){
			print ("I'm beyond the left boundary");
		return true;
		} else {
			return false;
		}
	}

	public bool isBeyondRight(Vector3 pos){
		float xOnLine = pointOnRight.x + ((pos.y - pointOnRight.y) / this.slopeLeft);
		
		if(pos.x > xOnLine){
			print ("I'm beyond the right boundary");
			return true;
		} else {
			return false;
		}
	}

	public bool isBeyondTop(Vector3 pos){
		return pos.y > -2.6f;
	}

	public bool isBeyondBottom(Vector3 pos){
		return pos.y < -2.6f;
	}

}
