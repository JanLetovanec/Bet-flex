using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class door_ctrl : NetworkBehaviour {

	public int key;
	public int maxKey;

	// Use this for initialization
	void Start () 
	{
		key = 0;	
	}

	//RETURNS TRUE WHEN DOOR OPENS
	public bool AddKey()
	{
		key = key + 1;

		if (key < maxKey)
			return false;
		else 
		{
			OpenDoor ();
			return true;
		}

	}

	public void OpenDoor()
	{
		Destroy (gameObject, 1.5f);
	}

	public void RemoveKey()
	{
		key = key -1;
	}
}
