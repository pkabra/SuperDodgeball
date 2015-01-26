using UnityEngine;
using System.Collections;

public class HPUpdaterGUI : MonoBehaviour {
	public GameObject child = null;
	private RectTransform cover;

	void Start () {
		cover = child.GetComponentInChildren<RectTransform>();
	}

	public void UpdateCover(float hp){
		cover.sizeDelta = new Vector2(180f - (15f * Mathf.Ceil(hp / 4f)), 20);
	}
}
