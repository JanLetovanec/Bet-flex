using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class enemy_ctrl : NetworkBehaviour {
	public float fforce = 3f;

	Transform targettf;
	Transform tf;
	Rigidbody rb;
	// Use this for initialization
	void Start () {
		tf = gameObject.GetComponent<Transform> ();
		rb = gameObject.GetComponent<Rigidbody> ();


		targettf = tf.FindChild("target");
	}
	
	// Update is called once per frame
	void Update () {
		Follow ();
	}

	void OnCollisionEnter( Collision col){
		GameObject _pl;
		if (col.collider.tag != null) 
		{
			if (col.collider.tag == "Player") {
				_pl = col.gameObject;
				pl_ctrl _plScript = _pl.GetComponent<pl_ctrl> ();
				if (!_plScript.isUber) {
					_plScript.CmdDie ();
				}
			} 
			else if (col.collider.tag == "EnemyKill") 
			{
				this.Die ();
			}
		}

	}

	[Server]
	void Follow() //apply force and so...follows target
	{ 
		tf.LookAt (targettf.position);
		rb.AddRelativeForce (Vector3.forward*fforce);
	}

	public void Die ()
	{
		rb.useGravity = true;
		Destroy (gameObject, 20f);
		this.enabled = false;
	}
}
