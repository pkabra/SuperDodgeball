using UnityEngine;
using System.Collections;

public enum BallState { rest, free, held, pass, thrown };

public class Ball : MonoBehaviour {
	public float grav = -10.0f;
	public float passMult = 1.0f;
	public float catchRadius = 0.5f;
	public Vector3 vel = Vector3.zero;
	public Vector3 accel = Vector3.zero;
	public GameObject holder = null;
	public BallState state = BallState.rest;
	
	private GameObject passTarg = null;
	// Use this for initialization
	void Start () {
		GameEngine.ball = this;
	}
	
	// Once per frame
	void FixedUpdate () {
		if(state == BallState.pass){
			this.transform.position += vel;
			if((this.transform.position - passTarg.transform.position ).magnitude < catchRadius){
				holder = passTarg;
				passTarg = null;
				state = BallState.held;
			}
		}
		
	}
	
	void PassTo(GameObject target){
		//Create a normalized vector to the target with 0 z component
		Vector3 toTarg = target.transform.position - this.transform.position;
		toTarg.z = 0f;
		toTarg.Normalize();
		
		state = BallState.pass;
		passTarg = target;
		holder = null;
		
	}
	
}
