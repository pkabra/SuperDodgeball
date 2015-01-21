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
		if(newXpos > rightLimit){ 
			newXpos = rightLimit; 
		} else if (newXpos < leftLimit){
			newXpos = leftLimit;
		} 
		Vector3 newPos = new Vector3(newXpos, this.transform.position.y, this.transform.position.z);
		this.transform.position = Vector3.Lerp(this.transform.position, newPos, 0.25f);
	}
}
