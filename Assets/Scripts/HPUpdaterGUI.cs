using UnityEngine;
using System.Collections;

public class HPUpdaterGUI : MonoBehaviour {
	private Rect cover;

	void Start () {
		cover = this.GetComponentInChildren<RectTransform>().rect;
	}

	public void UpdateCover(float hp){
		cover.width = 180f - (15f * Mathf.Ceil(hp / 4f));
	}
}
