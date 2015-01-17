using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {
	public Vector3 vel = Vector3.zero;
	public GameObject holder = null;
	public enum state { rest, free, held, pass, thrown };
	// Use this for initialization
	void Start () {
	
	}
	
	// Once per frame
	void FixedUpdate () {
	
	}
}
