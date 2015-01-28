using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour {

	public Player myPlayer = null;

	// Use this for initialization
	void Start () {
		//enableShield();
		this.collider.enabled = false;
		this.renderer.enabled = false;
	}
	
	public void disableShield(){
		this.collider.enabled = false;
		StartCoroutine(fadeOut());
	}

	public void enableShield(){
		this.collider.enabled = true;
		StartCoroutine(fadeIn());
	}

	IEnumerator fadeIn(){
		float endTime = Time.time + 0.6f;
		while(Time.time < endTime){
			this.renderer.enabled = !this.renderer.enabled;
			yield return null;
		}
		this.renderer.enabled = true;
	}

	IEnumerator fadeOut(){
		float endTime = Time.time + 0.6f;
		while(Time.time < endTime){
			this.renderer.enabled = !this.renderer.enabled;
			yield return null;
		}
		this.renderer.enabled = false;
	}
}
