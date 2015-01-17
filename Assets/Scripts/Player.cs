using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		
	}
	
	void OnTriggerEnter(Collider other){
		
		if(other.gameObject.tag == "InfieldBoundary"){
			print ("Boundary Collision Detercted");
			Vector3 dir = other.gameObject.transform.position - this.transform.position;
			Ray ray = new Ray(this.transform.position, dir);
			RaycastHit hit;
			other.Raycast(ray, out hit, 20.0f);
			Vector3 norm = hit.normal;
			print (norm);
			if(norm.x > 0){
				print ("right side");
			} else if( norm.x < 0){	
				print ("left side");
			} else if( norm.y > 0){
				print ("top side");
			} else if ( norm.y < 0){
				print ("bottom side");
			} else {
				print ("Raycast sucks.");
			}
		}
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
