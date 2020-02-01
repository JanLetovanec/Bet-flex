using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class barrel_ctrl : NetworkBehaviour 
{
	Transform tf;
	Transform [] enemyTransforms = new Transform[] {};
	Rigidbody [] enemyRigidbodies = new Rigidbody[] {};
	enemy_ctrl [] enemies = new enemy_ctrl[] {};

	void Start()
	{
		tf = gameObject.GetComponent<Transform> ();
	}

	public void AddEnemy( GameObject enemy)
	{
		Array.Resize (ref enemyTransforms, enemyTransforms.Length + 1);
		Array.Resize (ref enemyRigidbodies, enemyRigidbodies.Length + 1);
		Array.Resize (ref enemies, enemies.Length + 1);

		enemyTransforms [enemyTransforms.Length -1] = enemy.GetComponent<Transform> ();
		enemyRigidbodies [enemyRigidbodies.Length -1] = enemy.GetComponent<Rigidbody> ();
		enemies [enemies.Length -1] = enemy.GetComponent<enemy_ctrl> ();
	}

	[Command]
	void CmdExplode ()
	{
		int i;
		for (i=0; i<enemies.Length ;i++)
		{
			if (enemies [i] != null) 
			{
				Ray ray = new Ray (tf.position, enemyTransforms [i].position);
				RaycastHit rayHit;
				Physics.Raycast (ray, out rayHit);

				if (rayHit.distance < 20f) 
				{
					enemies [i].Die ();
					enemies [i].enabled = false;

					Vector3 force = (enemyTransforms [i].position - tf.position);
					force = (force * 800f) / (force.magnitude);
					enemyRigidbodies [i].AddForce (force);
					enemyRigidbodies [i].useGravity = true;
				}
			}
		}

		Destroy (gameObject);
	}

	void OnCollisionEnter (Collision col)
	{
		if (col.collider.CompareTag ("bullet") || col.collider.CompareTag ("EnemyKill")) {
			CmdExplode();
		}
	}
}
