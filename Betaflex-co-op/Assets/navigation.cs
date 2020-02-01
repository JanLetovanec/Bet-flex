using UnityEngine;
using System.Collections;

public class navigation : MonoBehaviour {
	GameObject[] pl; 
	Transform tf;
	Transform pltf;
	Transform etf;
	GameObject thePlayer;

	const float maxt = 5f; // lower the burden on PC
	float t;

	// Use this for initialization
	void Start () {
		pl  = GameObject.FindGameObjectsWithTag ("Player");//init
		tf = gameObject.GetComponent<Transform> ();
		etf = tf.parent.GetComponent<Transform> ();

		//Debug.Log (etf.gameObject.name);

		if (pl.Length != 0) {//if all pl dead
			pltf = pl [FindNear ()].GetComponent<Transform> ();
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (pl.Length == 0) {//all players dead
			FindNear();
			return;
		}

		if (t < 0f && FindNear()!=-1) {// locks on nearest player every maxt seconds (if any)
			t = maxt;
				pltf = pl [FindNear ()].GetComponent<Transform> ();
		} else {
			t = t - Time.deltaTime; 
		}



		if (pltf == null) return;//all players dead or anything

		tf.position = pltf.position;// sets targets to pl position

	}

	int FindNear(){//finds the nearest player
		pl  = GameObject.FindGameObjectsWithTag ("Player");
		float dist = -1f;
		int id = -1;

		for (int i = 0; i < pl.Length; i++) {
			if ((dist < 0) || ( dist >  Vector3.Distance (etf.position, pl[i].GetComponent<Transform> ().position)) ) {
				dist = Vector3.Distance (etf.position, pl [i].GetComponent<Transform>().position);
				id = i;
			}
		}
		return id;
	}
}
