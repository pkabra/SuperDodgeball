using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComputerOpponent : MonoBehaviour {
	
	public class PlayerDecision {
		public Player 	player;
		public bool 	hasTarget = false;
		public Vector3	target = Vector3.zero;
		public float	lastTargetSetTime = -4f;
		
		public bool		initThrow = false;
		
		public PlayerDecision(Player p) {
			player = p;
		}
	}
	
	public GameEngine.Controller 	control = GameEngine.player2;
	public PlayerDecision			controlDecision;
	public List<PlayerDecision>		team;

	// Use this for initialization
	void Start () {
		team = new List<PlayerDecision> ();
	}
	
	void FixedUpdate() {
		if (team.Count < 3) AddPlayersToTeam();
		foreach (PlayerDecision p in team) {
			if (p.hasTarget && controlDecision != p) {
				MoveToTarget(p);
			}
		}
		
		control.h = 0f;
		control.y = 0f;
		control.a = false;
		control.b = false;
		
		// What to do with the rest of the team
		MoveToGeneralLocation();
		
		// What to do with player being controlled
		ControlPlayerMove();
	}
	
	void AddPlayersToTeam() {
		foreach(Player p in GameEngine.team2) {
			team.Add (new PlayerDecision(p));
		}
	}
	
	// Update is called once per frame
	void Update () {
		foreach(PlayerDecision p in team) {
			if (p.player.GetInstanceID() == control.player.GetInstanceID()) {
				controlDecision = p;
			}
		}
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
		if (p.player.kState.state != KineticStates.walk && p.player.kState.state != KineticStates.run) return;
		
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
		foreach(PlayerDecision p in team) {
			if (p.player.GetInstanceID() == control.player.GetInstanceID()) {
				controlDecision = p;
			}
		}

		Ball ball = GameEngine.ballsack[0];
		
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
		} else if (ball.state == BallState.thrown && ball.throwerTeam == 1) {
			bool catchIt = Random.value < 0.3f;
			if (distance < 1f && catchIt) {
				control.b = true;
			}
		} else if (ball.state == BallState.powered) {
			bool catchIt = Random.value < 0.05f;
			if (distance < 1f && catchIt) {
				control.b = true;
			}
		} else if (ball.state == BallState.free || ball.state == BallState.rest) {
			if (ball.transform.position.x > -0.15f) {
				if (distance <= 0.3f) {
					control.b = true;
				} else {
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
			} else if(!controlDecision.hasTarget) {
				controlDecision.target.y = -2.7f + (Random.value * 2.4f);
				controlDecision.target.x = 4f + (Random.value * 4f);
				controlDecision.hasTarget = true;
				controlDecision.lastTargetSetTime = Time.time;
			} else {
				control.h = 0f;
				control.y = 0f;
				if (control.player.transform.position.x + 0.4f < controlDecision.target.x) {
					control.h = 1f;
				} else if (control.player.transform.position.x - 0.4f > controlDecision.target.x) {
					control.h = -1f;
				}
				
				if (control.player.transform.position.y + 0.4f < controlDecision.target.y) {
					control.y = 1f;
				} else if (control.player.transform.position.y - 0.4f > controlDecision.target.y) {
					control.y = -1f;
				}
				
				if (control.h == 0f && control.y == 0f) {
					controlDecision.hasTarget = false;
				}
			}
		}
	}
	
	void PlanAndThrow() {
		if (controlDecision.initThrow) {
			float distance = Mathf.Abs(control.player.transform.position.x);
			if ((control.player.kState.state == KineticStates.run && distance < 2f) ||
			    (control.player.kState.state == KineticStates.walk && distance < 1f)) {
				control.b = true;
				control.a = false;
				control.h = 0f;
				controlDecision.initThrow = false;
				if (control.player.kState.state == KineticStates.run) {
					control.player.kState.state = KineticStates.walk;
				}
			} else {
				control.h = -1f;
			}
			return;
		}
		
		controlDecision.initThrow = true;
		controlDecision.target = control.player.transform.position;
		controlDecision.target.x = 0.3f;
		if (control.player.transform.position.x > 4f) {
			control.player.kState.state = KineticStates.run;
			control.player.runDir = -1;
		}
		control.h = -1f;
	}
	
	void ControlSidelinePlayer() {
		control.a = true;
	}
}
