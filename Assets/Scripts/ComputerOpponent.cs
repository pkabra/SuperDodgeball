﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComputerOpponent : MonoBehaviour {
	
	class PlayerDecision {
		public Player 	player;
		public bool 	hasTarget = false;
		public Vector3	target = Vector3.zero;
		public float	lastTargetSetTime = -4f;
		
		public bool		initThrow = false;
		
		public PlayerDecision(Player p) {
			player = p;
		}
	}
	
	GameEngine.Controller 	control = GameEngine.player2;
	PlayerDecision			controlDecision;
	List<PlayerDecision>	team;
	
	// Use this for initialization
	void Start () {
		team = new List<PlayerDecision> ();
	}
	
	void FixedUpdate() {
		if (team.Count < 3) AddPlayersToTeam();
		foreach (PlayerDecision p in team) {
			if (p.hasTarget) {
				MoveToTarget(p);
			}
		}
	}
	
	void AddPlayersToTeam() {
		foreach(Player p in GameEngine.team2) {
			team.Add (new PlayerDecision(p));
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (team.Count < 3) AddPlayersToTeam();
		foreach(PlayerDecision p in team) {
			if (p.player.GetInstanceID() == control.player.GetInstanceID()) {
				p.hasTarget = false;
				controlDecision = p;
			}
		}
		
		// What to do with the rest of the team
		MoveToGeneralLocation();
		
		// What to do with player being controlled
		ControlPlayerMove();
	}
	
	void MoveToGeneralLocation() {
		foreach(PlayerDecision p in team) {
			if (p.player.GetInstanceID() == control.player.GetInstanceID()) continue;
			if (!p.hasTarget && Time.time - p.lastTargetSetTime > 4f) {
				p.target.y = -2.7f + (Random.value * 2.4f);
				p.target.x = 4f + (Random.value * 4f);
				p.hasTarget = true;
				p.lastTargetSetTime = Time.time;
			}
		}
	}
	
	void MoveToTarget(PlayerDecision p) {
		float h = 0f;
		float v = 0f;
		if (p.player.transform.position.x + 0.4f < p.target.x) {
			h = 1f;
		} else if (p.player.transform.position.x - 0.4f > p.target.x) {
			h = -1f;
		}
		
		if (p.player.transform.position.y + 0.4f < p.target.y) {
			v = 1f;
		} else if (p.player.transform.position.y - 0.4f > p.target.y) {
			v = -1f;
		}
		
		if (h == 0f && v == 0f) {
			p.hasTarget = false;
			p.player.FaceBall();
		}
		
		p.player.Movement(h, v);
	}
	
	void ControlPlayerMove() {
		Ball ball = GameEngine.ball;
		
		if (control.player.fieldPosition != 1) {
			ControlSidelinePlayer();
			return;
		}
		
		float distance = Vector3.Distance(ball.transform.position, control.player.transform.position);
		if (ball.state == BallState.held && ball.holder.team == 1) {
			if (control.player.transform.position.x < 5.6f) {
				control.h = 1f;
			} else if (control.player.transform.position.x > 6.2f) {
				control.h = -1f;
			} else {
				control.h = 0f;
			}
		} else if (ball.state == BallState.held && ball.holder.team == 2) {
			PlanAndThrow();
		} else if (ball.state == BallState.thrown) {
			bool catchIt = Random.value < 0.7f;
			if (distance < 1f && catchIt) {
				control.a = true;
			}
		} else if (ball.state == BallState.powered) {
			bool catchIt = Random.value < 0.2f;
			if (distance < 1f && catchIt) {
				control.a = true;
			}
		} else if (ball.state == BallState.free || ball.state == BallState.rest) {
			if (distance <= 0.5f) {
				control.b = true;
			} else {
				if (ball.transform.position.x > -0.01f) {
					float h = 0f;
					float v = 0f;
					if (ball.transform.position.x - control.player.transform.position.x < -0.3f) {
						h = -1f;
					} else if (ball.transform.position.x - control.player.transform.position.x > 0.3f) {
						h = 1f;
					}
					
					if (ball.transform.position.y - control.player.transform.position.y < -0.3f) {
						v = -1f;
					} else if (ball.transform.position.y - control.player.transform.position.y > 0.3f) {
						v = 1f;
					}
					
					control.h = h;
					control.y = v;
				}
			}
		}
	}
	
	void PlanAndThrow() {
		if (controlDecision.initThrow) {
			float distance = Vector3.Distance(controlDecision.target, control.player.transform.position);
			if ((control.player.kState.state == KineticStates.run && distance < 2f) ||
			    (control.player.kState.state == KineticStates.walk && distance < 1f)) {
				control.b = true;
				control.h = 0f;
				controlDecision.initThrow = false;
			} else {
				control.h = -1f;
			}
			return;
		}
		
		controlDecision.initThrow = true;
		controlDecision.target = control.player.transform.position;
		controlDecision.target.x = 0.2f;
		if (control.player.transform.position.x > 4f) {
			control.player.kState.state = KineticStates.run;
		}
		control.h = -1f;
	}
	
	void ControlSidelinePlayer() {
		control.a = true;
	}
}