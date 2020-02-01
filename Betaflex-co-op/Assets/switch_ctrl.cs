using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switch_ctrl : MonoBehaviour {

	public Light[] lights;
	public door_ctrl[] doors;

	void OnTriggerEnter(Collider hit)
	{
		if (!hit.CompareTag ("movable"))
			return;

		int i;
		for (i=0; i< lights.Length; i++)
		{
			lights [i].color = Color.green;
		}

		for (i=0; i<doors.Length ;i++)
		{
			doors [i].AddKey ();
		}
	}
	void OnTriggerExit ()
	{
		int i;
		for (i=0; i<doors.Length ;i++)
		{
			doors [i].RemoveKey ();
		}
	}

}
