using UnityEngine;
using System.Collections;

public class AngelFloaty : MonoBehaviour {
	public float startingX = 0.0f;
	private float startTime = 0.0f;

	// Use this for initialization
	void Start () {
		startTime = Time.time;
		startingX = this.transform.position.x;

		StartCoroutine(NoDrawQuick());
	}
	
	// Update is called once per frame
	void FixedUpdate(){
		float curY = this.transform.position.y;

		Vector3 newPosition = new Vector3(Mathf.Sin (Time.time - startTime * Mathf.PI / 2f) + startingX,
		                                  curY + Time.fixedDeltaTime,
		                                  this.transform.position.z);
		this.transform.position = newPosition;
	}

	IEnumerator NoDrawQuick(){
		float endTime = Time.time + 0.1f;
		this.renderer.enabled = false;
		while(Time.time <= endTime){
			yield return null;
		}
		this.renderer.enabled = true;
	}
}
