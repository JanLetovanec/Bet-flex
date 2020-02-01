using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class pl_ctrl : NetworkBehaviour {
	[SyncVar]
	public string nick;//player info
	//[SyncVar]
	public int inventory;

	public GameObject mybullet; //spawnables
	public GameObject myChargedBullet;
	public GameObject guiobj;

	public float speed = 5f;  //options
	public float sense = 1.5f;
	public float uber = 0f;

	[SyncVar]
	public bool isUber = false;

	private float reload; //time + reload time var
	private const float maxr = 3.5f;
	private const int maxammo = 3;
	private int ammo = maxammo ;
	private const float maxRecoil = 0.2f;
	private float recoil = 0f;

	private float pushed; //PUSH STUFF
	private Vector3 pv;

	private Animator anim; //SHORTCUTS, CACHES AND OTHER VARs
	private Rigidbody rb;
	private Transform mcam;
	private Transform tf;
	private Transform gunTf;

	private bool isInit;
	private Transform tagtf;
	private Renderer playerRenderer;
	private Renderer playerGunRenderer;


	private GameObject localguy;//marks and mnarked commponets
	private Transform localtf;

	private Text ammoui;//UI (playtime)
	private Text reloadui;
	private Slider sliderui;
	private Text uberUi;
	private Slider uberSlider;

	void Start()//--------------------------------------------------------------------------------------
	{
		//Debug.Log (nick);
		isInit = false;
		inventory = 0;
		tf = gameObject.GetComponent<Transform>();
		gunTf = tf.FindChild ("Camera").Find ("gun").Find ("GunModel");

		if (isLocalPlayer)
			return;

		mcam = tf.FindChild("Camera");//enable player cam,audio
	}

	void LateStart()
	{
		isInit = true;
		playerGunRenderer = tf.FindChild ("Camera").Find("gun").Find("GunModel").GetComponent<Renderer> ();
		playerRenderer = gameObject.GetComponent<Renderer> ();

		if (!isLocalPlayer)
		{
			localguy = DetermineLocal ();//caches local player stuff
			if (tf.FindChild ("nametag").GetComponentInChildren<Text> () != null) {//finds nametag and puts nick as text
				tf.FindChild ("nametag").GetComponentInChildren<Text> ().text = nick;
			}else {Debug.Log ("TEXT not found");}
			tagtf = gameObject.GetComponent<Transform> ().FindChild("nametag").GetComponent<Transform>();
			localtf = localguy.GetComponent<Transform> ();
		}
	}//-------------------------------------------------------------------------------------------------

	GameObject DetermineLocal(){//finds local player====================================================
		GameObject[] _pls;
		_pls = GameObject.FindGameObjectsWithTag ("Player");//gets all players
		for (int i = 0; i < _pls.Length; i++) {//checks all pl. wether they are local
			if (_pls [i].GetComponent<pl_ctrl> ().CheckLocal()) {
				return _pls [i];
			}
		}
		return null;
	}//===============================================================================================

	public bool CheckLocal(){//is this obj. local player?
		if (isLocalPlayer)
			return true;
		else
			return false;
	}

	public override void OnStartLocalPlayer() {
		//Debug.Log ("problem: not on start");
		reload = 0f;//init
		pushed = 0f;
		rb = 	gameObject.GetComponent<Rigidbody> ();
		tf = gameObject.GetComponent<Transform> ();
		anim = tf.GetComponentInChildren<Animator> ();
		anim.enabled = true;

		nick = GameObject.Find ("playerinfo").GetComponent<info_handler> ().plname;//plinfo init
		CmdSyncPlinfo(nick);

		Destroy (tf.FindChild ("nametag").gameObject);//destroy your own nametag

		//disable any cam, audio here------------
		Camera[]  cam = GameObject.FindObjectsOfType<Camera>(); 
		for (int i = 0; i < cam.Length; i++) 
		{
			cam [i].enabled = false;
		}
		AudioListener[]  alis = GameObject.FindObjectsOfType<AudioListener>();                          
		for (int i = 0; i < alis.Length; i++) 
		{                                                         
			alis [i].enabled = false;                                                                    
		}	
		//---------------------------------

		//enable player cam,audio---------------------------
		mcam = tf.FindChild("Camera");
		mcam.GetComponent<Camera> ().enabled = true;
		mcam.GetComponent<AudioListener> ().enabled = true;
		//--------------------------------------------------

		Cursor.lockState = CursorLockMode.Locked;//locks coursor in centre of screen

		//GUI STUFF=============================================================
		GameObject _guiobj = Instantiate(guiobj) as GameObject;
		Text[] txt = _guiobj.GetComponentsInChildren<Text> ();
		ammoui = txt [0];
		reloadui = txt [1];
		uberUi = txt [2];
		Slider[] sliders = _guiobj.GetComponentsInChildren<Slider> ();
		sliderui = sliders [0];
		uberSlider = sliders [1];


		ammoui.text =  ammo.ToString();//set gui
		reloadui.text =  reload.ToString();
		sliderui.value = reload;
		uberUi.text = uber.ToString ();
		uberSlider.value = uber;
		//=================================================================

	}

	[Command]
	void CmdSyncPlinfo(string _nick)
	{
		nick = _nick;
		LateStart ();
	} 

	// Update is called once per frame
	void Update () {

		if (!isInit) 
		{
			LateStart ();
			return;
		}

		if (isUber) {
			playerRenderer.material.SetFloat ("smoothness", 0.98f);
		} else {
			playerRenderer.material.SetFloat ("smoothness", 0f);
		}
			
		if (!isLocalPlayer)
		{
			tagtf.LookAt(localtf.position);
			return;
		}
			
		if (isInAir () && uber < 100f)//UBER 
		{
			uber = uber + 4f*Time.deltaTime;
			uberUi.text = uber.ToString();
			uberSlider.value = uber;
		}

		if (isUber) 
		{
			Uber ();
		}

		if (recoil > 0f) {
			recoil = recoil - Time.deltaTime;}

		if (tf.position.y < -10f) { //die if falling of map
			CmdDie();
		}
			
		if (pushed > 0f) {//PUSH
			Push (pv * Time.deltaTime * 50f);}


		if (reload <= 0f) //SHOOTING with all weapons
		{
			anim.SetBool("isreloading", false);

			if (Input.GetButtonDown ("Fire1") && inventory == 0 && recoil <= 0f) 
			{	
				if (ammo > 0) 
				{
					Shoot ();
					recoil = maxRecoil;
					ammoui.text = ammo.ToString ();
				}
			} else if (Input.GetButton ("Fire1") && inventory == 1) 
			{
				Pull ();
			}
		} 
		else {
			reload = reload - Time.deltaTime;//reloading stuff
			reloadui.text =  reload.ToString();
			sliderui.value = reload;
		}

		if (Input.GetButtonDown ("Special") && uber>= 100f)
		{
			isUber = true;
			CmdUber (true);
			playerRenderer.material.color = Color.red;
		}

		if (Input.GetButtonDown ("Reload")) //reloading
		{
			anim.SetBool ("isreloading", true);//animation
			ammo = maxammo;
			reload = maxr;
			ammoui.text =  ammo.ToString();
		}

		if (Input.GetButtonDown ("Switch")) //switching weapons
		{
			if (inventory < 1) {
				inventory++;
			} else {
				inventory = 0;
			}

			if (inventory == 0) {
				playerGunRenderer.material.color = Color.white;
			} 
			else if (inventory == 1) {
				playerGunRenderer.material.color = Color.green;
			}
		}

		if (Input.GetButtonDown ("Interact") ) {//Interact
			Interact ();}

		if (Input.GetButtonDown ("Jump") ) {//Jump
			Jump ();}

		if (Input.GetButton ("Fire2") ) { //Aim
			anim.SetBool ("isAiming", true);
		} else {
			anim.SetBool ("isAiming", false);}

		Move ();
		Turn ();
	}

	//actual controls ==================================================================================================================================
	void Uber () //UBERCHARGE
	{
		if (uber <= 0f) {
			isUber = false;
			CmdUber (false);
			playerRenderer.material.color = Color.black;
			return;
		} else 
		{
			uber = uber - Time.deltaTime*5f;
			uberUi.text = uber.ToString();
			uberSlider.value = uber;
		if (Input.GetButton ("Fire1") )
			{
				Shoot ();
				pushed = 0f;
				ammo = 3;
			}
		}
	}

	[Command]
	public void CmdUber(bool _newIsUber)
	{
		isUber = _newIsUber;	
	}

	bool isInAir () //DETERMINES INAIR
	{
		Ray ray = new Ray (tf.position, Vector3.down);
		RaycastHit rayHit;
		Physics.Raycast (ray,out rayHit, 1.6f);

		if (rayHit.collider != null) {
			return false;
		} else {
			return true;}
	}

	void Jump ()
	{
		float jumpHeight = 3.5f;

		Ray ray = new Ray (tf.position, Vector3.down);
		RaycastHit rayHit;
		Physics.Raycast (ray,out rayHit, 1.6f);

		if (rayHit.collider != null)
		{
			rb.velocity = rb.velocity + Vector3.up * jumpHeight;
		}
	}

	void Turn()// turns player (look)
	{ 
		float x,y ;

		x = Input.GetAxis ("Mouse X");
		Vector3 vectorX = new Vector3 (0f, x, 0f);
		tf.Rotate (vectorX*sense); //rotates player

		y = Input.GetAxis ("Mouse Y");
		Vector3 vectorY = new Vector3 (-y, 0f, 0f);
		mcam.Rotate (vectorY*sense); // rotates camera

		Vector3 newRotation = new Vector3 (
			                      Mathf.Clamp (mcam.localEulerAngles.x, -900f, 10000f),
			                      mcam.localEulerAngles.y, 
			                      mcam.localEulerAngles.z
		                      );
		mcam.localEulerAngles = newRotation;
		Debug.Log (mcam.rotation.x);
	}

	void Move ()//move player around
	{ 
		tf.Translate(
			Vector3.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime + 
			Vector3.right * Input.GetAxis("Horizontal") * speed * Time.deltaTime);
	}

	void Interact()
	{
		Ray ray = new Ray (mcam.position, mcam.forward); 
		RaycastHit rayHit;
		Physics.Raycast (ray,out rayHit, 2.5f);

		if (rayHit.collider != null) 
		{
			if (rayHit.collider.gameObject.tag == "interact")
			{
				if (rayHit.collider.gameObject.name == "grakata")
				{
					inventory = -1;

					rayHit.collider.gameObject.GetComponent<turret_ctrl> ().Activate (gameObject);
					CmdInteractGrakata (rayHit.collider.gameObject, gameObject, 0);
				}
				
			}	
		}
	}

	[Command]
	public void CmdInteractGrakata(GameObject _grakata, GameObject _player, int option)
	{
		if (option == 0) {//ACTIVATE
			_grakata.GetComponent<turret_ctrl> ().Activate (_player);
		} else if (option == 1) {//DEACTIVATE
			_grakata.GetComponent<turret_ctrl> ().Deactivate ();
		} else if (option == 2) {//SHOOT
			_grakata.GetComponent<turret_ctrl> ().Shoot ();
		}
	}

	void Pull()
	{
		Ray pullTarget = new Ray (mcam.position, mcam.forward);
		RaycastHit pullTargetHit;
		Physics.Raycast (pullTarget, out pullTargetHit, 25f);

		if (pullTargetHit.collider != null) 
		{
			if (pullTargetHit.collider.gameObject.tag == "movable")
			{
				
				if (pullTargetHit.distance > 1.3f)
				{
					Vector3 force;
					force = (((pullTargetHit.point - mcam.position - mcam.forward*0.8f)/pullTargetHit.distance)*-15f) / (pullTargetHit.distance*0.2f);
					pullTargetHit.rigidbody.AddForce (force * Time.deltaTime * 70f);
				}
					
			}
		}
	}

	void Shoot() //shooting supervisor (triggers other functions)
	{ 
		float bFire = 70f;

		pushed = 0.1f; //init push
		pv = -mcam.forward*bFire;

		anim.SetTrigger ("Shoot");

		Ray wJump = new Ray (mcam.position, mcam.forward); //lose / keep ammo
		RaycastHit wJhit;
		Physics.Raycast (wJump,out wJhit, 2.8f);

		if (wJhit.collider == null) {
			ammo = ammo - 1;
		}

		CmdFire ();
	}

	[Command] //fire the gun
	void CmdFire(){
		float bspeed = 10f;

		Vector3 _bullpose = gunTf.position + gunTf.forward*(-0.5f); //Sets starting posistion for bullet

		if (isUber) {
			var _bullet = (GameObject)Instantiate (myChargedBullet, _bullpose, mcam.rotation);
			_bullet.GetComponent<Rigidbody> ().velocity = mcam.forward * bspeed;//give it some speed
			NetworkServer.Spawn (_bullet);//manages network stuff
			Destroy(_bullet,3f);
		} else 
		{
			var _bullet = (GameObject)Instantiate (mybullet, _bullpose, mcam.rotation);
			_bullet.GetComponent<Rigidbody> ().velocity = mcam.forward * bspeed;//give it some speed
			NetworkServer.Spawn (_bullet);//manages network stuff
			Destroy(_bullet,3f);
		}


	}

	void Push(Vector3 _pv){ //do the actual PUSH
		pushed = pushed - Time.deltaTime;
		rb.AddForce (_pv);
	}
	//=============================================================================================================================================

	[ClientRpc]
	public void RpcDie()
	{
		Destroy (gameObject);
	}

	[Command]
	public void CmdDie()
	{
		RpcDie ();
		NetworkServer.Destroy (gameObject);
		return;
	}

	void OnDestroy(){
		if(isLocalPlayer)//re-init everything on death
		{
			if (GameObject.FindObjectOfType<Canvas> ()) 
			{
				Destroy (GameObject.FindObjectOfType<Canvas> ().gameObject);//destroy ui
			}
			GameObject.Find ("defaultcam").GetComponent<Camera> ().enabled = true;//restart cam
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

}
