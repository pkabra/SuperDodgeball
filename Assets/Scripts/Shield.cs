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

	void FixedUpdate(){
		Vector3 newVec = myPlayer.spriteHolderTrans.position;
		newVec.x = this.transform.position.x;
		this.transform.position = newVec;
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
		int i;
		float endTime = Time.time + 0.6f;
		while(Time.time < endTime){
			i = (int)(Time.time * 40f) % 2;
			if(i == 1){
				this.renderer.enabled = true;
			} else {
				this.renderer.enabled = false;
			}
			yield return null;
		}
		this.renderer.enabled = true;
	}

	IEnumerator fadeOut(){
		int i;
		float endTime = Time.time + 0.6f;
		while(Time.time < endTime){
			i = (int)(Time.time * 40f) % 2;
			if(i == 1){
				this.renderer.enabled = true;
			} else {
				this.renderer.enabled = false;
			}
			yield return null;
		}
		this.renderer.enabled = false;
	}
}
