using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class enemyGround_ctrl : NetworkBehaviour 
{
	public float speed = 5.1f;

	private Transform targettf;
	private Transform tf;
	private Rigidbody rb;
	private bool dead;
	// Use this for initialization
	void Start () 
	{
		tf = gameObject.GetComponent<Transform> ();
		rb = gameObject.GetComponent<Rigidbody> ();
		dead = false;

		targettf = tf.FindChild("target");
	}

	// Update is called once per frame
	void Update () 
	{
		Follow ();
	}

	void OnCollisionEnter( Collision col)
	{
		GameObject _pl;
		if (col.collider.tag != null && !dead) 
		{
			if (col.collider.tag == "Player") 
			{
				_pl = col.gameObject;
				pl_ctrl _plScript = _pl.GetComponent<pl_ctrl> ();
				if (!_plScript.isUber) {
					_plScript.CmdDie ();
				}
			} 
			else if (col.collider.tag == "bullet") 
			{
				this.Die ();
			}
		}

	}

	[Server]
	void Follow() //apply force and so...follows target
	{ 
		Vector3 targetPosition = new Vector3 (targettf.position.x, 1f, targettf.position.z);
		tf.LookAt (targetPosition);
		rb.velocity = tf.forward * speed;

	}

	public void Die ()
	{
		if (isServer) {
			Destroy (gameObject, 20f);
		}
		this.enabled = false;
		dead = true;
	}
}
