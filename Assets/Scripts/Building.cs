﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Destroy this to win
public class Building : NetworkBehaviour {
	public Spawn mySpawn;

	[SyncVar(hook="OnChangeHealth")]
	private int hp;
	private int MAXHP = 1000;
	public Transform hpDisplay;

	// Use this for initialization
	void Start()
	{
		hp = MAXHP;
	}

	void Update(){
		if (hpDisplay != null){
			hpDisplay.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
		}
	}

	public int MyTeam(){
		return mySpawn.playerId;
	}

	/*
	* Take amount damage from a source
	*/
	public void TakeDamage(int amount){
		if (!isServer){
			return;
		}

		if (hp > 0){
			hp -= amount;
			Debug.Log("take " + amount + " damage");
			if (hp <= 0){
				// game over for this team. Destroy the building from the local HUD
				transform.GetComponent<Spawn>().BuildingDestroyed();
			}
		}
	}

	public int GetHp(){
		return hp;
	}

	void OnChangeHealth(int newHealth){
		hp = newHealth;
		hpDisplay.localScale = new Vector3(newHealth/(float)MAXHP * 5f, 0.3f, 0.3f);
	}
}
