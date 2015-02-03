using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StageSelect : MonoBehaviour {
	
	bool 			select = false;
	bool			selectorOn = true;
	int				selectorFlashDelay = 5;
	
	int[]			stagePos = new int[2];
	int				curStagePos = 0;
	
	public Canvas 	screen1;
	public Canvas 	screen2;
	public Image 	selector;
	
	// Use this for initialization
	void Start () {
		stagePos[0] = 168;
		stagePos[1] = 138;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Return)) {
			if (select) {
				GoToStage();
			} else {
				GoToSelect();
			}
		}
		
		if (select) {
			if (Input.GetKeyDown(KeyCode.W)) {
				StageSelectUp();
			} else if (Input.GetKeyDown(KeyCode.S)) {
				StageSelectDown();
			} else if (Input.GetKeyDown(KeyCode.UpArrow)) {
				StageSelectUp();
			} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
				StageSelectDown();
			}
		}
		
		Vector3 pos = selector.transform.localPosition;
		pos.y = stagePos[curStagePos];
		selector.transform.localPosition = pos;
	}
	
	void FixedUpdate() {
		Vector3 scale = selector.transform.localScale;
		selectorFlashDelay--;
		
		if (selectorFlashDelay == 0) {
			selectorFlashDelay = 5;
			selectorOn = !selectorOn;
		}
		
		if (selectorOn) {
			scale.x = 1;
		} else {
			scale.x = 0;
		}
		
		selector.transform.localScale = scale;
	}
	
	// Loads the user request stage from the stage select screen
	void GoToStage() {
		// Check which stage to load
		string stage;
		if (curStagePos == 0) {
			stage = "classic_japan";
		} else {
			stage = "icestage";
		}
		
		// Load that stage
		Application.LoadLevel(stage);
	}
	
	// Goes from the home page to the stage select screen
	void GoToSelect() {
		screen1.sortingOrder = 0;
		screen2.sortingOrder = 1;
		select = true;
	}
	
	// Move the stage select pointer up
	void StageSelectUp() {
		curStagePos--;
		curStagePos = curStagePos % 2;
		curStagePos = curStagePos < 0 ? curStagePos * -1 : curStagePos;
	}
	
	// Moves the stage select pointer down
	void StageSelectDown() {
		curStagePos++;
		curStagePos = curStagePos % 2;
	}
}
