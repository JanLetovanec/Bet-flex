using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EnemyManager : NetworkBehaviour 
{
	public GameObject myenemy;
	private Transform[] spawns;
	private barrel_ctrl[] barrels;

	private const float spawnt = 10f;
	private float t = 0f;
	// Use this for initialization
	void Start () 
	{
		spawns = GetComponentsInChildren<Transform> ();
		barrels = gameObject.GetComponentsInChildren<barrel_ctrl> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!NetworkServer.active) { //not active server
			return;}

		if (t <= 0f) 
		{
			CmdSpawn (spawns [Random.Range (0, spawns.Length-1) +1].position); //FISRT EXCLUDED
			t = spawnt;
		} else 
		{
			t = t - Time.deltaTime;
		}
	}

	[Command]
	void CmdSpawn (Vector3 _pose) //SPAWN ENEMY
	{
		var _myEnemy = (GameObject)Instantiate (myenemy, _pose, gameObject.GetComponent<Transform> ().rotation);
		NetworkServer.Spawn (_myEnemy);

		int i;
		for (i = 0; i < barrels.Length; i++)
		{
			barrels [i].AddEnemy (_myEnemy);
		}
	}
}
