using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEngine : MonoBehaviour {
	
	public static List<Player>	team1;
	public static List<Player> 	team2;

	public Player 				player1;
	private float 				lastPlayer1ChangeTime;

	public Player				player2;
	private float 				lastPlayer2ChangeTime;
	
	public static Ball			ball;
	
	// Use this for initialization
	void Awake () {
		team1 = new List<Player>();
		team2 = new List<Player>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//// Handle Player 1
		if (player1 == null) {
			player1 = team1[0];
			lastPlayer1ChangeTime = Time.time;
		}

		foreach(Player p in team1) {
			// Calculate distance between ball and player
			float deltaA = Vector3.Distance(p.transform.position, ball.transform.position);
			float deltaB = Vector3.Distance(player1.transform.position, ball.transform.position);

			if (deltaA < deltaB && Time.time - lastPlayer1ChangeTime > 2f) {
				player1 = p;
				lastPlayer1ChangeTime = Time.time;
			}
		}

		// Movement
		float h1 = Input.GetAxisRaw("Horizontal");
		float y1 = Input.GetAxisRaw("Vertical");
		player1.Movement(h1, y1);

		//// Handle Player 2
		if (player2 == null) {
			player2 = team2[0];
			lastPlayer2ChangeTime = Time.time;
		}
		
		foreach(Player p in team2) {
			// Calculate distance between ball and player
			float deltaA = Vector3.Distance(p.transform.position, ball.transform.position);
			float deltaB = Vector3.Distance(player2.transform.position, ball.transform.position);
			
			if (deltaA < deltaB && Time.time - lastPlayer2ChangeTime > 2f) {
				player2 = p;
				lastPlayer2ChangeTime = Time.time;
			}
		}
		
		// Movement
		float h2 = Input.GetAxisRaw("Horizontal2");
		float y2 = Input.GetAxisRaw("Vertical2");
		player2.Movement(h2, y2);

	}
}
