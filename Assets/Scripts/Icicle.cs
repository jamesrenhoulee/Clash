﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Icicle : NetworkBehaviour {
	private int team;
	// for layermasks, dont change
	private int layerTeamOne = 8;
	private int layerTeamTwo = 9;

	private const int icicleDamage = 1;
	private bool active = false;
	private float destroyTimer = 10f;

	Quaternion initialRot;

	[SyncVar(hook="OnChangePosition")]
    Vector3 realPosition;
    [SyncVar(hook="OnChangeRotation")]
    Quaternion realRotation;
    private float updateInterval;
	
	public void Initialize(int t){
		team = t;
		if (team == 0){
			gameObject.layer = 10;
		}
		else if (team == 1){
			gameObject.layer = 11;
		}
		initialRot = transform.rotation;
		realPosition = transform.position;
		realRotation = transform.rotation;
		active = true;
	}

	// Update is called once per frame
	void Update () {
		if (destroyTimer > 0f){
			destroyTimer -= Time.deltaTime;
		}
		else{
			StartCoroutine(DelayDestroy(0f));
		}

		// smooth position and rotation
		if (isServer){
            updateInterval += Time.deltaTime;
            if (updateInterval > 0.1f) // 10 times per second
            {
                updateInterval = 0;
                CmdSync(transform.position, transform.rotation);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 1f);
        }
	}

	
	[Command]
    void CmdSync(Vector3 position, Quaternion rotation)
    {
        realPosition = position;
        realRotation = rotation;
    }

	void OnChangePosition(Vector3 pos){
		realPosition = pos;
	}

	void OnChangeRotation(Quaternion rot){
		realRotation = rot;
	}

	[Server]
	public IEnumerator DelayDestroy(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (isServer)
			NetworkServer.Destroy(this.gameObject);
	}

	/* 
	* On collision with unit
	*/
	void OnCollisionEnter(Collision collision)
    {
    	if (active){
	        ContactPoint contact = collision.contacts[0];
	        if (collision.gameObject.tag == "Unit"){
		        if (collision.gameObject != null){
			    	collision.gameObject.transform.GetComponent<Unit>().TakeDamage(icicleDamage);
				}
			}
			else if (collision.gameObject.tag == "Building"){
		        if (collision.gameObject != null){
			    	collision.gameObject.transform.parent.GetComponent<Building>().TakeDamage(icicleDamage);
				}
			}
			active = false;
		    StartCoroutine(DelayDestroy(0.2f));
		}
    }
}
