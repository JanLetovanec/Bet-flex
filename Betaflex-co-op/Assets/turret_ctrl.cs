using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class turret_ctrl : NetworkBehaviour {
	public GameObject myBullet;
	 
	pl_ctrl playerScript; // Cache and shorthands
	Renderer playerGunRenderer;
	Renderer turretRenderer;
	Transform playerTransform;
	Transform turretTransform;
	Rigidbody turretRigidbody;
	Vector3 playerBase;
	bool isLocal;

	float canShoot; //recoil
	const float  maxCanShoot = 0.06f;

	float heat; //overheating
	const float maxHeat = 10f;
	bool isOverHeat;
	const float pushBack = 400f;

	// Use this for initialization
	void Start () 
	{
		turretTransform = gameObject.GetComponent<Transform> ().Find ("turret");
		turretRenderer = turretTransform.Find ("Cube (1)").GetComponent<Renderer> ();
		turretRigidbody =turretTransform.gameObject.GetComponent<Rigidbody> ();
		playerBase = gameObject.GetComponent<Transform> ().Find ("PlayerBase").position;
		canShoot = 0f;
		heat = 0f;
		isOverHeat = false;

		playerScript = null;
		playerTransform = null;
		playerGunRenderer = null;

		isLocal = false;
	}

	public void Activate (GameObject _player)
	{
		playerScript = _player.GetComponent<pl_ctrl> ();
		playerTransform = _player.GetComponent<Transform> ();
		isLocal = true;

		playerGunRenderer = playerTransform.Find ("Camera").Find ("gun").Find("GunModel").GetComponent<Renderer> ();
		playerGunRenderer.enabled = false;

		playerTransform.position = playerBase;
	}
		
	public void Deactivate ()
	{
		playerScript = null;
		playerGunRenderer.enabled = true;
		playerGunRenderer.material.color = Color.black;
		playerTransform = null;
		isLocal = false;
	}

	// Update is called once per frame
	void Update () 
	{
		turretRenderer.material.color = new Color (heat / maxHeat, 0f, 0f);

		if (canShoot > 0f && !isOverHeat) { //Measures time between shots
			canShoot = canShoot - Time.deltaTime;
		}

		if (heat > 0f && !isOverHeat)
			heat = heat - Time.deltaTime;

		if (playerTransform != null && isLocal) 
		{
			turretTransform.rotation = playerTransform.Find ("Camera").rotation;

			if (Input.GetButton ("Fire1") && (canShoot <= 0f)) {
				playerScript.CmdInteractGrakata (gameObject, null, 2);
				canShoot = maxCanShoot;
			}
			
			if 
			(
				Input.GetAxisRaw("Horizontal") != 0f ||
				Input.GetAxisRaw("Vertical") !=0 ||
				Input.GetButton("Switch") ||
				Input.GetButton("Jump") ||
				Input.GetButton("Fire2")
			)
			{
				playerScript.CmdInteractGrakata (gameObject, null, 1);
				Deactivate ();
			}
		}

		if (isOverHeat)
		{
			Vector3 force = turretTransform.forward * (-pushBack) + Random.onUnitSphere*pushBack*2f;
			turretRigidbody.AddForce (force);
			Shoot ();
		}
	}

	void OverHeat ()
	{
		Deactivate ();
		gameObject.GetComponent<Collider> ().enabled = false;
		turretTransform.gameObject.GetComponent<Collider> ().enabled = true;
		isOverHeat = true;

		//turretRigidbody.useGravity = true;
		turretRigidbody.velocity = turretTransform.forward * (-4f) + turretTransform.right * (7f);
		Destroy (gameObject, 10f);
	}

	public void Shoot()
	{
		heat = heat + Time.deltaTime*10;
		RpcGetHeat (heat);

		float bspeed = 20f;

		Vector3 bulletPosition = turretTransform.Find("BulletSpawn").position; //creates instance of bullet
		var newBullet = (GameObject)Instantiate(myBullet, bulletPosition, turretTransform.rotation);
		newBullet.GetComponent<Rigidbody>().velocity = turretTransform.forward* bspeed;//give it some speed

		NetworkServer.Spawn (newBullet);//manages network stuff
		Destroy(newBullet,3f);

		if (heat > maxHeat && !isOverHeat)
			OverHeat ();
	}

	[ClientRpc]
	public void RpcGetHeat (float newHeat)
	{
		heat = newHeat;
	}
}
