using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Mathf.Round (Input.GetAxisRaw ("Vertical"));

		// Movement
		Vector3 pos1 = this.transform.position;
		pos1.x += h * 0.1f;
		pos1.y += v * 0.1f;
		this.transform.position = pos1;
	}
}
