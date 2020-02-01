using UnityEngine;
using System.Collections;

public class info_handler : MonoBehaviour {

	public string plname = "";
	public GameObject GM;


	public void change_str (string _plname){
		plname = _plname;
		//Debug.Log (plname);
	}


	public void play (){
		Destroy (GameObject.Find ("entrygui"));
		var myGM = (GameObject)Instantiate (GM);

	}
}
